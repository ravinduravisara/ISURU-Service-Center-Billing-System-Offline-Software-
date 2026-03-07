using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ServiceStationBillingApp
{
    public partial class LoginForm : Form
    {
        public bool IsAuthenticated { get; private set; }
        public string UserRole { get; private set; } = ""; // "Admin" or "User"

        public LoginForm()
        {
            InitializeComponent();
            HookEvents();
            LoadLogoImage();
        }

        private void LoadLogoImage()
        {
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "logo.png");
                if (File.Exists(logoPath))
                {
                    byte[] bytes = File.ReadAllBytes(logoPath);
                    var ms = new System.IO.MemoryStream(bytes);
                    picLogo.Image = Image.FromStream(ms);
                }
            }
            catch
            {
                // Ignore errors; logo is optional
            }
        }

        private void HookEvents()
        {
            btnLogin.Click += BtnLogin_Click;
            btnCancel.Click += (_, __) => { this.DialogResult = DialogResult.Cancel; Close(); };
            btnAdmin.Click += (_, __) => { txtUsername.Text = Database.GetSetting("admin_username", "Isuru@6800"); txtPassword.Focus(); };
            btnOtherUser.Click += (_, __) => { txtUsername.Text = Database.GetSetting("user_username", "User@123"); txtPassword.Focus(); };
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var user = txtUsername.Text.Trim();
            var pass = txtPassword.Text;

            // Load credentials from database (falls back to defaults)
            string adminUser = Database.GetSetting("admin_username", "Isuru@6800");
            string adminPass = Database.GetSetting("admin_password", "Isuru@service");
            string userUser = Database.GetSetting("user_username", "User@123");
            string userPass = Database.GetSetting("user_password", "User@service");

            if (user == adminUser && pass == adminPass)
            {
                IsAuthenticated = true;
                UserRole = "Admin";
                this.DialogResult = DialogResult.OK;
                Close();
            }
            else if (user == userUser && pass == userPass)
            {
                IsAuthenticated = true;
                UserRole = "User";
                this.DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                IsAuthenticated = false;
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
