using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class BillingController : Controller
{
    [HttpGet]
    public IActionResult Create()
    {
        return View(new BillingViewModel { Customers = CustomerOptions(), Services = ServiceOptions() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(BillingViewModel model)
    {
        model.Items = model.Items.Where(item => !string.IsNullOrWhiteSpace(item.Service) || !string.IsNullOrWhiteSpace(item.ItemId)).ToList();
        var paymentMethod = model.PaymentMethod.Trim();
        if (!new[] { "Cash", "Card", "Cheque", "Credit" }.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.PaymentMethod), "Select a valid payment method.");
        if (model.Items.Count == 0)
            ModelState.AddModelError(nameof(model.Items), "Add at least one invoice item.");
        if (model.Discount < 0)
            ModelState.AddModelError(nameof(model.Discount), "Discount cannot be negative.");

        if (!ModelState.IsValid)
        {
            PopulateOptions(model);
            return View(model);
        }

        using var connection = Database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        var customer = FindCustomer(connection, transaction, model.CustomerId);
        if (customer is null)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "Select an existing customer.");
            PopulateOptions(model);
            return View(model);
        }

        model.CustomerName = customer.Value.Name;
        model.CustomerPhone = customer.Value.Phone;
        foreach (var item in model.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Service))
                ModelState.AddModelError(nameof(model.Items), "Select a service for each invoice row.");
            if (item.Quantity <= 0 || item.Rate < 0)
                ModelState.AddModelError(nameof(model.Items), "Each item must have a positive quantity and a valid rate.");
            item.Amount = Math.Round(item.Quantity * item.Rate, 2);
        }
        model.Subtotal = model.Items.Sum(item => item.Amount);
        if (model.Discount > model.Subtotal)
            ModelState.AddModelError(nameof(model.Discount), "Discount cannot exceed the subtotal.");
        model.TotalAmount = Math.Round(model.Subtotal - model.Discount, 2);
        if (model.TotalAmount <= 0)
            ModelState.AddModelError(nameof(model.TotalAmount), "Invoice total must be greater than zero.");
        if (!ModelState.IsValid)
        {
            PopulateOptions(model);
            return View(model);
        }

        using var vehicleCommand = connection.CreateCommand();
        vehicleCommand.Transaction = transaction;
        vehicleCommand.CommandText = @"INSERT INTO vehicles (customer_id, vehicle_no, vehicle_type)
                                       VALUES ($customer, $vehicle, $type);
                                       SELECT last_insert_rowid();";
        vehicleCommand.Parameters.AddWithValue("$customer", model.CustomerId);
        vehicleCommand.Parameters.AddWithValue("$vehicle", model.VehicleNo.Trim());
        vehicleCommand.Parameters.AddWithValue("$type", model.VehicleType.Trim());
        var vehicleId = Convert.ToInt32(vehicleCommand.ExecuteScalar());

        using var invoiceCommand = connection.CreateCommand();
        invoiceCommand.Transaction = transaction;
        invoiceCommand.CommandText = @"INSERT INTO invoices
            (bill_no, customer_id, vehicle_id, date_time, subtotal_amount, discount_amount, total_amount, payment_method, payment_status)
            VALUES ($bill, $customer, $vehicle, $date, $subtotal, $discount, $total, $method, $status);
            SELECT last_insert_rowid();";
        invoiceCommand.Parameters.AddWithValue("$bill", model.BillNo.Trim());
        invoiceCommand.Parameters.AddWithValue("$customer", model.CustomerId);
        invoiceCommand.Parameters.AddWithValue("$vehicle", vehicleId);
        invoiceCommand.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        invoiceCommand.Parameters.AddWithValue("$subtotal", model.Subtotal);
        invoiceCommand.Parameters.AddWithValue("$discount", model.Discount);
        invoiceCommand.Parameters.AddWithValue("$total", model.TotalAmount);
        invoiceCommand.Parameters.AddWithValue("$method", paymentMethod);
        invoiceCommand.Parameters.AddWithValue("$status", paymentMethod.Equals("Credit", StringComparison.OrdinalIgnoreCase) ? "Pending" : "Paid");
        var invoiceId = Convert.ToInt32(invoiceCommand.ExecuteScalar());

        foreach (var item in model.Items)
        {
            using var itemCommand = connection.CreateCommand();
            itemCommand.Transaction = transaction;
            itemCommand.CommandText = @"INSERT INTO invoice_items
                    (invoice_id, service, description, qty, rate, amount, item_id, buying_price)
                    VALUES ($invoice, $service, $description, $qty, $rate, $amount, $item_id, COALESCE((
                        SELECT buying_price FROM inventory WHERE item_id = $item_id
                        ORDER BY updated_at DESC, id DESC LIMIT 1), 0))";
            itemCommand.Parameters.AddWithValue("$invoice", invoiceId);
            itemCommand.Parameters.AddWithValue("$service", item.Service.Trim());
            itemCommand.Parameters.AddWithValue("$description", item.Description.Trim());
            itemCommand.Parameters.AddWithValue("$qty", item.Quantity);
            itemCommand.Parameters.AddWithValue("$rate", item.Rate);
            itemCommand.Parameters.AddWithValue("$amount", item.Amount);
            itemCommand.Parameters.AddWithValue("$item_id", string.IsNullOrWhiteSpace(item.ItemId) ? DBNull.Value : item.ItemId.Trim());
            itemCommand.ExecuteNonQuery();
        }
        transaction.Commit();

        TempData["Notice"] = $"Invoice {model.BillNo} created.";
        return RedirectToAction("Index", "Invoices");
    }

    private static IReadOnlyList<CustomerOption> CustomerOptions()
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, COALESCE(phone, '') FROM customers ORDER BY LOWER(name)";
        using var reader = command.ExecuteReader();
        var customers = new List<CustomerOption>();
        while (reader.Read()) customers.Add(new CustomerOption { Id = reader.GetInt32(0), Name = reader.GetString(1), Phone = reader.GetString(2) });
        return customers;
    }

    private static IReadOnlyList<ServiceOption> ServiceOptions()
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, price, COALESCE(description, '') FROM services ORDER BY LOWER(name)";
        using var reader = command.ExecuteReader();
        var services = new List<ServiceOption>();
        while (reader.Read()) services.Add(new ServiceOption { Id = reader.GetInt32(0), Name = reader.GetString(1), Price = Convert.ToDecimal(reader.GetValue(2)), Description = reader.GetString(3) });
        return services;
    }

    private static void PopulateOptions(BillingViewModel model)
    {
        model.Customers = CustomerOptions();
        model.Services = ServiceOptions();
        if (model.Items.Count == 0) model.Items.Add(new BillingLineItemViewModel());
    }

    private static (string Name, string Phone)? FindCustomer(Microsoft.Data.Sqlite.SqliteConnection connection, Microsoft.Data.Sqlite.SqliteTransaction transaction, int id)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT name, COALESCE(phone, '') FROM customers WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? (reader.GetString(0), reader.GetString(1)) : null;
    }
}