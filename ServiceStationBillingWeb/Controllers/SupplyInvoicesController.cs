using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class SupplyInvoicesController : Controller
{
    public IActionResult Index(string? search, string? paymentStatus)
    {
        var searchValue = search?.Trim() ?? string.Empty;
        var status = paymentStatus?.Trim() ?? string.Empty;
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT id, invoice_no, supplier_code, item_id, buying_price, quantity,
                                       payment_status, created_at, COALESCE(cheque_no, ''), COALESCE(branch_code, ''),
                                       COALESCE(bank_code, ''), COALESCE(value_date, ''), COALESCE(settled_at, '')
                                FROM supply_invoices
                                WHERE ($search = '' OR LOWER(invoice_no) LIKE $like OR LOWER(supplier_code) LIKE $like OR LOWER(item_id) LIKE $like)
                                  AND ($status = '' OR LOWER(TRIM(payment_status)) = LOWER($status))
                                ORDER BY created_at DESC, id DESC";
        command.Parameters.AddWithValue("$search", searchValue);
        command.Parameters.AddWithValue("$like", $"%{searchValue.ToLowerInvariant()}%");
        command.Parameters.AddWithValue("$status", status);
        using var reader = command.ExecuteReader();
        var invoices = new List<SupplyInvoiceViewModel>();
        while (reader.Read())
        {
            var price = Convert.ToDecimal(reader.GetValue(4));
            var quantity = Convert.ToDecimal(reader.GetValue(5));
            invoices.Add(new SupplyInvoiceViewModel { Id = reader.GetInt64(0), InvoiceNo = reader.GetString(1), SupplierCode = reader.GetString(2), ItemId = reader.GetString(3), BuyingPrice = price, Quantity = quantity, Total = price * quantity, PaymentStatus = reader.GetString(6), CreatedAt = reader.GetString(7), ChequeNo = reader.GetString(8), BranchCode = reader.GetString(9), BankCode = reader.GetString(10), ValueDate = reader.GetString(11), SettledAt = reader.GetString(12) });
        }
        return View(new SupplyInvoiceListViewModel { Search = searchValue, PaymentStatus = status, Invoices = invoices });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new SupplyInvoiceFormViewModel());

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public IActionResult Create(SupplyInvoiceFormViewModel model)
    {
        if (!new[] { "Cash", "Credit", "Cheque" }.Contains(model.PaymentStatus, StringComparer.OrdinalIgnoreCase)) ModelState.AddModelError(nameof(model.PaymentStatus), "Choose Cash, Credit or Cheque.");
        if (!ModelState.IsValid) return View(model);
        var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        using var connection = Database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var itemCheck = connection.CreateCommand();
            itemCheck.Transaction = transaction;
            itemCheck.CommandText = "SELECT COUNT(*) FROM inventory WHERE item_id = $item";
            itemCheck.Parameters.AddWithValue("$item", model.ItemId.Trim());
            if (Convert.ToInt32(itemCheck.ExecuteScalar()) == 0) { ModelState.AddModelError(nameof(model.ItemId), "Item ID must already exist in inventory."); transaction.Rollback(); return View(model); }

            using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = @"INSERT INTO supply_invoices (invoice_no, supplier_code, item_id, buying_price, quantity, payment_status, created_at, cheque_no, branch_code, bank_code, value_date)
                                   VALUES ($invoice, $supplier, $item, $price, $quantity, $status, $created, $cheque, $branch, $bank, $valueDate)";
            insert.Parameters.AddWithValue("$invoice", model.InvoiceNo.Trim()); insert.Parameters.AddWithValue("$supplier", model.SupplierCode.Trim()); insert.Parameters.AddWithValue("$item", model.ItemId.Trim()); insert.Parameters.AddWithValue("$price", model.BuyingPrice); insert.Parameters.AddWithValue("$quantity", model.Quantity); insert.Parameters.AddWithValue("$status", model.PaymentStatus.Trim()); insert.Parameters.AddWithValue("$created", now); insert.Parameters.AddWithValue("$cheque", model.PaymentStatus.Equals("Cheque", StringComparison.OrdinalIgnoreCase) ? model.ChequeNo.Trim() : (object)DBNull.Value); insert.Parameters.AddWithValue("$branch", model.PaymentStatus.Equals("Cheque", StringComparison.OrdinalIgnoreCase) ? model.BranchCode.Trim() : (object)DBNull.Value); insert.Parameters.AddWithValue("$bank", model.PaymentStatus.Equals("Cheque", StringComparison.OrdinalIgnoreCase) ? model.BankCode.Trim() : (object)DBNull.Value); insert.Parameters.AddWithValue("$valueDate", model.PaymentStatus.Equals("Cheque", StringComparison.OrdinalIgnoreCase) && model.ValueDate.HasValue ? model.ValueDate.Value.ToString("yyyy-MM-dd") : (object)DBNull.Value);
            insert.ExecuteNonQuery();

            using var inventory = connection.CreateCommand();
            inventory.Transaction = transaction;
            inventory.CommandText = @"UPDATE inventory SET quantity = COALESCE(quantity, 0) + $quantity, buying_price = $price, supplier_code = $supplier, payment_method = $status, updated_at = $updated WHERE item_id = $item";
            inventory.Parameters.AddWithValue("$quantity", model.Quantity); inventory.Parameters.AddWithValue("$price", model.BuyingPrice); inventory.Parameters.AddWithValue("$supplier", model.SupplierCode.Trim()); inventory.Parameters.AddWithValue("$status", model.PaymentStatus.Trim()); inventory.Parameters.AddWithValue("$updated", now); inventory.Parameters.AddWithValue("$item", model.ItemId.Trim()); inventory.ExecuteNonQuery();

            using var movement = connection.CreateCommand();
            movement.Transaction = transaction;
            movement.CommandText = "INSERT INTO stock_movements (item_id, supplier_code, quantity, unit_price, buying_price, payment_method, moved_at) VALUES ($item, $supplier, $quantity, $price, $price, $status, $moved)";
            movement.Parameters.AddWithValue("$item", model.ItemId.Trim()); movement.Parameters.AddWithValue("$supplier", model.SupplierCode.Trim()); movement.Parameters.AddWithValue("$quantity", model.Quantity); movement.Parameters.AddWithValue("$price", model.BuyingPrice); movement.Parameters.AddWithValue("$status", model.PaymentStatus.Trim()); movement.Parameters.AddWithValue("$moved", now); movement.ExecuteNonQuery();

            using var supplier = connection.CreateCommand();
            supplier.Transaction = transaction;
            supplier.CommandText = "INSERT OR IGNORE INTO suppliers (supplier_code, name, contact, cash_total, credit_total, invoice) VALUES ($code, $code, '', 0, 0, $invoice)";
            supplier.Parameters.AddWithValue("$code", model.SupplierCode.Trim()); supplier.Parameters.AddWithValue("$invoice", model.InvoiceNo.Trim()); supplier.ExecuteNonQuery();
            transaction.Commit();
        }
        catch { transaction.Rollback(); throw; }
        TempData["Notice"] = "Supply invoice recorded.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public IActionResult Settle(string? search, string? paymentStatus, long[] selectedIds)
    {
        using var connection = Database.OpenConnection();
        using var transaction = connection.BeginTransaction();
        foreach (var id in selectedIds.Distinct())
        {
            using var command = connection.CreateCommand(); command.Transaction = transaction; command.CommandText = "UPDATE supply_invoices SET settled_at = $settled WHERE id = $id AND TRIM(COALESCE(settled_at, '')) = ''"; command.Parameters.AddWithValue("$settled", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); command.Parameters.AddWithValue("$id", id); command.ExecuteNonQuery();
        }
        transaction.Commit();
        TempData["Notice"] = "Selected supply invoices marked settled.";
        return RedirectToAction(nameof(Index), new { search, paymentStatus });
    }
}