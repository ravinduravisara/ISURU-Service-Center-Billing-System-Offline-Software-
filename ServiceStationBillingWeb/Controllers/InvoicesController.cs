using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class InvoicesController : Controller
{
    public IActionResult Index(string? search, string? from, string? to, string? paymentMethod, bool creditorsOnly = false)
    {
        var fromValue = DateTime.TryParse(from, out var parsedFrom) ? parsedFrom.Date : DateTime.Today.AddMonths(-1).Date;
        var toValue = DateTime.TryParse(to, out var parsedTo) ? parsedTo.Date : DateTime.Today;
        if (toValue < fromValue) toValue = fromValue;
        var searchValue = search?.Trim() ?? string.Empty;
        var method = paymentMethod?.Trim() ?? string.Empty;
        var fromText = fromValue.ToString("yyyy-MM-dd");
        var toText = toValue.AddDays(1).ToString("yyyy-MM-dd");

        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT i.id, i.bill_no, COALESCE(c.name, ''), COALESCE(v.vehicle_no, ''),
                           i.date_time, i.subtotal_amount, i.discount_amount, i.total_amount, COALESCE(i.payment_method, ''),
                                       COALESCE(i.payment_status, ''), COALESCE(i.payment_settled_at, '')
                                FROM invoices i
                                LEFT JOIN customers c ON c.id = i.customer_id
                                LEFT JOIN vehicles v ON v.id = i.vehicle_id
                                WHERE i.date_time >= $from AND i.date_time < $to
                                  AND ($search = '' OR LOWER(i.bill_no) LIKE $like OR LOWER(c.name) LIKE $like)
                                  AND ($method = '' OR LOWER(TRIM(COALESCE(i.payment_method, ''))) = LOWER($method))
                                  AND ($creditors = 0 OR (LOWER(TRIM(COALESCE(i.payment_method, ''))) = 'credit'
                                       AND LOWER(TRIM(COALESCE(i.payment_status, ''))) <> 'paid'))
                                ORDER BY i.date_time DESC, i.id DESC";
        command.Parameters.AddWithValue("$from", fromText);
        command.Parameters.AddWithValue("$to", toText);
        command.Parameters.AddWithValue("$search", searchValue);
        command.Parameters.AddWithValue("$like", $"%{searchValue.ToLowerInvariant()}%");
        command.Parameters.AddWithValue("$method", method);
        command.Parameters.AddWithValue("$creditors", creditorsOnly ? 1 : 0);
        using var reader = command.ExecuteReader();
        var invoices = new List<InvoiceViewModel>();
        decimal cashTotal = 0, chequeTotal = 0, creditTotal = 0;
        while (reader.Read())
        {
            var invoice = new InvoiceViewModel
            {
                Id = reader.GetInt32(0),
                BillNo = reader.GetString(1),
                CustomerName = reader.GetString(2),
                VehicleNo = reader.GetString(3),
                DateTime = reader.GetString(4),
                Subtotal = Convert.ToDecimal(reader.GetValue(5)),
                Discount = Convert.ToDecimal(reader.GetValue(6)),
                TotalAmount = Convert.ToDecimal(reader.GetValue(7)),
                PaymentMethod = reader.GetString(8),
                PaymentStatus = reader.GetString(9),
                PaymentSettledAt = reader.GetString(10)
            };
            invoices.Add(invoice);
            if (invoice.PaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase)) cashTotal += invoice.TotalAmount;
            else if (invoice.PaymentMethod.Equals("Cheque", StringComparison.OrdinalIgnoreCase)) chequeTotal += invoice.TotalAmount;
            else if (invoice.PaymentMethod.Equals("Credit", StringComparison.OrdinalIgnoreCase)) creditTotal += invoice.TotalAmount;
        }
        return View(new InvoiceListViewModel { Search = searchValue, From = fromValue.ToString("yyyy-MM-dd"), To = toValue.ToString("yyyy-MM-dd"), PaymentMethod = method, CreditorsOnly = creditorsOnly, FilteredTotal = invoices.Sum(i => i.TotalAmount), CashTotal = cashTotal, ChequeTotal = chequeTotal, CreditTotal = creditTotal, Invoices = invoices });
    }

    public IActionResult Details(int id)
    {
        using var connection = Database.OpenConnection();
        using var invoiceCommand = connection.CreateCommand();
        invoiceCommand.CommandText = @"SELECT i.id, i.bill_no, COALESCE(c.name, ''), COALESCE(c.phone, ''),
                                              COALESCE(v.vehicle_no, ''), COALESCE(v.vehicle_type, ''), i.date_time,
                                              i.subtotal_amount, i.discount_amount, i.total_amount,
                                              COALESCE(i.payment_method, ''), COALESCE(i.payment_status, '')
                                       FROM invoices i
                                       LEFT JOIN customers c ON c.id = i.customer_id
                                       LEFT JOIN vehicles v ON v.id = i.vehicle_id
                                       WHERE i.id = $id";
        invoiceCommand.Parameters.AddWithValue("$id", id);
        using var reader = invoiceCommand.ExecuteReader();
        if (!reader.Read()) return NotFound();
        var invoice = new InvoiceViewModel
        {
            Id = reader.GetInt32(0), BillNo = reader.GetString(1), CustomerName = reader.GetString(2), VehicleNo = reader.GetString(4),
            DateTime = reader.GetString(6), Subtotal = Convert.ToDecimal(reader.GetValue(7)), Discount = Convert.ToDecimal(reader.GetValue(8)),
            TotalAmount = Convert.ToDecimal(reader.GetValue(9)), PaymentMethod = reader.GetString(10), PaymentStatus = reader.GetString(11)
        };
        var phone = reader.GetString(3);
        var vehicleType = reader.GetString(5);
        reader.Close();

        using var itemCommand = connection.CreateCommand();
        itemCommand.CommandText = @"SELECT COALESCE(service, ''), COALESCE(item_id, ''), COALESCE(description, ''),
                                           COALESCE(qty, 0), COALESCE(rate, 0), COALESCE(amount, 0)
                                    FROM invoice_items WHERE invoice_id = $id ORDER BY id";
        itemCommand.Parameters.AddWithValue("$id", id);
        using var itemReader = itemCommand.ExecuteReader();
        var items = new List<BillingLineItemViewModel>();
        while (itemReader.Read()) items.Add(new BillingLineItemViewModel { Service = itemReader.GetString(0), ItemId = itemReader.GetString(1), Description = itemReader.GetString(2), Quantity = Convert.ToDecimal(itemReader.GetValue(3)), Rate = Convert.ToDecimal(itemReader.GetValue(4)), Amount = Convert.ToDecimal(itemReader.GetValue(5)) });
        return View(new InvoiceDetailViewModel { Invoice = invoice, CustomerPhone = phone, VehicleType = vehicleType, Items = items });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Settle(string? search, string? from, string? to, string? paymentMethod, bool creditorsOnly, int[] selectedIds)
    {
        if (selectedIds.Length > 0)
        {
            using var connection = Database.OpenConnection();
            using var transaction = connection.BeginTransaction();
            foreach (var id in selectedIds.Distinct())
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"UPDATE invoices SET payment_status = 'Paid', payment_settled_at = $settled
                                        WHERE id = $id AND LOWER(TRIM(COALESCE(payment_method, ''))) = 'credit'
                                          AND LOWER(TRIM(COALESCE(payment_status, ''))) <> 'paid'";
                command.Parameters.AddWithValue("$settled", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
            transaction.Commit();
            TempData["Notice"] = $"{selectedIds.Distinct().Count()} credit invoice(s) marked settled.";
        }
        return RedirectToAction(nameof(Index), new { search, from, to, paymentMethod, creditorsOnly });
    }
}