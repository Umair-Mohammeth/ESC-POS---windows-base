using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace POSWinForms.Views
{
    public class InventoryForm : Form
    {
        private ListView lvProducts;

        public InventoryForm()
        {
            this.BackColor = UITheme.ColBackground;
            this.Font = UITheme.FontBody;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var pad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16), BackColor = UITheme.ColBackground };

            // ── Header row ────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "Inventory Management",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Manage stock levels, prices, and product catalogue.",
                Font = UITheme.FontBody,
                ForeColor = UITheme.ColTextMuted,
                Location = new Point(0, 28),
                AutoSize = true
            };

            // ── Toolbar ───────────────────────────────────────────────────
            var toolBar = new Panel
            {
                Location = new Point(0, 64),
                Size = new Size(820, 42),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Search box
            var searchWrap = new Panel
            {
                Location = new Point(0, 2),
                Size = new Size(280, 36),
                BackColor = Color.White
            };
            searchWrap.Paint += (s, e) =>
            {
                using var pen = new Pen(UITheme.ColBorder, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, searchWrap.Width - 1, searchWrap.Height - 1);
            };
            var lblSrchIcon = new Label { Text = "🔍", Font = new Font("Segoe UI Emoji", 10), Location = new Point(6, 7), AutoSize = true, BackColor = Color.White };
            var txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = UITheme.FontBody,
                Location = new Point(30, 8),
                Size = new Size(240, 20),
                BackColor = Color.White,
            };
            txtSearch.TextChanged += (s, e) => FilterProducts(txtSearch.Text);
            searchWrap.Controls.Add(lblSrchIcon);
            searchWrap.Controls.Add(txtSearch);

            // Add button
            var btnAdd = new Button { Text = "＋  Add Product", Location = new Point(295, 2), Size = new Size(140, 36) };
            UITheme.StylePrimary(btnAdd);
            btnAdd.Click += (s, e) => ShowAddEditPanel(null);

            // Export button
            var btnExport = new Button { Text = "⬇  Export CSV", Location = new Point(445, 2), Size = new Size(130, 36) };
            UITheme.StyleSecondary(btnExport);

            toolBar.Controls.AddRange(new Control[] { searchWrap, btnAdd, btnExport });

            // ── Summary chips ─────────────────────────────────────────────
            var chipRow = new FlowLayoutPanel
            {
                Location = new Point(0, 116),
                Size = new Size(820, 34),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            chipRow.Controls.Add(MakeChip("All Items: 24",     UITheme.ColBlue));
            chipRow.Controls.Add(MakeChip("Low Stock: 5",      UITheme.ColRed));
            chipRow.Controls.Add(MakeChip("Out of Stock: 1",   UITheme.ColOrange));
            chipRow.Controls.Add(MakeChip("Categories: 6",     UITheme.ColPurple));

            // ── Product ListView ─────────────────────────────────────────
            lvProducts = new ListView
            {
                Location = new Point(0, 160),
                Size = new Size(820, 360),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = UITheme.ColCard,
                Font = UITheme.FontBody,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lvProducts.Columns.Add("SKU",           80);
            lvProducts.Columns.Add("Product Name", 220);
            lvProducts.Columns.Add("Category",     120);
            lvProducts.Columns.Add("Stock",         80);
            lvProducts.Columns.Add("Price",         90);
            lvProducts.Columns.Add("Edit",          64);
            lvProducts.Columns.Add("Delete",        64);

            // Mock inventory data — rows with stock < 10 are highlighted
            var products = new[]
            {
                new { Sku="P001", Name="Espresso Beans 500g",    Cat="Coffee",  Stock=45, Price=18.50m },
                new { Sku="P002", Name="Whole Milk 1L",           Cat="Dairy",   Stock=12, Price= 2.10m },
                new { Sku="P003", Name="Croissants (Box 6)",      Cat="Bakery",  Stock= 4, Price=12.00m },
                new { Sku="P004", Name="Cappuccino Cups x50",     Cat="Supply",  Stock=  8, Price= 6.50m },
                new { Sku="P005", Name="Sugar Sachets x200",      Cat="Supply",  Stock=120, Price= 4.80m },
                new { Sku="P006", Name="Oat Milk 1L",             Cat="Dairy",   Stock=  3, Price= 2.90m },
                new { Sku="P007", Name="Dark Roast Beans 250g",   Cat="Coffee",  Stock=  0, Price=14.00m },
                new { Sku="P008", Name="Baguette",                Cat="Bakery",  Stock= 15, Price= 3.50m },
            };

            bool alt = false;
            foreach (var p in products)
            {
                var item = new ListViewItem(new[]
                {
                    p.Sku, p.Name, p.Cat,
                    p.Stock.ToString(),
                    $"${p.Price:F2}",
                    "✏ Edit", "🗑 Del"
                });

                item.BackColor = p.Stock == 0
                    ? UITheme.ColRed.WithAlpha(30)
                    : p.Stock < 10
                        ? UITheme.ColLowStock
                        : (alt ? UITheme.ColRowAlt : UITheme.ColCard);
                if (p.Stock < 10)
                    item.SubItems[3].ForeColor = UITheme.ColRed;
                alt = !alt;
                lvProducts.Items.Add(item);
            }

            // Column-click event handler placeholder (sorting would be done here)
            lvProducts.ColumnClick += (s, e) => SortList(e.Column);

            pad.Controls.AddRange(new Control[] { lblTitle, lblSub, toolBar, chipRow, lvProducts });
            this.Controls.Add(pad);
        }

        private Label MakeChip(string text, Color color)
        {
            return new Label
            {
                Text = text,
                Font = UITheme.FontSmall,
                ForeColor = color,
                BackColor = Color.FromArgb(20, color.R, color.G, color.B),
                AutoSize = false,
                Size = new Size(0, 26),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(10, 0, 10, 0),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void FilterProducts(string query)
        {
            foreach (ListViewItem item in lvProducts.Items)
            {
                bool match = string.IsNullOrEmpty(query) ||
                             item.SubItems[1].Text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                             item.SubItems[0].Text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                item.ForeColor = match ? UITheme.ColTextPrimary : UITheme.ColTextMuted;
            }
        }

        private void SortList(int col)
        {
            // Placeholder: in production, use ListViewItemSorter
        }

        private void ShowAddEditPanel(ListViewItem item)
        {
            // Side-drawer panel that slides in within the form
            var drawer = new Panel
            {
                Size = new Size(320, this.Size.Height),
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
                Text = item == null ? "Add Product" : "Edit Product",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(20, 20),
                AutoSize = true
            };

            var fields = new[] { "SKU", "Product Name", "Category", "Stock Qty", "Price ($)" };
            int top = 60;
            foreach (var field in fields)
            {
                var lbl = new Label { Text = field, Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(20, top), AutoSize = true };
                var tb  = new TextBox { Location = new Point(20, top + 18), Width = 278, Height = 32, BorderStyle = BorderStyle.FixedSingle, Font = UITheme.FontBody };
                drawer.Controls.Add(lbl);
                drawer.Controls.Add(tb);
                top += 66;
            }

            var btnSave = new Button { Text = "Save Product", Location = new Point(20, top + 10), Size = new Size(278, 40) };
            UITheme.StylePrimary(btnSave);
            var btnClose = new Button { Text = "Cancel", Location = new Point(20, top + 60), Size = new Size(278, 36) };
            UITheme.StyleSecondary(btnClose);
            btnClose.Click += (s, e) => { this.Controls.Remove(drawer); drawer.Dispose(); };

            drawer.Controls.Add(lblDrawerTitle);
            drawer.Controls.Add(btnSave);
            drawer.Controls.Add(btnClose);
            this.Controls.Add(drawer);
            drawer.BringToFront();
            // Animate slide-in
            var timer = new System.Windows.Forms.Timer { Interval = 10 };
            timer.Tick += (s, e) =>
            {
                if (drawer.Left > this.Width - 320)
                    drawer.Left -= 16;
                else { drawer.Left = this.Width - 320; timer.Stop(); }
            };
            timer.Start();
        }
    }
}

// Extension method to create colour with alpha
namespace POSWinForms
{
    internal static class ColorExtensions
    {
        public static System.Drawing.Color WithAlpha(this System.Drawing.Color c, int alpha)
            => System.Drawing.Color.FromArgb(alpha, c.R, c.G, c.B);
    }
}
