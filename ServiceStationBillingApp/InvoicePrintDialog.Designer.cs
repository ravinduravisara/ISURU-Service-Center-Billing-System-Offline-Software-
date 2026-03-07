using System.Drawing;
using System.Windows.Forms;

namespace ServiceStationBillingApp
{
    partial class InvoicePrintDialog
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblHeader;
        private TextBox txtHeader;
        private ListView lvItems;
        private ColumnHeader chService;
        private ColumnHeader chDescription;
        private ColumnHeader chQty;
        private ColumnHeader chRate;
        private ColumnHeader chAmount;
        private Button btnPrint;
        private Button btnExport;
        private Button btnPrintPdf;
        private Button btnClose;
        private PrintPreviewDialog previewDialog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblHeader = new Label();
            txtHeader = new TextBox();
            lvItems = new ListView();
            chService = new ColumnHeader();
            chDescription = new ColumnHeader();
            chQty = new ColumnHeader();
            chRate = new ColumnHeader();
            chAmount = new ColumnHeader();
            btnPrint = new Button();
            btnExport = new Button();
            btnPrintPdf = new Button();
            btnClose = new Button();
            previewDialog = new PrintPreviewDialog();

            SuspendLayout();

            // lblHeader
            lblHeader.AutoSize = true;
            lblHeader.Location = new Point(12, 9);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(49, 15);
            lblHeader.Text = "Invoice";

            // txtHeader
            txtHeader.Location = new Point(12, 27);
            txtHeader.Multiline = true;
            txtHeader.ReadOnly = true;
            txtHeader.ScrollBars = ScrollBars.Vertical;
            txtHeader.Size = new Size(760, 120);
            txtHeader.Name = "txtHeader";

            // lvItems
            lvItems.Location = new Point(12, 155);
            lvItems.Size = new Size(760, 260);
            lvItems.View = View.Details;
            lvItems.FullRowSelect = true;
            lvItems.GridLines = true;
            lvItems.Columns.AddRange(new ColumnHeader[] { chService, chDescription, chQty, chRate, chAmount });

            chService.Text = "Service";
            chService.Width = 160;
            chDescription.Text = "Description";
            chDescription.Width = 250;
            chQty.Text = "Qty";
            chQty.Width = 80;
            chRate.Text = "Rate";
            chRate.Width = 100;
            chAmount.Text = "Amount";
            chAmount.Width = 120;

            // btnPrint
            btnPrint.Text = "Print";
            btnPrint.Size = new Size(100, 30);
            btnPrint.Location = new Point(12, 425);
            btnPrint.Click += BtnPrint_Click;

            // btnExport
            btnExport.Text = "Export HTML";
            btnExport.Size = new Size(120, 30);
            btnExport.Location = new Point(118, 425);
            btnExport.Click += BtnExport_Click;

            // btnPrintPdf
            btnPrintPdf.Text = "Print to PDF";
            btnPrintPdf.Size = new Size(120, 30);
            btnPrintPdf.Location = new Point(244, 425);
            btnPrintPdf.Click += BtnPrintPdf_Click;

            // btnClose
            btnClose.Text = "Close";
            btnClose.Size = new Size(100, 30);
            btnClose.Location = new Point(672, 425);
            btnClose.Click += (s, e) => this.Close();

            // previewDialog
            previewDialog.AutoScrollMargin = new Size(0, 0);
            previewDialog.AutoScrollMinSize = new Size(0, 0);
            previewDialog.ClientSize = new Size(800, 600);
            previewDialog.Enabled = true;
            previewDialog.Name = "previewDialog";
            previewDialog.Visible = false;

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 471);
            Controls.Add(lblHeader);
            Controls.Add(txtHeader);
            Controls.Add(lvItems);
            Controls.Add(btnPrint);
            Controls.Add(btnExport);
            Controls.Add(btnClose);
            Controls.Add(btnPrintPdf);
            Text = "Invoice Print/Export";

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
