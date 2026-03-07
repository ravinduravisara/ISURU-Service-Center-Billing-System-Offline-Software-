using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public partial class InvoicePrintDialog : Form
    {
        private readonly long _invoiceId;
        private string _billNo = "";
        private string _dateTime = "";
        private string _customerName = "";
        private string _phone = "";
        private string _vehicleNo = "";
        private string _vehicleType = "";
        private double _totalAmount = 0;
        private readonly PrintDocument _printDoc = new PrintDocument();
        private Image? _logoImage;
        private string? _logoPath;

        // Simple company header (customize as needed)
        private readonly string _companyName = "Isuru Service Center";
        private readonly string _companyAddress = "Your Address Here";
        private readonly string _companyPhone = "+94-000-000000";

        public InvoicePrintDialog(long invoiceId)
        {
            _invoiceId = invoiceId;
            InitializeComponent();

            _printDoc.PrintPage += PrintDoc_PrintPage;

            TryLoadLogo();
            LoadInvoice();
        }

        private void TryLoadLogo()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "logo.png");
                if (File.Exists(path))
                {
                    _logoPath = path;
                    byte[] bytes = File.ReadAllBytes(path);
                    var ms = new System.IO.MemoryStream(bytes);
                    _logoImage = Image.FromStream(ms);
                }
            }
            catch { /* ignore logo issues */ }
        }

        private void LoadInvoice()
        {
            using var conn = new SqliteConnection(Database.ConnectionString);
            conn.Open();

            string headerSql = @"
                SELECT i.bill_no, i.date_time, i.total_amount,
                       c.name, c.phone,
                       v.vehicle_no, v.vehicle_type
                FROM invoices i
                JOIN customers c ON i.customer_id = c.id
                JOIN vehicles v  ON i.vehicle_id = v.id
                WHERE i.id = $iid;";

            using (var cmdHeader = conn.CreateCommand())
            {
                cmdHeader.CommandText = headerSql;
                cmdHeader.Parameters.AddWithValue("$iid", _invoiceId);

                using var reader = cmdHeader.ExecuteReader();
                if (reader.Read())
                {
                    _billNo = reader.GetString(0);
                    _dateTime = reader.GetString(1);
                    _totalAmount = reader.GetDouble(2);
                    _customerName = reader.GetString(3);
                    _phone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    _vehicleNo = reader.GetString(5);
                    _vehicleType = reader.IsDBNull(6) ? "" : reader.GetString(6);
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Bill No: {_billNo}");
            sb.AppendLine($"Date/Time: {_dateTime}");
            sb.AppendLine($"Customer: {_customerName} ({_phone})");
            sb.AppendLine($"Vehicle: {_vehicleNo} ({_vehicleType})");
            sb.AppendLine($"Total: {_totalAmount:0.00}");
            txtHeader.Text = sb.ToString();

            string itemsSql = @"
                SELECT service, description, qty, rate, amount
                FROM invoice_items
                WHERE invoice_id = $iid;";

            using (var cmdItems = conn.CreateCommand())
            {
                cmdItems.CommandText = itemsSql;
                cmdItems.Parameters.AddWithValue("$iid", _invoiceId);

                using var reader = cmdItems.ExecuteReader();
                lvItems.Items.Clear();
                while (reader.Read())
                {
                    string service = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string description = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    double qty = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
                    double rate = reader.IsDBNull(3) ? 0 : reader.GetDouble(3);
                    double amount = reader.IsDBNull(4) ? 0 : reader.GetDouble(4);

                    var item = new ListViewItem(service);
                    item.SubItems.Add(description);
                    item.SubItems.Add(qty.ToString("0.##"));
                    item.SubItems.Add(rate.ToString("0.00"));
                    item.SubItems.Add(amount.ToString("0.00"));
                    lvItems.Items.Add(item);
                }
            }
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            using var dlg = new PrintPreviewDialog();
            dlg.Document = _printDoc;
            dlg.Width = 1000;
            dlg.Height = 800;
            dlg.ShowDialog(this);
        }

        private void PrintDoc_PrintPage(object? sender, PrintPageEventArgs e)
        {
            float y = 20;
            var g = e.Graphics!;
            var font = new Font("Segoe UI", 10);
            var bold = new Font("Segoe UI", 10, FontStyle.Bold);

            // Logo and company header
            float left = 20;
            if (_logoImage != null)
            {
                // Fit logo into 90x60 box keeping aspect
                float maxW = 90, maxH = 60;
                float scale = Math.Min(maxW / _logoImage.Width, maxH / _logoImage.Height);
                float w = _logoImage.Width * scale;
                float h = _logoImage.Height * scale;
                g.DrawImage(_logoImage, left, y, w, h);
            }
            // Company text to the right of logo
            float headerX = (_logoImage != null) ? left + 100 : left;
            g.DrawString(_companyName, new Font("Segoe UI", 12, FontStyle.Bold), Brushes.Black, headerX, y);
            y += 20;
            g.DrawString(_companyAddress, font, Brushes.Black, headerX, y); y += 18;
            g.DrawString($"Tel: {_companyPhone}", font, Brushes.Black, headerX, y); y += 26;

            g.DrawLine(Pens.Black, 20, y, e.PageBounds.Width - 20, y); y += 10;
            g.DrawString("Invoice", new Font("Segoe UI", 12, FontStyle.Bold), Brushes.Black, 20, y); y += 24;
            g.DrawString($"Bill No: {_billNo}", font, Brushes.Black, 20, y); y += 20;
            g.DrawString($"Date/Time: {_dateTime}", font, Brushes.Black, 20, y); y += 20;
            g.DrawString($"Customer: {_customerName} ({_phone})", font, Brushes.Black, 20, y); y += 20;
            g.DrawString($"Vehicle: {_vehicleNo} ({_vehicleType})", font, Brushes.Black, 20, y); y += 20;

            g.DrawLine(Pens.Black, 20, y, e.PageBounds.Width - 20, y); y += 10;

            g.DrawString("Service", bold, Brushes.Black, 20, y);
            g.DrawString("Description", bold, Brushes.Black, 200, y);
            g.DrawString("Qty", bold, Brushes.Black, 530, y);
            g.DrawString("Amount", bold, Brushes.Black, 610, y);
            y += 22;

            double subtotal = 0;
            foreach (ListViewItem it in lvItems.Items)
            {
                g.DrawString(it.SubItems[0].Text, font, Brushes.Black, 20, y);
                g.DrawString(it.SubItems[1].Text, font, Brushes.Black, 200, y);
                g.DrawString(it.SubItems[2].Text, font, Brushes.Black, 530, y);
                // Skip Rate column intentionally
                g.DrawString(it.SubItems[4].Text, font, Brushes.Black, 610, y);
                if (double.TryParse(it.SubItems[4].Text, out var amt)) subtotal += amt;
                y += 20;
            }

            // Totals block aligned to the Amount column (right-aligned)
            float rightEdge = e.PageBounds.Width - 20; // same as table right
            string subText = $"Subtotal: {subtotal:0.00}";
            double discount = Math.Max(0, subtotal - _totalAmount);
            string discText = $"Discount: {discount:0.00}";
            string totText = $"Total: {_totalAmount:0.00}";

            SizeF wSub = g.MeasureString(subText, bold);
            SizeF wDisc = g.MeasureString(discText, bold);
            SizeF wTot = g.MeasureString(totText, bold);

            // Separator line above totals
            g.DrawLine(Pens.Black, 20, y, e.PageBounds.Width - 20, y);
            y += 6;
            g.DrawString(subText, bold, Brushes.Black, rightEdge - wSub.Width, y); y += 18;
            g.DrawString(discText, bold, Brushes.Black, rightEdge - wDisc.Width, y); y += 18;
            g.DrawString(totText, bold, Brushes.Black, rightEdge - wTot.Width, y); y += 18;
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = Path.Combine(documents, "ServiceStationBilling");
            Directory.CreateDirectory(folderPath);

            string fileName = $"Invoice_{_billNo}.html";
            string path = Path.Combine(folderPath, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8'><title>Invoice</title>" +
                          "<style>body{font-family:Segoe UI, Arial;max-width:800px;margin:20px;}table{border-collapse:collapse;width:100%;}th,td{border:1px solid #999;padding:6px;text-align:left;}th{background:#eee} .hdr{display:flex;gap:16px;align-items:center;margin-bottom:10px} .logo{height:60px}</style></head><body>");

            // Header with optional logo
            sb.Append("<div class='hdr'>");
            if (!string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath))
            {
                var dataUri = BuildImageDataUri(_logoPath);
                sb.Append($"<img class='logo' src='{dataUri}' alt='logo' />");
            }
            sb.Append($"<div><div style='font-weight:700;font-size:18px'>{Html(_companyName)}</div><div>{Html(_companyAddress)}</div><div>Tel: {Html(_companyPhone)}</div></div></div>");

            sb.AppendLine("<h2>Invoice</h2>");
            sb.AppendLine($"<p><strong>Bill No:</strong> {_billNo}<br><strong>Date/Time:</strong> {_dateTime}<br><strong>Customer:</strong> {_customerName} ({_phone})<br><strong>Vehicle:</strong> {_vehicleNo} ({_vehicleType})</p>");
            sb.AppendLine("<table><thead><tr><th>Service</th><th>Description</th><th>Qty</th><th>Amount</th></tr></thead><tbody>");
            double subtotal = 0;
            foreach (ListViewItem it in lvItems.Items)
            {
                sb.AppendLine($"<tr><td>{Html(it.SubItems[0].Text)}</td><td>{Html(it.SubItems[1].Text)}</td><td>{Html(it.SubItems[2].Text)}</td><td style='text-align:right'>{Html(it.SubItems[4].Text)}</td></tr>");
                if (double.TryParse(it.SubItems[4].Text, out var amt)) subtotal += amt;
            }
            double discount = Math.Max(0, subtotal - _totalAmount);
            sb.AppendLine("</tbody></table>");
            sb.AppendLine("<hr style='border:0;border-top:1px solid #333;margin:8px 0'>");
            sb.AppendLine($"<div style='width:100%;display:flex;justify-content:flex-end;margin-top:8px'>" +
                           $"<div style='text-align:right;min-width:240px'>" +
                           $"<div><strong>Subtotal:</strong> {subtotal:0.00}</div>" +
                           $"<div><strong>Discount:</strong> {discount:0.00}</div>" +
                           $"<div><strong>Total:</strong> {_totalAmount:0.00}</div>" +
                           $"</div></div></body></html>");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Exported to:\n{path}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnPrintPdf_Click(object? sender, EventArgs e)
        {
            // Find Microsoft Print to PDF
            string? pdfPrinter = null;
            foreach (string p in PrinterSettings.InstalledPrinters)
            {
                if (p.IndexOf("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    pdfPrinter = p; break;
                }
            }
            if (string.IsNullOrEmpty(pdfPrinter))
            {
                MessageBox.Show("'Microsoft Print to PDF' printer not found. Please install/enable it in Windows Features.", "Printer Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Invoice_{_billNo}.pdf"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            var ps = (PrinterSettings)_printDoc.PrinterSettings.Clone();
            ps.PrinterName = pdfPrinter;
            ps.PrintToFile = true;
            ps.PrintFileName = sfd.FileName;

            var oldController = _printDoc.PrintController;
            _printDoc.PrinterSettings = ps;
            _printDoc.PrintController = new StandardPrintController(); // no UI
            try
            {
                _printDoc.Print();
                MessageBox.Show("Saved PDF to:\n" + sfd.FileName, "Print to PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save PDF:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _printDoc.PrintController = oldController;
            }
        }

        private static string Html(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string BuildImageDataUri(string path)
        {
            try
            {
                string mime = "image/png";
                string ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".jpg" || ext == ".jpeg") mime = "image/jpeg";
                else if (ext == ".gif") mime = "image/gif";
                var bytes = File.ReadAllBytes(path);
                string b64 = Convert.ToBase64String(bytes);
                return $"data:{mime};base64,{b64}";
            }
            catch { return ""; }
        }
    }
}
