using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using POSWinForms.Models;

namespace POSWinForms.Views
{
    public class MainForm : Form
    {
        private User _user;
        private Panel sidebar;
        private Panel topBar;
        private Panel contentArea;
        private Form activeForm;
        private Button activeSidebarBtn;
        private Label lblPageTitle;

        public MainForm(User user)
        {
            _user = user;
            this.Text = "ESC-POS";
            this.Size = new Size(1200, 750);
            this.MinimumSize = new Size(960, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.ColBackground;
            this.Font = UITheme.FontBody;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // ── Sidebar ───────────────────────────────────────────────────────
            sidebar = new Panel
            {
                Width = 230,
                Dock = DockStyle.Left,
                BackColor = UITheme.ColSidebar
            };

            // Logo section
            var logoPanel = new Panel
            {
                Height = 72,
                Dock = DockStyle.Top,
                BackColor = UITheme.ColSidebar,
                Padding = new Padding(0, 0, 0, 0)
            };
            logoPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var pen = new Pen(Color.FromArgb(50, 255, 255, 255), 1);
                g.DrawLine(pen, 0, logoPanel.Height - 1, logoPanel.Width, logoPanel.Height - 1);
            };
            var lblLogoIcon = new Label
            {
                Text = "🛒",
                Font = new Font("Segoe UI Emoji", 18),
                ForeColor = UITheme.ColAccent,
                Location = new Point(18, 16),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            var lblLogoName = new Label
            {
                Text = "ESC-POS",
                Font = UITheme.FontLogo,
                ForeColor = Color.White,
                Location = new Point(56, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            logoPanel.Controls.Add(lblLogoIcon);
            logoPanel.Controls.Add(lblLogoName);
            sidebar.Controls.Add(logoPanel);

            // User info section
            var userPanel = new Panel
            {
                Height = 76,
                Location = new Point(0, 72),
                Width = 230,
                BackColor = UITheme.ColSidebarHover,
                Padding = new Padding(16, 12, 8, 8)
            };
            userPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var pen = new Pen(Color.FromArgb(50, 255, 255, 255), 1);
                g.DrawLine(pen, 0, userPanel.Height - 1, userPanel.Width, userPanel.Height - 1);
            };
            // Avatar circle
            var avatarPanel = new Panel
            {
                Size = new Size(38, 38),
                Location = new Point(16, 18),
                BackColor = Color.Transparent
            };
            avatarPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var roleCol = UITheme.RoleColour(_user.Role);
                using var brush = new SolidBrush(roleCol);
                g.FillEllipse(brush, 0, 0, 37, 37);
                string initials = _user.Name?.Length > 0 ? _user.Name[0].ToString().ToUpper() : "?";
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(initials, UITheme.FontBold10, Brushes.White, new RectangleF(0, 0, 37, 37), sf);
            };
            var lblUserName = new Label
            {
                Text = _user.Name ?? "Unknown",
                Font = UITheme.FontBold10,
                ForeColor = Color.White,
                Location = new Point(62, 14),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            var lblUserRole = new Label
            {
                Text = _user.Role ?? "—",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.RoleColour(_user.Role),
                Location = new Point(62, 34),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            userPanel.Controls.Add(avatarPanel);
            userPanel.Controls.Add(lblUserName);
            userPanel.Controls.Add(lblUserRole);
            sidebar.Controls.Add(userPanel);

            // Nav items — role-gated
            int navTop = 160;
            var role = _user.Role?.ToLower() ?? "";

            // Dashboard — visible to all
            AddSidebarButton("🏠  Dashboard", navTop, (s, e) => { lblPageTitle.Text = "Dashboard"; ShowDashboard(); });
            navTop += 50;

            // Cashier — Admin, Manager, Cashier
            if (role == "admin" || role == "manager" || role == "cashier")
            {
                AddSidebarButton("🛒  Cashier", navTop, (s, e) => { lblPageTitle.Text = "Cashier"; OpenForm(new CashierForm()); });
                navTop += 50;
            }

            // Inventory — Admin, Inventory staff
            if (role == "admin" || role == "inventory")
            {
                AddSidebarButton("📦  Inventory", navTop, (s, e) => { lblPageTitle.Text = "Inventory"; OpenForm(new InventoryForm()); });
                navTop += 50;
            }

            // Management — Admin, Manager
            if (role == "admin" || role == "manager")
            {
                AddSidebarButton("📊  Management", navTop, (s, e) => { lblPageTitle.Text = "Management"; OpenForm(new ManagerForm()); });
                navTop += 50;
            }

            // Admin Panel — Admin only
            if (role == "admin")
            {
                AddSidebarButton("⚙  Admin Panel", navTop, (s, e) => { lblPageTitle.Text = "Admin Panel"; OpenForm(new AdminForm()); });
                navTop += 50;
            }

            // Logout button — anchored to bottom
            var btnLogout = new Button
            {
                Text = "🚪  Logout",
                Location = new Point(0, sidebar.Height - 58),
                Size = new Size(230, 48),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = UITheme.FontSidebar,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                BackColor = UITheme.ColSidebar,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(229, 62, 62, 62);
            btnLogout.Click += (s, e) =>
            {
                // Closing MainForm triggers the original LoginForm to show again
                this.Close();
            };
            btnLogout.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(40, 255, 255, 255), 1);
                e.Graphics.DrawLine(pen, 0, 0, btnLogout.Width, 0);
            };
            sidebar.Controls.Add(btnLogout);
            sidebar.Resize += (s, e) => btnLogout.Location = new Point(0, sidebar.Height - 58);

            // ── Top Bar ───────────────────────────────────────────────────────
            topBar = new Panel
            {
                Height = 64,
                Dock = DockStyle.Top,
                BackColor = UITheme.ColTopBar
            };
            topBar.Paint += (s, e) =>
            {
                using var pen = new Pen(UITheme.ColBorder, 1);
                e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
            };

            lblPageTitle = new Label
            {
                Text = "Dashboard",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(20, 18),
                AutoSize = true
            };
            topBar.Controls.Add(lblPageTitle);

            var lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMM yyyy  •  HH:mm"),
                Font = UITheme.FontSmall,
                ForeColor = UITheme.ColTextMuted,
                Location = new Point(20, 42),
                AutoSize = true
            };
            topBar.Controls.Add(lblDateTime);

            // ── Content Area ──────────────────────────────────────────────────
            contentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.ColBackground,
                Padding = new Padding(24, 20, 24, 20),
                AutoScroll = true
            };

            this.Controls.Add(contentArea);
            this.Controls.Add(topBar);
            this.Controls.Add(sidebar);

            ShowDashboard();
        }

        private void AddSidebarButton(string text, int top, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Top = top,
                Left = 0,
                Width = 230,
                Height = 48,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(180, 180, 180),
                Font = UITheme.FontSidebar,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                BackColor = UITheme.ColSidebar,
                Cursor = Cursors.Hand,
                Tag = false   // not active
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = UITheme.ColSidebarHover;
            btn.Click += (s, e) =>
            {
                SetActiveSidebarButton(btn);
                onClick(s, e);
            };
            btn.Paint += (s2, e2) =>
            {
                if ((bool)btn.Tag)
                {
                    // Left accent bar
                    using var brush = new SolidBrush(UITheme.ColAccent);
                    e2.Graphics.FillRectangle(brush, 0, 0, 4, btn.Height);
                }
            };
            sidebar.Controls.Add(btn);
        }

        private void SetActiveSidebarButton(Button btn)
        {
            if (activeSidebarBtn != null)
            {
                activeSidebarBtn.Tag = false;
                activeSidebarBtn.BackColor = UITheme.ColSidebar;
                activeSidebarBtn.ForeColor = Color.FromArgb(180, 180, 180);
                activeSidebarBtn.Invalidate();
            }
            activeSidebarBtn = btn;
            btn.Tag = true;
            btn.BackColor = UITheme.ColSidebarHover;
            btn.ForeColor = Color.White;
            btn.Invalidate();
        }

        private void ShowDashboard()
        {
            contentArea.Controls.Clear();

            var sectionLbl = new Label
            {
                Text = "System Overview",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            var subLbl = new Label
            {
                Text = $"Here's what's happening today, {_user.Name}.",
                Font = UITheme.FontBody,
                ForeColor = UITheme.ColTextMuted,
                Location = new Point(0, 28),
                AutoSize = true
            };
            contentArea.Controls.Add(sectionLbl);
            contentArea.Controls.Add(subLbl);

            // Stat cards in a TableLayoutPanel for responsiveness
            var cardRow = new TableLayoutPanel
            {
                Location = new Point(0, 64),
                Size = new Size(contentArea.Width - 48, 110),
                ColumnCount = 4,
                RowCount = 1,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BackColor = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            cardRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            cardRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            contentArea.SizeChanged += (s, e) =>
                cardRow.Width = contentArea.Width - 48;

            cardRow.Controls.Add(MakeStatCard("Total Sales",  "💰", "Rs. 12,450", "↑ 12% vs yesterday", UITheme.ColGreen), 0, 0);
            cardRow.Controls.Add(MakeStatCard("Orders Today", "🧾", "142", "↑ 8 since morning", UITheme.ColBlue), 1, 0);
            cardRow.Controls.Add(MakeStatCard("Low Stock",    "⚠", "5 items", "Needs restocking", UITheme.ColRed), 2, 0);
            cardRow.Controls.Add(MakeStatCard("Active Users", "👤", "3", _user.Name + " online", UITheme.ColPurple), 3, 0);
            contentArea.Controls.Add(cardRow);

            // Recent transactions header
            var txnLabel = new Label
            {
                Text = "Recent Transactions",
                Font = UITheme.FontH3,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 196),
                AutoSize = true
            };
            contentArea.Controls.Add(txnLabel);

            // Transactions table
            var txnList = new ListView
            {
                Location = new Point(0, 224),
                Size = new Size(contentArea.Width - 48, 260),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = UITheme.ColCard,
                Font = UITheme.FontBody,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            contentArea.SizeChanged += (s, e) =>
                txnList.Width = contentArea.Width - 48;

            txnList.Columns.Add("Transaction ID", 140);
            txnList.Columns.Add("Time", 100);
            txnList.Columns.Add("Cashier", 140);
            txnList.Columns.Add("Items", 80);
            txnList.Columns.Add("Total", 110);
            txnList.Columns.Add("Status", 110);

            var rows = new[]
            {
                new[] { "#TXN-0488", "09:15 AM", "Staff_01",  "3", "Rs. 55.00",  "✔ Completed" },
                new[] { "#TXN-0487", "09:02 AM", "Staff_01",  "1", "Rs. 18.50",  "✔ Completed" },
                new[] { "#TXN-0486", "08:47 AM", "Admin",     "5", "Rs. 104.20", "✔ Completed" },
                new[] { "#TXN-0485", "08:30 AM", "Manager_X", "2", "Rs. 32.00",  "↩ Refunded"  },
                new[] { "#TXN-0484", "08:11 AM", "Staff_01",  "4", "Rs. 76.80",  "✔ Completed" },
            };
            bool alt = false;
            foreach (var r in rows)
            {
                var item = new ListViewItem(r);
                item.BackColor = alt ? UITheme.ColRowAlt : UITheme.ColCard;
                alt = !alt;
                txnList.Items.Add(item);
            }
            contentArea.Controls.Add(txnList);
        }

        private Panel MakeStatCard(string title, string icon, string value, string sub, Color accentColor)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0),
                BackColor = UITheme.ColCard,
                Padding = new Padding(0)
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Shadow
                using var shadowBrush = new SolidBrush(Color.FromArgb(14, 0, 0, 0));
                using var shadowPath = UITheme.RoundedRect(new Rectangle(3, 4, card.Width - 5, card.Height - 5), UITheme.RadiusCard);
                g.FillPath(shadowBrush, shadowPath);
                // Card bg
                using var cardBrush = new SolidBrush(UITheme.ColCard);
                using var cardPath = UITheme.RoundedRect(new Rectangle(0, 0, card.Width - 3, card.Height - 4), UITheme.RadiusCard);
                g.FillPath(cardBrush, cardPath);

                // Icon Background Circle
                using var iconBg = new SolidBrush(Color.FromArgb(20, accentColor.R, accentColor.G, accentColor.B));
                g.FillEllipse(iconBg, card.Width - 50, 12, 34, 34);

                card.Region = new System.Drawing.Region(cardPath);
            };

            var lblTitle = new Label { Text = title, Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(16, 16), AutoSize = true, BackColor = Color.Transparent };
            var lblValue = new Label { Text = value, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = UITheme.ColTextPrimary, Location = new Point(14, 34), AutoSize = true, BackColor = Color.Transparent };
            var lblSub   = new Label { Text = sub,   Font = UITheme.FontSmall,  ForeColor = accentColor, Location = new Point(16, 72), AutoSize = true, BackColor = Color.Transparent };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 12),
                ForeColor = accentColor,
                Location = new Point(card.Width - 49, 19),
                Size = new Size(32, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            card.SizeChanged += (s, e) => lblIcon.Left = card.Width - 49;

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblSub);
            card.Controls.Add(lblIcon);
            return card;
        }

        private void OpenForm(Form f)
        {
            if (activeForm != null) { activeForm.Close(); activeForm = null; }
            activeForm = f;
            f.TopLevel = false;
            f.FormBorderStyle = FormBorderStyle.None;
            f.Dock = DockStyle.Fill;
            contentArea.Controls.Clear();
            contentArea.AutoScroll = false;
            contentArea.Controls.Add(f);
            f.Show();
        }
    }
}
