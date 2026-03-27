using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using POSWinForms.Data;
using System.Data.SQLite;

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
            LoadProductsFromDatabase();
        }

        private void LoadProductsFromDatabase()
        {
            lvProducts.Items.Clear();
            try
            {
                var db = DatabaseContext.Instance;
                db.Connection.Open();
                using var cmd = db.Connection.CreateCommand();
                cmd.CommandText = "SELECT id, name, price, stock_quantity FROM products";
                using var reader = cmd.ExecuteReader();
                
                bool alt = false;
                while (reader.Read())
                {
                    string id = reader["id"].ToString();
                    string name = reader["name"].ToString();
                    decimal price = Convert.ToDecimal(reader["price"]);
                    int stock = Convert.ToInt32(reader["stock_quantity"]);

                    var item = new ListViewItem(new[]
                    {
                        $"P-{id.PadLeft(3, '0')}", // Auto SKU format
                        name,
                        "General", // Category placeholder if not in DB
                        stock.ToString(),
                        $"Rs. {price:F2}",
                        "✏ Edit", "🗑 Del"
                    });

                    item.BackColor = stock == 0
                        ? UITheme.ColRed.WithAlpha(30)
                        : stock < 10
                            ? UITheme.ColLowStock
                            : (alt ? UITheme.ColRowAlt : UITheme.ColCard);
                    
                    if (stock < 10)
                        item.SubItems[3].ForeColor = UITheme.ColRed;
                    
                    item.Tag = id; // Store ID for editing/deleting
                    lvProducts.Items.Add(item);
                    alt = !alt;
                }
                db.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
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
                Text = "Manage live stock levels and prices directly from the database.",
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

            // Refresh button
            var btnRefresh = new Button { Text = "🔄", Location = new Point(445, 2), Size = new Size(40, 36) };
            UITheme.StyleSecondary(btnRefresh);
            btnRefresh.Click += (s, e) => LoadProductsFromDatabase();

            toolBar.Controls.AddRange(new Control[] { searchWrap, btnAdd, btnRefresh });

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
            lvProducts.MouseClick += (s, e) =>
            {
                var item = lvProducts.GetItemAt(e.X, e.Y);
                if (item == null) return;
                var test = lvProducts.HitTest(e.Location);
                int col = test.Item.SubItems.IndexOf(test.SubItem);
                
                if (col == 5) ShowAddEditPanel(item.Tag.ToString()); // Edit column
                if (col == 6) DeleteProduct(item.Tag.ToString());   // Delete column
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

        private void ShowAddEditPanel(string productId)
        {
            var drawer = new Panel { Size = new Size(360, this.Height), Location = new Point(this.Width, 0), BackColor = UITheme.ColCard, Padding = new Padding(24) };
            drawer.Paint += (s, e) => { using var pen = new Pen(UITheme.ColBorder, 1); e.Graphics.DrawLine(pen, 0, 0, 0, drawer.Height); };

            var lblTitle = new Label { Text = productId == null ? "Add New Product" : "Edit Product", Font = UITheme.FontH2, ForeColor = UITheme.ColTextPrimary, Location = new Point(24, 24), AutoSize = true };
            
            int top = 80;
            var txtName = CreateField(drawer, "Product Name", ref top);
            var txtPrice = CreateField(drawer, "Unit Price (Rs.)", ref top);
            var txtStock = CreateField(drawer, "Current Stock", ref top);

            if (productId != null)
            {
                try {
                    var db = DatabaseContext.Instance; db.Connection.Open();
                    using var cmd = db.Connection.CreateCommand();
                    cmd.CommandText = "SELECT name, price, stock_quantity FROM products WHERE id=@id";
                    cmd.Parameters.AddWithValue("@id", productId);
                    using var r = cmd.ExecuteReader();
                    if (r.Read()) {
                        txtName.Text = r["name"].ToString();
                        txtPrice.Text = Convert.ToDecimal(r["price"]).ToString("F2");
                        txtStock.Text = r["stock_quantity"].ToString();
                    }
                    db.Connection.Close();
                } catch { }
            }

            var btnSave = new Button { Text = "SAVE PRODUCT", Location = new Point(24, top + 20), Size = new Size(312, 44) };
            UITheme.StylePrimary(btnSave);
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text)) return;
                try {
                    var db = DatabaseContext.Instance; db.Connection.Open();
                    using var cmd = db.Connection.CreateCommand();
                    if (productId == null)
                        cmd.CommandText = "INSERT INTO products (name, price, stock_quantity) VALUES (@n, @p, @s)";
                    else {
                        cmd.CommandText = "UPDATE products SET name=@n, price=@p, stock_quantity=@s WHERE id=@id";
                        cmd.Parameters.AddWithValue("@id", productId);
                    }
                    cmd.Parameters.AddWithValue("@n", txtName.Text);
                    cmd.Parameters.AddWithValue("@p", decimal.Parse(txtPrice.Text));
                    cmd.Parameters.AddWithValue("@s", int.Parse(txtStock.Text));
                    cmd.ExecuteNonQuery();
                    db.Connection.Close();
                    this.Controls.Remove(drawer); LoadProductsFromDatabase();
                } catch (Exception ex) { MessageBox.Show("Save failed: " + ex.Message); }
            };

            var btnCancel = new Button { Text = "Cancel", Location = new Point(24, top + 74), Size = new Size(312, 38) };
            UITheme.StyleSecondary(btnCancel);
            btnCancel.Click += (s, e) => { this.Controls.Remove(drawer); drawer.Dispose(); };

            drawer.Controls.AddRange(new Control[] { lblTitle, btnSave, btnCancel });
            this.Controls.Add(drawer); drawer.BringToFront();
            var timer = new System.Windows.Forms.Timer { Interval = 10 };
            timer.Tick += (s, e) => { if (drawer.Left > this.Width - 360) drawer.Left -= 20; else { drawer.Left = this.Width - 360; timer.Stop(); } };
            timer.Start();
        }

        private TextBox CreateField(Panel p, string label, ref int top)
        {
            p.Controls.Add(new Label { Text = label, Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(24, top), AutoSize = true });
            var tb = new TextBox { Location = new Point(24, top + 20), Width = 312, Font = UITheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(tb);
            top += 60;
            return tb;
        }

        private void DeleteProduct(string id)
        {
            if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try {
                    var db = DatabaseContext.Instance; db.Connection.Open();
                    using var cmd = db.Connection.CreateCommand();
                    cmd.CommandText = "DELETE FROM products WHERE id=@id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    db.Connection.Close();
                    LoadProductsFromDatabase();
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
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
