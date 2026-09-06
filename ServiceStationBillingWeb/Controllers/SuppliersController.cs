using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    public IActionResult Index(string? search)
    {
        SyncTotals();
        var value = search?.Trim() ?? string.Empty;
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT COALESCE(supplier_code, ''), COALESCE(name, ''), COALESCE(contact, ''),
                                       COALESCE(cash_total, 0), COALESCE(credit_total, 0), COALESCE(cheque_total, 0), COALESCE(invoice, '')
                                FROM suppliers
                                WHERE $search = '' OR LOWER(COALESCE(supplier_code, '')) LIKE $like
                                   OR LOWER(COALESCE(name, '')) LIKE $like OR LOWER(COALESCE(contact, '')) LIKE $like
                                ORDER BY LOWER(name)";
        command.Parameters.AddWithValue("$search", value);
        command.Parameters.AddWithValue("$like", $"%{value.ToLowerInvariant()}%");
        using var reader = command.ExecuteReader();
        var suppliers = new List<SupplierViewModel>();
        while (reader.Read())
        {
            suppliers.Add(new SupplierViewModel
            {
                SupplierCode = reader.GetString(0), Name = reader.GetString(1), Contact = reader.GetString(2),
                CashTotal = Convert.ToDecimal(reader.GetValue(3)), CreditTotal = Convert.ToDecimal(reader.GetValue(4)),
                ChequeTotal = Convert.ToDecimal(reader.GetValue(5)), Invoice = reader.GetString(6)
            });
        }
        return View(new SupplierListViewModel { Search = value, Suppliers = suppliers });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View("Form", new SupplierFormViewModel { SupplierCode = NextSupplierCode() });

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public IActionResult Create(SupplierFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        Save(model, null);
        TempData["Notice"] = "Supplier added.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Edit(string id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(supplier_code, ''), COALESCE(name, ''), COALESCE(contact, ''), COALESCE(invoice, '') FROM suppliers WHERE supplier_code = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return NotFound();
        return View("Form", new SupplierFormViewModel { OriginalSupplierCode = reader.GetString(0), SupplierCode = reader.GetString(0), Name = reader.GetString(1), Contact = reader.GetString(2), Invoice = reader.GetString(3) });
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public IActionResult Edit(SupplierFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        Save(model, model.OriginalSupplierCode);
        TempData["Notice"] = "Supplier updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public IActionResult Delete(string id)
    {
        using var connection = Database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        foreach (var table in new[] { "supply_invoices", "stock_movements" })
        {
            using var related = connection.CreateCommand();
            related.Transaction = transaction;
            related.CommandText = $"DELETE FROM {table} WHERE supplier_code = $id";
            related.Parameters.AddWithValue("$id", id);
            related.ExecuteNonQuery();
        }
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM suppliers WHERE supplier_code = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
        transaction.Commit();
        TempData["Notice"] = "Supplier deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public IActionResult UpdateInvoice(string id, string? invoice, string? search)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE suppliers SET invoice = $invoice WHERE supplier_code = $id";
        command.Parameters.AddWithValue("$invoice", invoice?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
        return RedirectToAction(nameof(Index), new { search });
    }

    private static void Save(SupplierFormViewModel model, string? originalCode)
    {
        using var connection = Database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = originalCode is null
            ? "INSERT INTO suppliers (supplier_code, name, contact, cash_total, credit_total, cheque_total, invoice) VALUES ($code, $name, $contact, 0, 0, 0, $invoice)"
            : "UPDATE suppliers SET supplier_code = $code, name = $name, contact = $contact, invoice = $invoice WHERE supplier_code = $original";
        command.Parameters.AddWithValue("$code", model.SupplierCode.Trim());
        command.Parameters.AddWithValue("$name", model.Name.Trim());
        command.Parameters.AddWithValue("$contact", model.Contact.Trim());
        command.Parameters.AddWithValue("$invoice", model.Invoice.Trim());
        if (originalCode is not null) command.Parameters.AddWithValue("$original", originalCode);
        command.ExecuteNonQuery();
        if (originalCode is not null && !string.Equals(originalCode, model.SupplierCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            foreach (var table in new[] { "inventory", "stock_movements", "supply_invoices" })
            {
                using var related = connection.CreateCommand();
                related.Transaction = transaction;
                related.CommandText = $"UPDATE {table} SET supplier_code = $code WHERE supplier_code = $original";
                related.Parameters.AddWithValue("$code", model.SupplierCode.Trim());
                related.Parameters.AddWithValue("$original", originalCode);
                related.ExecuteNonQuery();
            }
        }
        transaction.Commit();
    }

    private static string NextSupplierCode()
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT supplier_code FROM suppliers WHERE supplier_code LIKE 'SUP-%' ORDER BY supplier_code DESC LIMIT 1";
        var last = command.ExecuteScalar() as string;
        var next = last is not null && int.TryParse(last[4..], out var number) ? number + 1 : 1;
        return $"SUP-{next:D4}";
    }

    private static void SyncTotals()
    {
        using var connection = Database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        using (var ensure = connection.CreateCommand())
        {
            ensure.Transaction = transaction;
            ensure.CommandText = @"INSERT OR IGNORE INTO suppliers (supplier_code, name) SELECT DISTINCT TRIM(supplier_code), TRIM(supplier_code) FROM inventory WHERE TRIM(COALESCE(supplier_code, '')) <> ''";
            ensure.ExecuteNonQuery();
        }
        using (var reset = connection.CreateCommand())
        {
            reset.Transaction = transaction;
            reset.CommandText = "UPDATE suppliers SET cash_total = 0, credit_total = 0, cheque_total = 0";
            reset.ExecuteNonQuery();
        }
        using var totals = connection.CreateCommand();
        totals.Transaction = transaction;
        totals.CommandText = @"UPDATE suppliers SET cash_total = COALESCE((SELECT SUM(quantity * COALESCE(buying_price, 0)) FROM inventory WHERE supplier_code = suppliers.supplier_code AND LOWER(TRIM(COALESCE(payment_method, ''))) NOT LIKE '%credit%' AND LOWER(TRIM(COALESCE(payment_method, ''))) NOT LIKE '%cheque%'), 0), credit_total = COALESCE((SELECT SUM(quantity * COALESCE(buying_price, 0)) FROM inventory WHERE supplier_code = suppliers.supplier_code AND LOWER(TRIM(COALESCE(payment_method, ''))) LIKE '%credit%'), 0), cheque_total = COALESCE((SELECT SUM(quantity * COALESCE(buying_price, 0)) FROM inventory WHERE supplier_code = suppliers.supplier_code AND LOWER(TRIM(COALESCE(payment_method, ''))) LIKE '%cheque%'), 0)";
        totals.ExecuteNonQuery();
        transaction.Commit();
    }
}