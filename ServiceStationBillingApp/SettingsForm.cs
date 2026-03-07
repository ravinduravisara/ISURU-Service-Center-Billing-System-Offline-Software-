using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ServiceStationBillingApp
{
    public class SettingsForm : Form
    {
        // Logo section
        private Label lblLogoTitle = null!;
        private PictureBox picLogoPreview = null!;
        private Button btnBrowseLogo = null!;
        private Button btnSaveLogo = null!;
        private string? _pendingLogoPath;

        // Credentials section
        private Label lblCredTitle = null!;

        private Label lblAdminUser = null!;
        private TextBox txtAdminUser = null!;
        private Label lblAdminPass = null!;
        private TextBox txtAdminPass = null!;

        private Label lblUserUser = null!;
        private TextBox txtUserUser = null!;
        private Label lblUserPass = null!;
        private TextBox txtUserPass = null!;

        private Button btnSaveCredentials = null!;

        public SettingsForm()
        {
            InitializeComponent();
            LoadCurrentLogo();
            LoadCurrentCredentials();
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(480, 520);
            this.MinimumSize = new Size(460, 500);
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 15;
            int leftLabel = 20;
            int leftInput = 170;
            int inputWidth = 260;

            // ========== LOGO SECTION ==========
            lblLogoTitle = new Label
            {
                Text = "Logo",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(leftLabel, y),
                AutoSize = true
            };
            y += 30;

            picLogoPreview = new PictureBox
            {
                Location = new Point(leftLabel, y),
                Size = new Size(120, 80),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            btnBrowseLogo = new Button
            {
                Text = "Browse...",
                Location = new Point(160, y + 10),
                Size = new Size(100, 28)
            };
            btnBrowseLogo.Click += BtnBrowseLogo_Click;

            btnSaveLogo = new Button
            {
                Text = "Update Logo",
                Location = new Point(160, y + 45),
                Size = new Size(100, 28),
                Enabled = false
            };
            btnSaveLogo.Click += BtnSaveLogo_Click;

            y += 100;

            // Separator
            var sep1 = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(leftLabel, y),
                Size = new Size(420, 2),
                AutoSize = false
            };
            y += 15;

            // ========== CREDENTIALS SECTION ==========
            lblCredTitle = new Label
            {
                Text = "Login Credentials",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(leftLabel, y),
                AutoSize = true
            };
            y += 35;

            // Admin username
            lblAdminUser = new Label
            {
                Text = "Admin Username:",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(leftLabel, y + 3),
                AutoSize = true
            };
            txtAdminUser = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(leftInput, y),
                Size = new Size(inputWidth, 24)
            };
            y += 34;

            // Admin password
            lblAdminPass = new Label
            {
                Text = "Admin Password:",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(leftLabel, y + 3),
                AutoSize = true
            };
            txtAdminPass = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(leftInput, y),
                Size = new Size(inputWidth, 24)
            };
            y += 45;

            // Separator
            var sep2 = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(leftLabel, y),
                Size = new Size(420, 2),
                AutoSize = false
            };
            y += 15;

            // User username
            lblUserUser = new Label
            {
                Text = "User Username:",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(leftLabel, y + 3),
                AutoSize = true
            };
            txtUserUser = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(leftInput, y),
                Size = new Size(inputWidth, 24)
            };
            y += 34;

            // User password
            lblUserPass = new Label
            {
                Text = "User Password:",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(leftLabel, y + 3),
                AutoSize = true
            };
            txtUserPass = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(leftInput, y),
                Size = new Size(inputWidth, 24)
            };
            y += 45;

            // Save credentials button
            btnSaveCredentials = new Button
            {
                Text = "Save Credentials",
                Location = new Point(leftInput, y),
                Size = new Size(140, 32),
                BackColor = Color.FromArgb(30, 136, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSaveCredentials.FlatAppearance.BorderSize = 0;
            btnSaveCredentials.Click += BtnSaveCredentials_Click;

            this.Controls.AddRange(new Control[]
            {
                lblLogoTitle, picLogoPreview, btnBrowseLogo, btnSaveLogo,
                sep1,
                lblCredTitle,
                lblAdminUser, txtAdminUser, lblAdminPass, txtAdminPass,
                sep2,
                lblUserUser, txtUserUser, lblUserPass, txtUserPass,
                btnSaveCredentials
            });
        }

        // ========== LOGO ==========

        private Image? LoadImageFromFile(string path)
        {
            // Load image into a MemoryStream so the file is never locked
            byte[] bytes = File.ReadAllBytes(path);
            var ms = new MemoryStream(bytes);
            return Image.FromStream(ms); // ms must NOT be disposed; Image owns it
        }

        private void LoadCurrentLogo()
        {
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "logo.png");
                if (File.Exists(logoPath))
                {
                    picLogoPreview.Image?.Dispose();
                    picLogoPreview.Image = LoadImageFromFile(logoPath);
                }
            }
            catch { }
        }

        private void BtnBrowseLogo_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select Logo Image",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*"
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _pendingLogoPath = dlg.FileName;
                try
                {
                    picLogoPreview.Image?.Dispose();
                    picLogoPreview.Image = LoadImageFromFile(_pendingLogoPath);
                    btnSaveLogo.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _pendingLogoPath = null;
                    btnSaveLogo.Enabled = false;
                }
            }
        }

        private void BtnSaveLogo_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_pendingLogoPath) || !File.Exists(_pendingLogoPath))
            {
                MessageBox.Show("No image selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string assetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
                if (!Directory.Exists(assetsDir))
                    Directory.CreateDirectory(assetsDir);

                string destPath = Path.Combine(assetsDir, "logo.png");

                // Dispose current image so the file is not locked
                picLogoPreview.Image?.Dispose();
                picLogoPreview.Image = null;

                File.Copy(_pendingLogoPath, destPath, overwrite: true);

                // Reload preview (file-lock-free)
                picLogoPreview.Image = LoadImageFromFile(destPath);

                _pendingLogoPath = null;
                btnSaveLogo.Enabled = false;

                MessageBox.Show("Logo updated successfully!\nIt will be used in invoices and the login screen.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update logo:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== CREDENTIALS ==========

        private void LoadCurrentCredentials()
        {
            try
            {
                Database.EnsureSettingsTable();
                using var conn = Database.OpenConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT key, value FROM app_settings WHERE key IN ('admin_username','admin_password','user_username','user_password')";
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string key = rdr.GetString(0);
                    string val = rdr.GetString(1);
                    switch (key)
                    {
                        case "admin_username": txtAdminUser.Text = val; break;
                        case "admin_password": txtAdminPass.Text = val; break;
                        case "user_username": txtUserUser.Text = val; break;
                        case "user_password": txtUserPass.Text = val; break;
                    }
                }
            }
            catch
            {
                // Load defaults if DB not ready
                txtAdminUser.Text = "Isuru@6800";
                txtAdminPass.Text = "Isuru@service";
                txtUserUser.Text = "User@123";
                txtUserPass.Text = "User@service";
            }
        }

        private void BtnSaveCredentials_Click(object? sender, EventArgs e)
        {
            string adminUser = txtAdminUser.Text.Trim();
            string adminPass = txtAdminPass.Text;
            string userUser = txtUserUser.Text.Trim();
            string userPass = txtUserPass.Text;

            if (string.IsNullOrWhiteSpace(adminUser) || string.IsNullOrEmpty(adminPass))
            {
                MessageBox.Show("Admin username and password cannot be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(userUser) || string.IsNullOrEmpty(userPass))
            {
                MessageBox.Show("User username and password cannot be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Database.EnsureSettingsTable();
                using var conn = Database.OpenConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                UpsertSetting(conn, "admin_username", adminUser);
                UpsertSetting(conn, "admin_password", adminPass);
                UpsertSetting(conn, "user_username", userUser);
                UpsertSetting(conn, "user_password", userPass);

                tx.Commit();
                MessageBox.Show("Credentials updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save credentials:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void UpsertSetting(SqliteConnection conn, string key, string value)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO app_settings (key, value) VALUES ($k, $v)
                                ON CONFLICT(key) DO UPDATE SET value = excluded.value";
            cmd.Parameters.AddWithValue("$k", key);
            cmd.Parameters.AddWithValue("$v", value);
            cmd.ExecuteNonQuery();
        }
    }
}
