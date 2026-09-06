using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStationBillingApp;
using ServiceStationBillingWeb.Models;

namespace ServiceStationBillingWeb.Controllers;

[Authorize]
public class ProfitController : Controller
{
    public IActionResult Index(string? from, string? to)
    {
        var fromDate = DateTime.TryParse(from, out var parsedFrom) ? parsedFrom.Date : DateTime.Today.AddMonths(-1).Date;
        var toDate = DateTime.TryParse(to, out var parsedTo) ? parsedTo.Date : DateTime.Today;
        if (toDate < fromDate) toDate = fromDate;
        var fromText = fromDate.ToString("yyyy-MM-dd");
        var toText = toDate.AddDays(1).ToString("yyyy-MM-dd");
        var lines = new List<ProfitLineViewModel>();
        decimal serviceRevenue = 0, partsRevenue = 0, cogs = 0;

        using var connection = Database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT i.date_time, i.bill_no, COALESCE(ii.service, ''), COALESCE(ii.item_id, ''),
                                       COALESCE(ii.description, ''), COALESCE(ii.qty, 0), COALESCE(ii.amount, 0),
                                       COALESCE(ii.buying_price, 0)
                                FROM invoice_items ii
                                INNER JOIN invoices i ON i.id = ii.invoice_id
                                WHERE i.date_time >= $from AND i.date_time < $to
                                ORDER BY i.date_time DESC, i.bill_no";
        command.Parameters.AddWithValue("$from", fromText);
        command.Parameters.AddWithValue("$to", toText);
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var itemId = reader.GetString(3);
                var quantity = Convert.ToDecimal(reader.GetValue(5));
                var revenue = Convert.ToDecimal(reader.GetValue(6));
                var cost = string.IsNullOrWhiteSpace(itemId) ? 0 : quantity * Convert.ToDecimal(reader.GetValue(7));
                var line = new ProfitLineViewModel
                {
                    Date = reader.GetString(0), InvoiceNo = reader.GetString(1), Description = string.IsNullOrWhiteSpace(reader.GetString(2)) ? reader.GetString(4) : reader.GetString(2),
                    ItemId = itemId, Quantity = quantity, Revenue = revenue, Cost = cost, Profit = revenue - cost
                };
                lines.Add(line);
                if (string.IsNullOrWhiteSpace(itemId)) serviceRevenue += revenue;
                else { partsRevenue += revenue; cogs += cost; }
            }
        }

        using var purchasesCommand = connection.CreateCommand();
        purchasesCommand.CommandText = "SELECT COALESCE(SUM(buying_price * quantity), 0) FROM supply_invoices WHERE created_at >= $from AND created_at < $to";
        purchasesCommand.Parameters.AddWithValue("$from", fromText);
        purchasesCommand.Parameters.AddWithValue("$to", toText);
        var purchases = Convert.ToDecimal(purchasesCommand.ExecuteScalar() ?? 0);
        var grossProfit = serviceRevenue + partsRevenue - cogs;
        return View(new ProfitStatementViewModel { From = fromDate.ToString("yyyy-MM-dd"), To = toDate.ToString("yyyy-MM-dd"), ServiceRevenue = serviceRevenue, PartsRevenue = partsRevenue, Cogs = cogs, GrossProfit = grossProfit, Purchases = purchases, NetProfit = grossProfit - purchases, Lines = lines });
    }

    public IActionResult Print(string? from, string? to) => View("Print", IndexModel(from, to));

    private ProfitStatementViewModel IndexModel(string? from, string? to)
    {
        var result = Index(from, to);
        return (result as ViewResult)?.Model as ProfitStatementViewModel ?? new ProfitStatementViewModel();
    }
}