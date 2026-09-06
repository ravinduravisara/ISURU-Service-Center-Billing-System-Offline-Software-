using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class ServicesController : Controller
{
    public IActionResult Index(string? search)
    {
        var value = search?.Trim() ?? string.Empty;
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT id, category, name, price, COALESCE(description, '')
                                FROM services
                                WHERE $search = '' OR LOWER(category) LIKE $like OR LOWER(name) LIKE $like OR LOWER(COALESCE(description, '')) LIKE $like
                                ORDER BY LOWER(category), LOWER(name)";
        command.Parameters.AddWithValue("$search", value);
        command.Parameters.AddWithValue("$like", $"%{value.ToLowerInvariant()}%");

        using var reader = command.ExecuteReader();
        var services = new List<ServiceViewModel>();
        while (reader.Read())
        {
            services.Add(new ServiceViewModel
            {
                Id = reader.GetInt32(0),
                Category = reader.GetString(1),
                Name = reader.GetString(2),
                Price = Convert.ToDecimal(reader.GetValue(3)),
                Description = reader.GetString(4)
            });
        }

        return View(new ServiceListViewModel { Search = value, Services = services });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View("Form", new ServiceFormViewModel());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ServiceFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        Save(model, null);
        TempData["Notice"] = "Service added.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Edit(int id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, category, name, price, COALESCE(description, '') FROM services WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return NotFound();
        return View("Form", new ServiceFormViewModel
        {
            Id = reader.GetInt32(0),
            Category = reader.GetString(1),
            Name = reader.GetString(2),
            Price = Convert.ToDecimal(reader.GetValue(3)),
            Description = reader.GetString(4)
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ServiceFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        Save(model, model.Id);
        TempData["Notice"] = "Service updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM services WHERE id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
        TempData["Notice"] = "Service deleted.";
        return RedirectToAction(nameof(Index));
    }

    private static void Save(ServiceFormViewModel model, int? id)
    {
        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = id is null
            ? "INSERT INTO services (category, name, price, description) VALUES ($category, $name, $price, $description)"
            : "UPDATE services SET category = $category, name = $name, price = $price, description = $description WHERE id = $id";
        command.Parameters.AddWithValue("$category", model.Category.Trim());
        command.Parameters.AddWithValue("$name", model.Name.Trim());
        command.Parameters.AddWithValue("$price", model.Price);
        command.Parameters.AddWithValue("$description", model.Description.Trim());
        if (id is not null) command.Parameters.AddWithValue("$id", id.Value);
        command.ExecuteNonQuery();
    }
}