using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class CustomersController : Controller
{
    public IActionResult Index(string? search)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT c.id, c.name, COALESCE(c.phone, ''),
                                       (SELECT COUNT(1) FROM vehicles v WHERE v.customer_id = c.id)
                                FROM customers c
                                WHERE $search = '' OR LOWER(c.name) LIKE $like OR COALESCE(c.phone, '') LIKE $like
                                ORDER BY LOWER(c.name)";
        var value = search?.Trim() ?? string.Empty;
        command.Parameters.AddWithValue("$search", value);
        command.Parameters.AddWithValue("$like", $"%{value.ToLowerInvariant()}%");

        using var reader = command.ExecuteReader();
        var customers = new List<CustomerViewModel>();
        while (reader.Read())
        {
            customers.Add(new CustomerViewModel
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Phone = reader.GetString(2),
                Vehicles = reader.GetInt32(3)
            });
        }

        return View(new CustomerListViewModel { Search = value, Customers = customers });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View("Form", new CustomerFormViewModel());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CustomerFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        Save(model, null);
        TempData["Notice"] = "Customer added.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Edit(int id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, COALESCE(phone, '') FROM customers WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return NotFound();
        return View("Form", new CustomerFormViewModel
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Phone = reader.GetString(2)
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(CustomerFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        Save(model, model.Id);
        TempData["Notice"] = "Customer updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM customers WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
        TempData["Notice"] = "Customer deleted.";
        return RedirectToAction(nameof(Index));
    }

    private static void Save(CustomerFormViewModel model, int? id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = id is null
            ? "INSERT INTO customers (name, phone) VALUES ($name, $phone)"
            : "UPDATE customers SET name = $name, phone = $phone WHERE id = $id";
        command.Parameters.AddWithValue("$name", model.Name.Trim());
        command.Parameters.AddWithValue("$phone", model.Phone.Trim());
        if (id is not null) command.Parameters.AddWithValue("$id", id.Value);
        command.ExecuteNonQuery();
    }
}