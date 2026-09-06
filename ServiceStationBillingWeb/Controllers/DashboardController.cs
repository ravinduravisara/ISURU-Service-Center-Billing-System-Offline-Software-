using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        using var connection = Database.OpenConnection();
        var model = new DashboardViewModel
        {
            Customers = Count(connection, "customers"),
            Vehicles = Count(connection, "vehicles"),
            Invoices = Count(connection, "invoices"),
            InventoryItems = Count(connection, "inventory"),
            Revenue = ScalarDecimal(connection, "SELECT COALESCE(SUM(total_amount), 0) FROM invoices"),
            RecentInvoices = Recent(connection)
        };

        return View(model);
    }

    private static int Count(Microsoft.Data.Sqlite.SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {table}";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static decimal ScalarDecimal(Microsoft.Data.Sqlite.SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    private static IReadOnlyList<RecentInvoice> Recent(Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT i.bill_no, COALESCE(c.name, 'Walk-in customer'),
                                       i.date_time, i.total_amount, COALESCE(i.payment_status, '')
                                FROM invoices i
                                LEFT JOIN customers c ON c.id = i.customer_id
                                ORDER BY i.id DESC LIMIT 8";
        using var reader = command.ExecuteReader();
        var invoices = new List<RecentInvoice>();
        while (reader.Read())
        {
            invoices.Add(new RecentInvoice
            {
                BillNo = reader.GetString(0),
                CustomerName = reader.GetString(1),
                DateTime = reader.GetString(2),
                TotalAmount = reader.GetDecimal(3),
                PaymentStatus = reader.GetString(4)
            });
        }
        return invoices;
    }
}