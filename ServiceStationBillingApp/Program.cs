namespace ServiceStationBillingApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        // Ensure database file and tables exist before any form uses it
        Database.Ensure();

        // Maintenance flag to purge all suppliers-related data
        var args = Environment.GetCommandLineArgs();
        foreach (var a in args)
        {
            if (string.Equals(a, "--purge-suppliers", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Database.PurgeSuppliersData();
                    Console.WriteLine("Suppliers data purged successfully.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to purge suppliers data: {ex.Message}");
                    Environment.ExitCode = 1;
                }
                return;
            }
            if (string.Equals(a, "--purge-all", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Database.PurgeAllData();
                    Console.WriteLine("All database data purged successfully.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to purge all data: {ex.Message}");
                    Environment.ExitCode = 1;
                }
                return;
            }
            if (string.Equals(a, "--db-counts", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var conn = Database.OpenConnection();
                    using var cmd = conn.CreateCommand();
                    string[] tables = new[] { "customers", "vehicles", "invoices", "invoice_items", "inventory", "suppliers", "stock_movements" };
                    foreach (var t in tables)
                    {
                        cmd.CommandText = $"SELECT COUNT(*) FROM {t}";
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        Console.WriteLine($"{t}: {count}");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to read counts: {ex.Message}");
                    Environment.ExitCode = 1;
                }
                return;
            }
        }
        using var login = new LoginForm();
        var result = login.ShowDialog();
        if (result == DialogResult.OK && login.IsAuthenticated)
        {
            Application.Run(new BillingForm(login.UserRole));
        }
        else
        {
            // Exit if not authenticated
            Application.Exit();
        }


    }    
}