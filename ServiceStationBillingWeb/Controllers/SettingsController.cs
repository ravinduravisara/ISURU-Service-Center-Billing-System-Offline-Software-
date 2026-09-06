using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    public IActionResult Index() => View(new SettingsViewModel
    {
        AdminUsername = Database.GetSetting("admin_username"), AdminPassword = Database.GetSetting("admin_password"),
        UserUsername = Database.GetSetting("user_username"), UserPassword = Database.GetSetting("user_password")
    });

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Index(SettingsViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        Database.EnsureSettingsTable();
        using var connection = Database.OpenConnection();
        foreach (var setting in new[] { ("admin_username", model.AdminUsername), ("admin_password", model.AdminPassword), ("user_username", model.UserUsername), ("user_password", model.UserPassword) })
        {
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO app_settings (key, value) VALUES ($key, $value) ON CONFLICT(key) DO UPDATE SET value = excluded.value";
            command.Parameters.AddWithValue("$key", setting.Item1);
            command.Parameters.AddWithValue("$value", setting.Item2.Trim());
            command.ExecuteNonQuery();
        }
        TempData["Notice"] = "Settings updated.";
        return RedirectToAction(nameof(Index));
    }
}