using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace ServiceStationBillingApp
{
    public class ProfitStatementForm : Form
    {
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnGenerate;
        private Button btnExportPdf;
        private DataGridView dgvStatement;
        private Label lblTitle;
        private Label lblFrom;
        private Label lblTo;
        private Panel panelSummary;
        private Label lblSalesRevenue;
        private Label lblCOGS;
        private Label lblGrossProfit;
        private Label lblServiceRevenue;
        private Label lblPartsRevenue;
        private Label lblNetProfit;
        private Label lblTotalPurchases;
        private Label lblNetProfitAfterPurchases;

        public ProfitStatementForm()
        {
            InitializeComponent();
            // Default: current month
            dtpFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpTo.Value = DateTime.Now;
            GenerateStatement();
        }

        private void InitializeComponent()
        {
            this.Text = "Profit Statement";
            this.Size = new Size(850, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "Profit Statement",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 71, 161),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // Date range controls
            lblFrom = new Label { Text = "From:", Location = new Point(20, 55), AutoSize = true };
            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(65, 52),
                Size = new Size(130, 25)
            };

            lblTo = new Label { Text = "To:", Location = new Point(210, 55), AutoSize = true };
            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(240, 52),
                Size = new Size(130, 25)
            };

            btnGenerate = new Button
            {
                Text = "Generate",
                Location = new Point(390, 50),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(13, 71, 161),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += BtnGenerate_Click;

            btnExportPdf = new Button
            {
                Text = "Export PDF",
                Location = new Point(490, 50),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(56, 142, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportPdf.FlatAppearance.BorderSize = 0;
            btnExportPdf.Click += BtnExportPdf_Click;

            // Detail grid
            dgvStatement = new DataGridView
            {
                Location = new Point(20, 90),
                Size = new Size(795, 320),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvStatement.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 220, 255);
            dgvStatement.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvStatement.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(13, 71, 161);
            dgvStatement.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStatement.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvStatement.EnableHeadersVisualStyles = false;

            // Summary panel
            panelSummary = new Panel
            {
                Location = new Point(20, 420),
                Size = new Size(795, 210),
                BackColor = Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblServiceRevenue = new Label
            {
                Location = new Point(15, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            lblPartsRevenue = new Label
            {
                Location = new Point(15, 38),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };

            lblSalesRevenue = new Label
            {
                Location = new Point(15, 64),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            lblCOGS = new Label
            {
                Location = new Point(15, 94),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(198, 40, 40)
            };

            lblGrossProfit = new Label
            {
                Location = new Point(15, 124),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            lblNetProfit = new Label
            {
                Location = new Point(400, 124),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };

            lblTotalPurchases = new Label
            {
                Location = new Point(15, 154),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(198, 40, 40)
            };

            lblNetProfitAfterPurchases = new Label
            {
                Location = new Point(400, 154),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            panelSummary.Controls.AddRange(new Control[] {
                lblServiceRevenue, lblPartsRevenue, lblSalesRevenue,
                lblCOGS, lblGrossProfit, lblNetProfit,
                lblTotalPurchases, lblNetProfitAfterPurchases
            });

            this.Controls.AddRange(new Control[] {
                lblTitle, lblFrom, dtpFrom, lblTo, dtpTo, btnGenerate, btnExportPdf,
                dgvStatement, panelSummary
            });
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            GenerateStatement();
        }

        private void GenerateStatement()
        {
            var fromDate = dtpFrom.Value.Date.ToString("yyyy-MM-dd");
            var toDate = dtpTo.Value.Date.AddDays(1).ToString("yyyy-MM-dd"); // inclusive

            var dt = new DataTable();
            dt.Columns.Add("Date");
            dt.Columns.Add("Invoice");
            dt.Columns.Add("Service");
            dt.Columns.Add("Item ID");
            dt.Columns.Add("Description");
            dt.Columns.Add("Qty", typeof(double));
            dt.Columns.Add("Selling Price", typeof(double));
            dt.Columns.Add("Buying Price", typeof(double));
            dt.Columns.Add("Revenue", typeof(double));
            dt.Columns.Add("Cost", typeof(double));
            dt.Columns.Add("Profit", typeof(double));

            double totalServiceRevenue = 0;
            double totalPartsRevenue = 0;
            double totalCOGS = 0;
            double totalPurchases = 0;

            try
            {
                using var conn = Database.OpenConnection();
                using var cmd = conn.CreateCommand();
                // Use ii.buying_price which is snapshotted at sale time for accurate COGS
                cmd.CommandText = @"
                    SELECT
                        i.date_time,
                        i.bill_no,
                        ii.service,
                        ii.item_id,
                        ii.description,
                        COALESCE(ii.qty, 0),
                        COALESCE(ii.rate, 0),
                        COALESCE(ii.amount, 0),
                        COALESCE(ii.buying_price, 0)
                    FROM invoice_items ii
                    INNER JOIN invoices i ON i.id = ii.invoice_id
                    WHERE i.date_time >= $from AND i.date_time < $to
                    ORDER BY i.date_time DESC, i.bill_no;";
                cmd.Parameters.AddWithValue("$from", fromDate);
                cmd.Parameters.AddWithValue("$to", toDate);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var dateTime = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    var billNo = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var service = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var itemId = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var description = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    var qty = reader.IsDBNull(5) ? 0.0 : reader.GetDouble(5);
                    var sellingPrice = reader.IsDBNull(6) ? 0.0 : reader.GetDouble(6);
                    var revenue = reader.IsDBNull(7) ? 0.0 : reader.GetDouble(7);
                    var buyingPrice = reader.IsDBNull(8) ? 0.0 : reader.GetDouble(8);

                    bool hasInventoryItem = !string.IsNullOrWhiteSpace(itemId);
                    double cost = hasInventoryItem ? qty * buyingPrice : 0.0;
                    double profit = revenue - cost;

                    // Format date for display
                    string displayDate = dateTime;
                    if (DateTime.TryParse(dateTime, out var parsed))
                        displayDate = parsed.ToString("yyyy-MM-dd");

                    dt.Rows.Add(displayDate, billNo, service, itemId, description,
                        qty, sellingPrice, hasInventoryItem ? buyingPrice : (object)DBNull.Value,
                        revenue, hasInventoryItem ? cost : (object)DBNull.Value, profit);

                    if (hasInventoryItem)
                    {
                        totalPartsRevenue += revenue;
                        totalCOGS += cost;
                    }
                    else
                    {
                        totalServiceRevenue += revenue;
                    }
                }

                // Query total purchases from supply_invoices for the period
                using var purchCmd = conn.CreateCommand();
                purchCmd.CommandText = @"
                    SELECT COALESCE(SUM(buying_price * quantity), 0)
                    FROM supply_invoices
                    WHERE created_at >= $from AND created_at < $to;";
                purchCmd.Parameters.AddWithValue("$from", fromDate);
                purchCmd.Parameters.AddWithValue("$to", toDate);
                var purchResult = purchCmd.ExecuteScalar();
                totalPurchases = purchResult != null && purchResult != DBNull.Value
                    ? Convert.ToDouble(purchResult) : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating profit statement:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            dgvStatement.DataSource = dt;

            // Format currency columns
            foreach (var colName in new[] { "Selling Price", "Buying Price", "Revenue", "Cost", "Profit" })
            {
                if (dgvStatement.Columns.Contains(colName))
                    dgvStatement.Columns[colName].DefaultCellStyle.Format = "N2";
            }

            if (dgvStatement.Columns.Contains("Qty"))
                dgvStatement.Columns["Qty"].DefaultCellStyle.Format = "N0";

            // Color the Profit column
            dgvStatement.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && dgvStatement.Columns[e.ColumnIndex].Name == "Profit"
                    && e.Value is double profitVal)
                {
                    e.CellStyle.ForeColor = profitVal >= 0 ? Color.FromArgb(27, 94, 32) : Color.FromArgb(198, 40, 40);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            };

            double totalRevenue = totalServiceRevenue + totalPartsRevenue;
            double grossProfit = totalRevenue - totalCOGS;

            lblServiceRevenue.Text = $"Service Revenue (Labour):    Rs. {totalServiceRevenue:N2}";
            lblPartsRevenue.Text = $"Parts / Inventory Revenue:    Rs. {totalPartsRevenue:N2}";
            lblSalesRevenue.Text = $"Total Sales Revenue:             Rs. {totalRevenue:N2}";
            lblCOGS.Text = $"Less: Cost of Goods Sold:     Rs. {totalCOGS:N2}";
            lblGrossProfit.Text = $"Gross Profit:  Rs. {grossProfit:N2}";
            lblGrossProfit.ForeColor = grossProfit >= 0 ? Color.FromArgb(27, 94, 32) : Color.FromArgb(198, 40, 40);

            double profitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;
            lblNetProfit.Text = $"Profit Margin:  {profitMargin:N1}%";
            lblNetProfit.ForeColor = profitMargin >= 0 ? Color.FromArgb(27, 94, 32) : Color.FromArgb(198, 40, 40);

            lblTotalPurchases.Text = $"Total Purchases (Supply Invoices):  Rs. {totalPurchases:N2}";

            double netAfterPurchases = grossProfit - totalPurchases;
            lblNetProfitAfterPurchases.Text = $"Net Profit (After Purchases):  Rs. {netAfterPurchases:N2}";
            lblNetProfitAfterPurchases.ForeColor = netAfterPurchases >= 0 ? Color.FromArgb(27, 94, 32) : Color.FromArgb(198, 40, 40);
        }

        private void BtnExportPdf_Click(object? sender, EventArgs e)
        {
            if (dgvStatement.DataSource == null || dgvStatement.Rows.Count == 0)
            {
                MessageBox.Show("Please generate the statement first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Export Profit Statement as PDF",
                Filter = "PDF Files|*.pdf",
                FileName = $"ProfitStatement_{dtpFrom.Value:yyyy-MM-dd}_to_{dtpTo.Value:yyyy-MM-dd}.pdf"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                // Collect rows from the grid's DataTable
                var dt = (DataTable)dgvStatement.DataSource;

                // Read summary values from labels
                string serviceRevText = lblServiceRevenue.Text;
                string partsRevText = lblPartsRevenue.Text;
                string salesRevText = lblSalesRevenue.Text;
                string cogsText = lblCOGS.Text;
                string grossProfitText = lblGrossProfit.Text;
                string netProfitText = lblNetProfit.Text;
                string totalPurchasesText = lblTotalPurchases.Text;
                string netAfterPurchasesText = lblNetProfitAfterPurchases.Text;

                string dateRange = $"{dtpFrom.Value:yyyy-MM-dd} to {dtpTo.Value:yyyy-MM-dd}";

                // Optional logo
                byte[]? logoBytes = null;
                try
                {
                    string logoPath = Path.Combine(AppContext.BaseDirectory, "assets", "logo.png");
                    if (File.Exists(logoPath))
                        logoBytes = File.ReadAllBytes(logoPath);
                }
                catch { }

                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(25);

                        page.Header().Element(header =>
                        {
                            header.Column(col =>
                            {
                                col.Item().Row(row =>
                                {
                                    if (logoBytes != null)
                                        row.ConstantItem(60).Image(logoBytes);
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("Isuru Service Center").SemiBold().FontSize(18);
                                        inner.Item().Text("Profit Statement").SemiBold().FontSize(14);
                                        inner.Item().Text($"Period: {dateRange}").FontSize(10).FontColor("#666");
                                        inner.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}").FontSize(10).FontColor("#666");
                                    });
                                });
                                col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#CCCCCC");
                            });
                        });

                        page.Content().PaddingTop(10).Column(content =>
                        {
                            // Data table
                            content.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);  // Date
                                    columns.RelativeColumn(2);  // Invoice
                                    columns.RelativeColumn(2);  // Service
                                    columns.RelativeColumn(1.5f); // Item ID
                                    columns.RelativeColumn(3);  // Description
                                    columns.RelativeColumn(1);  // Qty
                                    columns.RelativeColumn(1.5f); // Selling Price
                                    columns.RelativeColumn(1.5f); // Buying Price
                                    columns.RelativeColumn(1.5f); // Revenue
                                    columns.RelativeColumn(1.5f); // Cost
                                    columns.RelativeColumn(1.5f); // Profit
                                });

                                table.Header(hdr =>
                                {
                                    hdr.Cell().Element(CellHeader).Text("Date");
                                    hdr.Cell().Element(CellHeader).Text("Invoice");
                                    hdr.Cell().Element(CellHeader).Text("Service");
                                    hdr.Cell().Element(CellHeader).Text("Item ID");
                                    hdr.Cell().Element(CellHeader).Text("Description");
                                    hdr.Cell().Element(CellHeader).AlignRight().Text("Qty");
                                    hdr.Cell().Element(CellHeader).AlignRight().Text("Selling");
                                    hdr.Cell().Element(CellHeader).AlignRight().Text("Buying");
                                    hdr.Cell().Element(CellHeader).AlignRight().Text("Revenue");
                                    hdr.Cell().Element(CellHeader).AlignRight().Text("Cost");
                                    hdr.Cell().Element(CellHeader).AlignRight().Text("Profit");

                                    static QuestPDF.Infrastructure.IContainer CellHeader(QuestPDF.Infrastructure.IContainer c)
                                    {
                                        return c.PaddingVertical(5).PaddingHorizontal(3)
                                            .Background("#0D47A1")
                                            .DefaultTextStyle(x => x.FontColor("#FFFFFF").FontSize(8).SemiBold());
                                    }
                                });

                                foreach (DataRow row in dt.Rows)
                                {
                                    table.Cell().Element(CellBody).Text(row["Date"]?.ToString() ?? "").FontSize(8);
                                    table.Cell().Element(CellBody).Text(row["Invoice"]?.ToString() ?? "").FontSize(8);
                                    table.Cell().Element(CellBody).Text(row["Service"]?.ToString() ?? "").FontSize(8);
                                    table.Cell().Element(CellBody).Text(row["Item ID"]?.ToString() ?? "").FontSize(8);
                                    table.Cell().Element(CellBody).Text(row["Description"]?.ToString() ?? "").FontSize(8);

                                    var qtyVal = row["Qty"] is double q ? q.ToString("N0") : "";
                                    var sellingVal = row["Selling Price"] is double sp ? sp.ToString("N2") : "";
                                    var buyingVal = row["Buying Price"] is double bp ? bp.ToString("N2") : "";
                                    var revVal = row["Revenue"] is double rv ? rv.ToString("N2") : "";
                                    var costVal = row["Cost"] is double cv ? cv.ToString("N2") : "";
                                    var profitVal = row["Profit"] is double pv ? pv : 0.0;
                                    string profitStr = row["Profit"] is double ? profitVal.ToString("N2") : "";
                                    string profitColor = profitVal >= 0 ? "#1B5E20" : "#C62828";

                                    table.Cell().Element(CellBody).AlignRight().Text(qtyVal).FontSize(8);
                                    table.Cell().Element(CellBody).AlignRight().Text(sellingVal).FontSize(8);
                                    table.Cell().Element(CellBody).AlignRight().Text(buyingVal).FontSize(8);
                                    table.Cell().Element(CellBody).AlignRight().Text(revVal).FontSize(8);
                                    table.Cell().Element(CellBody).AlignRight().Text(costVal).FontSize(8);
                                    table.Cell().Element(CellBody).AlignRight().Text(profitStr).FontSize(8).FontColor(profitColor);
                                }

                                static QuestPDF.Infrastructure.IContainer CellBody(QuestPDF.Infrastructure.IContainer c)
                                {
                                    return c.PaddingVertical(3).PaddingHorizontal(3)
                                        .BorderBottom(0.5f).BorderColor("#E0E0E0");
                                }
                            });

                            // Summary section
                            content.Item().PaddingTop(15).Element(summaryContainer =>
                            {
                                summaryContainer.Background("#F5F5F5").Padding(15).Column(col =>
                                {
                                    col.Item().Text("Summary").SemiBold().FontSize(14).FontColor("#0D47A1");
                                    col.Item().PaddingTop(8).Text(serviceRevText).FontSize(10);
                                    col.Item().PaddingTop(4).Text(partsRevText).FontSize(10);
                                    col.Item().PaddingTop(4).Text(salesRevText).SemiBold().FontSize(11);
                                    col.Item().PaddingTop(6).Text(cogsText).FontSize(10).FontColor("#C62828");
                                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#CCCCCC");
                                    col.Item().PaddingTop(8).Row(row =>
                                    {
                                        row.RelativeItem().Text(grossProfitText).SemiBold().FontSize(12);
                                        row.RelativeItem().AlignRight().Text(netProfitText).SemiBold().FontSize(12);
                                    });
                                    col.Item().PaddingTop(6).Text(totalPurchasesText).FontSize(10).FontColor("#C62828");
                                    col.Item().PaddingTop(4).Text(netAfterPurchasesText).SemiBold().FontSize(11);
                                });
                            });
                        });

                        page.Footer().AlignRight().Text(t =>
                        {
                            t.Span("Page ");
                            t.CurrentPageNumber();
                            t.Span(" / ");
                            t.TotalPages();
                        });
                    });
                });

                doc.GeneratePdf(sfd.FileName);
                MessageBox.Show("Profit statement exported to PDF successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export PDF:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
