using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using POSWinForms.Data;
using System.Data.SQLite;

namespace POSWinForms.Views
{
    public class AdminForm : Form
    {
        private ListView lvUsers;

        public AdminForm()
        {
            this.BackColor = UITheme.ColBackground;
            this.Font = UITheme.FontBody;
            InitializeComponents();
            LoadUsersFromDatabase();
        }

        private void LoadUsersFromDatabase()
        {
            lvUsers.Items.Clear();
            try
            {
                var db = DatabaseContext.Instance;
                db.Connection.Open();
                using var cmd = db.Connection.CreateCommand();
                cmd.CommandText = "SELECT name, role, pin FROM users ORDER BY role ASC";
                using var reader = cmd.ExecuteReader();
                
                bool alt = false;
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    string role = reader["role"].ToString();
                    string pin = "****"; // Mask PIN for security
                    string lastLogin = "—"; // Placeholder

                    string initials = name.Length > 0 ? name[0].ToString().ToUpper() : "?";
                    var item = new ListViewItem(new[] { initials, name, role, pin, lastLogin, "✏ Edit", "🗑 Del" });
                    item.BackColor = alt ? UITheme.ColRowAlt : UITheme.ColCard;
                    item.SubItems[2].ForeColor = UITheme.RoleColour(role);
                    lvUsers.Items.Add(item);
                    alt = !alt;
                }
                db.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }

        private void InitializeComponents()
        {
            var pad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16), BackColor = UITheme.ColBackground };

            // ── Title ─────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "Admin Panel — User Management",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Manage system users and access roles from the database.",
                Font = UITheme.FontBody,
                ForeColor = UITheme.ColTextMuted,
                Location = new Point(0, 28),
                AutoSize = true
            };

            // ── Toolbar ───────────────────────────────────────────────────
            var btnAddUser = new Button { Text = "＋  Add User", Location = new Point(0, 64), Size = new Size(130, 36) };
            UITheme.StylePrimary(btnAddUser);
            btnAddUser.Click += (s, e) => ShowUserPanel(null);

            var btnRefresh = new Button { Text = "🔄  Refresh", Location = new Point(140, 64), Size = new Size(100, 36) };
            UITheme.StyleSecondary(btnRefresh);
            btnRefresh.Click += (s, e) => LoadUsersFromDatabase();

            // ── User ListView ─────────────────────────────────────────────
            lvUsers = new ListView
            {
                Location = new Point(0, 114),
                Size = new Size(880, 380),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = UITheme.ColCard,
                Font = UITheme.FontBody,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lvUsers.Columns.Add("",           46);   // Avatar placeholder
            lvUsers.Columns.Add("Name",       160);
            lvUsers.Columns.Add("Role",       120);
            lvUsers.Columns.Add("PIN",         80);
            lvUsers.Columns.Add("Last Login", 180);
            lvUsers.Columns.Add("Edit",        64);
            lvUsers.Columns.Add("Delete",      64);

            // Column header section label
            var colHeader = new Panel
            {
                Location = new Point(0, 108),
                Size = new Size(880, 6),
                BackColor = Color.Transparent
            };
            colHeader.Paint += (s, e) =>
            {
                using var b = new SolidBrush(UITheme.ColAccent);
                e.Graphics.FillRectangle(b, 0, 5, 880, 1);
            };

            pad.Controls.AddRange(new Control[] { lblTitle, lblSub, btnAddUser, btnRefresh, colHeader, lvUsers });
            this.Controls.Add(pad);
        }

        private void ShowUserPanel(ListViewItem editItem)
        {
            // Inline side-drawer panel
            var drawer = new Panel
            {
                Size = new Size(340, this.Height),
                Location = new Point(this.Width, 0),
                BackColor = UITheme.ColCard,
                Padding = new Padding(20)
            };
            drawer.Paint += (s, e) =>
            {
                using var pen = new Pen(UITheme.ColBorder, 1);
                e.Graphics.DrawLine(pen, 0, 0, 0, drawer.Height);
            };

            var lblDrawerTitle = new Label
            {
                Text = editItem == null ? "Add New User" : "Edit User",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(20, 20),
                AutoSize = true
            };
            drawer.Controls.Add(lblDrawerTitle);

            // Name
            var lblName = new Label { Text = "Full Name", Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(20, 64), AutoSize = true };
            var txtName = new TextBox { Location = new Point(20, 82), Width = 298, BorderStyle = BorderStyle.FixedSingle, Font = UITheme.FontBody };
            // Role
            var lblRole = new Label { Text = "Role", Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(20, 124), AutoSize = true };
            var cbRole  = new ComboBox { Location = new Point(20, 142), Width = 298, DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.FontBody };
            cbRole.Items.AddRange(new[] { "Admin", "Manager", "Cashier", "Inventory" });
            cbRole.SelectedIndex = 2;
            UITheme.StyleComboBox(cbRole);
            // PIN
            var lblPin = new Label { Text = "Set PIN (4–6 digits)", Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(20, 184), AutoSize = true };
            var txtPin = new TextBox { Location = new Point(20, 202), Width = 298, BorderStyle = BorderStyle.FixedSingle, Font = UITheme.FontBody, UseSystemPasswordChar = true };

            // Save button
            var btnSave = new Button { Text = "Save User", Location = new Point(20, 260), Size = new Size(298, 40) };
            UITheme.StylePrimary(btnSave);
            btnSave.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtName.Text) && cbRole.SelectedItem != null && txtPin.Text.Length >= 4)
                {
                    // In production: persist to DB
                    string initials = txtName.Text.Trim()[0].ToString().ToUpper();
                    var newItem = new ListViewItem(new[]
                    {
                        initials,
                        txtName.Text.Trim(),
                        cbRole.SelectedItem.ToString(),
                        "****",
                        DateTime.Now.ToString("yyyy-MM-dd  hh:mm tt"),
                        "✏ Edit", "🗑 Del"
                    });
                    newItem.SubItems[2].ForeColor = UITheme.RoleColour(cbRole.SelectedItem.ToString());
                    lvUsers.Items.Add(newItem);
                    this.Controls.Remove(drawer);
                    drawer.Dispose();
                }
            };

            var btnCancel = new Button { Text = "Cancel", Location = new Point(20, 310), Size = new Size(298, 36) };
            UITheme.StyleSecondary(btnCancel);
            btnCancel.Click += (s, e) => { this.Controls.Remove(drawer); drawer.Dispose(); };

            drawer.Controls.AddRange(new Control[] { lblName, txtName, lblRole, cbRole, lblPin, txtPin, btnSave, btnCancel });
            this.Controls.Add(drawer);
            drawer.BringToFront();

            // Slide-in animation
            var timer = new System.Windows.Forms.Timer { Interval = 10 };
            timer.Tick += (s, e) =>
            {
                if (drawer.Left > this.Width - 340) drawer.Left -= 18;
                else { drawer.Left = this.Width - 340; timer.Stop(); }
            };
            timer.Start();
        }
    }
}
