using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class InventoryController : Controller
{
    public IActionResult Index(string? search)
    {
        var value = search?.Trim() ?? string.Empty;
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT id, COALESCE(item_id, ''), item_name, COALESCE(brand, ''), quantity,
                                       unit_price, buying_price, updated_at, COALESCE(supplier_code, ''), COALESCE(payment_method, '')
                                FROM inventory
                                WHERE $search = '' OR LOWER(COALESCE(item_id, '')) LIKE $like OR LOWER(item_name) LIKE $like
                                   OR LOWER(COALESCE(brand, '')) LIKE $like OR LOWER(COALESCE(supplier_code, '')) LIKE $like
                                ORDER BY LOWER(item_name)";
        command.Parameters.AddWithValue("$search", value);
        command.Parameters.AddWithValue("$like", $"%{value.ToLowerInvariant()}%");

        using var reader = command.ExecuteReader();
        var items = new List<InventoryViewModel>();
        while (reader.Read())
        {
            items.Add(new InventoryViewModel
            {
                Id = reader.GetInt32(0),
                ItemId = reader.GetString(1),
                ItemName = reader.GetString(2),
                Brand = reader.GetString(3),
                Quantity = Convert.ToDecimal(reader.GetValue(4)),
                UnitPrice = Convert.ToDecimal(reader.GetValue(5)),
                BuyingPrice = Convert.ToDecimal(reader.GetValue(6)),
                UpdatedAt = reader.GetString(7),
                SupplierCode = reader.GetString(8),
                PaymentMethod = reader.GetString(9)
            });
        }

        return View(new InventoryListViewModel { Search = value, Items = items });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View("Form", new InventoryFormViewModel());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(InventoryFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        Save(model, null);
        TempData["Notice"] = "Inventory item added.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Edit(int id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT id, COALESCE(item_id, ''), item_name, COALESCE(brand, ''), quantity,
                                       unit_price, buying_price, COALESCE(supplier_code, ''), COALESCE(payment_method, '')
                                FROM inventory WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return NotFound();
        return View("Form", new InventoryFormViewModel
        {
            Id = reader.GetInt32(0),
            ItemId = reader.GetString(1),
            ItemName = reader.GetString(2),
            Brand = reader.GetString(3),
            Quantity = Convert.ToDecimal(reader.GetValue(4)),
            UnitPrice = Convert.ToDecimal(reader.GetValue(5)),
            BuyingPrice = Convert.ToDecimal(reader.GetValue(6)),
            SupplierCode = reader.GetString(7),
            PaymentMethod = reader.GetString(8)
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(InventoryFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        Save(model, model.Id);
        TempData["Notice"] = "Inventory item updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM inventory WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
        TempData["Notice"] = "Inventory item deleted.";
        return RedirectToAction(nameof(Index));
    }

    private static void Save(InventoryFormViewModel model, int? id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = id is null
            ? @"INSERT INTO inventory (item_id, item_name, brand, quantity, unit_price, buying_price, updated_at, supplier_code, payment_method)
               VALUES ($item_id, $item_name, $brand, $quantity, $unit_price, $buying_price, $updated_at, $supplier_code, $payment_method)"
            : @"UPDATE inventory SET item_id = $item_id, item_name = $item_name, brand = $brand, quantity = $quantity,
                                      unit_price = $unit_price, buying_price = $buying_price, updated_at = $updated_at,
                                      supplier_code = $supplier_code, payment_method = $payment_method WHERE id = $id";
        command.Parameters.AddWithValue("$item_id", string.IsNullOrWhiteSpace(model.ItemId) ? DBNull.Value : model.ItemId.Trim());
        command.Parameters.AddWithValue("$item_name", model.ItemName.Trim());
        command.Parameters.AddWithValue("$brand", model.Brand.Trim());
        command.Parameters.AddWithValue("$quantity", model.Quantity);
        command.Parameters.AddWithValue("$unit_price", model.UnitPrice);
        command.Parameters.AddWithValue("$buying_price", model.BuyingPrice);
        command.Parameters.AddWithValue("$updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("$supplier_code", string.IsNullOrWhiteSpace(model.SupplierCode) ? DBNull.Value : model.SupplierCode.Trim());
        command.Parameters.AddWithValue("$payment_method", model.PaymentMethod.Trim());
        if (id is not null) command.Parameters.AddWithValue("$id", id.Value);
        command.ExecuteNonQuery();
    }
}