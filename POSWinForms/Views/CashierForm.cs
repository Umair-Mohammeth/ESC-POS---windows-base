using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using POSWinForms.Data;
using System.Data.SQLite;

namespace POSWinForms.Views
{
    public class CashierForm : Form
    {
        private ListView lvCart;
        private Label lblSubtotal, lblTax, lblTotal;
        private RadioButton rbCash, rbCard;
        private TextBox txtTender, txtCardNum, txtExpiry, txtCvv;
        private Label lblChange;
        private Label lblUsdTotalLabel;

        private decimal subtotal = 0m;
        private const decimal TAX_RATE = 0.05m;
        private const decimal GATEWAY_CHARGE_RATE = 0.03m;
        private const decimal LKR_TO_USD = 300m;

        public CashierForm()
        {
            this.BackColor = UITheme.ColBackground;
            this.Font = UITheme.FontBody;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // ── Two-panel split ───────────────────────────────────────────
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.None,
                SplitterWidth = 1,
                BackColor = UITheme.ColBorder
            };
            // Set distance safely after initialization if width allows
            this.Load += (s, e) => {
                splitContainer.Panel1MinSize = 300;
                splitContainer.Panel2MinSize = 280;
                try {
                    int target = (int)(splitContainer.Width * 0.65);
                    if (target > splitContainer.Panel1MinSize && target < (splitContainer.Width - splitContainer.Panel2MinSize))
                        splitContainer.SplitterDistance = target;
                    else
                        splitContainer.SplitterDistance = Math.Max(splitContainer.Panel1MinSize, splitContainer.Width - splitContainer.Panel2MinSize - 10);
                } catch { /* Ignore failed distance adjustments */ }
            };
            splitContainer.Panel1.BackColor = UITheme.ColBackground;
            splitContainer.Panel2.BackColor = UITheme.ColBackground;

            // ── LEFT PANEL — Product Browser & Cart ─────────────────────
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.ColBackground };
            
            // Product Browser — light card style
            var productBrowser = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 270,
                AutoScroll = true,
                Padding = new Padding(10, 10, 4, 10),
                BackColor = UITheme.ColBackground,
                WrapContents = true
            };

            // Header label
            var lblBrowserTitle = new Label
            {
                Text = "Products",
                Font = UITheme.FontBold10,
                ForeColor = UITheme.ColTextMuted,
                Margin = new Padding(4, 4, 4, 8),
                AutoSize = false,
                Size = new Size(246, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            productBrowser.Controls.Add(lblBrowserTitle);

            // Separator
            var sep = new Panel { Size = new Size(246, 1), BackColor = UITheme.ColBorder, Margin = new Padding(0, 0, 0, 8) };
            productBrowser.Controls.Add(sep);

            // Load products
            try {
                var db = DatabaseContext.Instance; db.Connection.Open();
                using var cmd = db.Connection.CreateCommand();
                cmd.CommandText = "SELECT name, price FROM products";
                using var r = cmd.ExecuteReader();
                string[] icons = { "☕","🥛","🥐","🧋","🍵","🫖","🍰","🧃" };
                int idx = 0;
                while (r.Read()) {
                    string name = r["name"].ToString();
                    decimal price = Convert.ToDecimal(r["price"]);
                    string icon = icons[idx++ % icons.Length];

                    // Card panel
                    var card = new Panel
                    {
                        Size = new Size(118, 110),
                        Margin = new Padding(4),
                        BackColor = UITheme.ColCard,
                        Cursor = Cursors.Hand,
                        Tag = new object[] { name, price }
                    };

                    // Rounded card paint
                    card.Paint += (s, e) =>
                    {
                        var g = e.Graphics;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using var shadowBrush = new SolidBrush(Color.FromArgb(10, 0, 0, 0));
                        using var shadowPath = UITheme.RoundedRect(new Rectangle(2, 3, card.Width - 3, card.Height - 3), 12);
                        g.FillPath(shadowBrush, shadowPath);
                        using var cardBrush = new SolidBrush(card.BackColor);
                        using var cardPath = UITheme.RoundedRect(new Rectangle(0, 0, card.Width - 2, card.Height - 2), 12);
                        g.FillPath(cardBrush, cardPath);
                        card.Region = new System.Drawing.Region(cardPath);
                    };

                    // Icon label
                    var lblIcon = new Label
                    {
                        Text = icon,
                        Font = new Font("Segoe UI Emoji", 18),
                        Location = new Point(0, 8),
                        Size = new Size(116, 36),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent
                    };

                    // Product name (truncate)
                    string shortName = name.Length > 16 ? name.Substring(0, 14) + ".." : name;
                    var lblName = new Label
                    {
                        Text = shortName,
                        Font = UITheme.FontSmall,
                        ForeColor = UITheme.ColTextPrimary,
                        Location = new Point(4, 48),
                        Size = new Size(110, 18),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent
                    };

                    // Price badge
                    var lblPrice = new Label
                    {
                        Text = $"Rs. {price:F2}",
                        Font = new Font("Segoe UI", 8, FontStyle.Bold),
                        ForeColor = UITheme.ColAccent,
                        Location = new Point(4, 70),
                        Size = new Size(110, 18),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent
                    };

                    // Add-to-cart "+" button
                    var btnAdd = new Label
                    {
                        Text = "+",
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = UITheme.ColAccent,
                        Location = new Point(42, 88),
                        Size = new Size(34, 18),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Cursor = Cursors.Hand
                    };

                    card.Controls.AddRange(new Control[] { lblIcon, lblName, lblPrice, btnAdd });

                    // Hover effect
                    EventHandler enterHandler = (s2, e2) => { card.BackColor = UITheme.ColRowAlt; card.Invalidate(); };
                    EventHandler leaveHandler = (s2, e2) => { card.BackColor = UITheme.ColCard; card.Invalidate(); };
                    card.MouseEnter += enterHandler; card.MouseLeave += leaveHandler;
                    foreach (Control c in card.Controls) { c.MouseEnter += enterHandler; c.MouseLeave += leaveHandler; }

                    // Click anywhere on card adds item
                    EventHandler clickHandler = (s2, e2) => AddCartItem(name, 1, price);
                    card.Click += clickHandler;
                    foreach (Control c in card.Controls) c.Click += clickHandler;

                    productBrowser.Controls.Add(card);
                }
                db.Connection.Close();
            } catch (Exception ex) {
                productBrowser.Controls.Add(new Label { Text = "⚠ " + ex.Message, ForeColor = UITheme.ColRed, AutoSize = true, Margin = new Padding(8) });
            }

            // Cart Pad
            var cartPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 16, 16, 16), BackColor = UITheme.ColBackground };

            var lblCartTitle = new Label { Text = "Current Transaction", Font = UITheme.FontH2, ForeColor = UITheme.ColTextPrimary, Location = new Point(0, 0), AutoSize = true };

            // Rounded search bar
            var searchPanel = new Panel { Location = new Point(0, 34), Size = new Size(520, 38), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            searchPanel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var path = UITheme.RoundedRect(new Rectangle(0, 0, searchPanel.Width - 1, searchPanel.Height - 1), 8);
                using var b = new SolidBrush(Color.White); e.Graphics.FillPath(b, path);
                using var pen = new Pen(UITheme.ColBorder, 1.5f); e.Graphics.DrawPath(pen, path);
                searchPanel.Region = new System.Drawing.Region(path);
            };
            searchPanel.Controls.Add(new Label { Text = "🔍", Font = new Font("Segoe UI Emoji", 11), Location = new Point(8, 7), AutoSize = true, BackColor = Color.White });
            var txtSearch = new TextBox { BorderStyle = BorderStyle.None, Font = UITheme.FontBody, ForeColor = UITheme.ColTextPrimary, Location = new Point(36, 10), Size = new Size(460, 20), BackColor = Color.White };
            searchPanel.Controls.Add(txtSearch);

            lvCart = new ListView { Location = new Point(0, 84), Size = new Size(520, 340), View = View.Details, FullRowSelect = true, GridLines = false, BorderStyle = BorderStyle.None, BackColor = UITheme.ColCard, Font = UITheme.FontBody, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            lvCart.Columns.Add("Product", 200); lvCart.Columns.Add("Qty", 60); lvCart.Columns.Add("Unit Price", 100); lvCart.Columns.Add("Subtotal", 100); lvCart.Columns.Add("Remove", 60);

            var btnClear = new Button { Text = "🗑  Clear Cart", Location = new Point(0, lvCart.Bottom + 10), Size = new Size(130, 34), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            UITheme.StyleDanger(btnClear);
            btnClear.Click += (s, e) => { lvCart.Items.Clear(); subtotal = 0; UpdateTotals(); UpdateChange(); };

            cartPad.Controls.AddRange(new Control[] { lblCartTitle, searchPanel, lvCart, btnClear });
            leftPanel.Controls.Add(cartPad);
            leftPanel.Controls.Add(productBrowser);
            splitContainer.Panel1.Controls.Add(leftPanel);

            // ═══════════════════════════════════════════════
            // RIGHT PANEL — Totals & Checkout
            // ═══════════════════════════════════════════════

            var rightPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 16, 20, 16), BackColor = UITheme.ColBackground };

            var lblCheckoutTitle = new Label
            {
                Text = "Checkout",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };

            // Totals card
            var totalsCard = new Panel
            {
                Location = new Point(0, 36),
                Size = new Size(260, 170),
                BackColor = UITheme.ColCard,
                Padding = new Padding(16)
            };
            totalsCard.Paint += UITheme.PaintCardShadow;

            MakeTotalRow(totalsCard, "Subtotal",      16,  out lblSubtotal, UITheme.ColTextPrimary);
            var divRow = new Panel { Location = new Point(16, 66), Size = new Size(228, 1), BackColor = UITheme.ColBorder };
            MakeTotalRow(totalsCard, $"Tax ({TAX_RATE * 100:0}%)", 76, out lblTax,      UITheme.ColTextMuted);
            var divRow2 = new Panel { Location = new Point(16, 117), Size = new Size(228, 1), BackColor = UITheme.ColBorder };
            MakeTotalRow(totalsCard, "Total",       124, out lblTotal,    UITheme.ColGreen);
            lblTotal.Font = new Font("Segoe UI", 16, FontStyle.Bold);

            lblUsdTotalLabel = new Label
            {
                Text = "USD Total: $0.00",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.ColTextMuted,
                Location = new Point(150, 154),
                AutoSize = true,
                BackColor = Color.Transparent,
                Visible = false
            };
            totalsCard.Controls.Add(lblUsdTotalLabel);

            totalsCard.Controls.Add(divRow);
            totalsCard.Controls.Add(divRow2);

            // Payment method
            var lblPayMethod = new Label
            {
                Text = "Payment Method",
                Font = UITheme.FontBold10,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 222),
                AutoSize = true
            };

            rbCash = new RadioButton
            {
                Text = "💵  Cash",
                Font = UITheme.FontBody,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 248),
                AutoSize = true,
                Checked = true
            };
            rbCard = new RadioButton
            {
                Text = "💳  Card",
                Font = UITheme.FontBody,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(110, 248),
                AutoSize = true
            };

            // Tender Panel (Cash)
            var cashPanel = new Panel { Location = new Point(0, 280), Size = new Size(260, 100), BackColor = Color.Transparent };
            var lblTenderLbl = new Label { Text = "Tender Amount", Font = UITheme.FontBold10, ForeColor = UITheme.ColTextPrimary, Location = new Point(0, 0), AutoSize = true };
            txtTender = new TextBox { Location = new Point(0, 22), Width = 260, Height = 36, Font = UITheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            txtTender.TextChanged += (s, e) => UpdateChange();
            lblChange = new Label { Text = "Change: Rs. 0.00", Font = UITheme.FontBold10, ForeColor = UITheme.ColGreen, Location = new Point(0, 64), AutoSize = true };
            cashPanel.Controls.AddRange(new Control[] { lblTenderLbl, txtTender, lblChange });

            // Card Panel
            var cardPanel = new Panel { Location = new Point(0, 280), Size = new Size(260, 100), BackColor = Color.Transparent, Visible = false };
            var lblCardNum = new Label { Text = "Card Number", Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(0, 0), AutoSize = true };
            txtCardNum = new TextBox { Text = "XXXX-XXXX-XXXX-XXXX", Location = new Point(0, 18), Width = 260, Font = UITheme.FontBody };
            var lblExpiry = new Label { Text = "Expiry (MM/YY)", Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(0, 48), AutoSize = true };
            txtExpiry = new TextBox { Text = "MM/YY", Location = new Point(0, 66), Width = 120, Font = UITheme.FontBody };
            var lblCvv = new Label { Text = "CVV", Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(140, 48), AutoSize = true };
            txtCvv = new TextBox { Text = "XXX", Location = new Point(140, 66), Width = 120, Font = UITheme.FontBody };
            cardPanel.Controls.AddRange(new Control[] { lblCardNum, txtCardNum, lblExpiry, txtExpiry, lblCvv, txtCvv });

            rbCash.CheckedChanged += (s, e) => { cashPanel.Visible = rbCash.Checked; cardPanel.Visible = !rbCash.Checked; UpdateTotals(); UpdateChange(); };
            rbCard.CheckedChanged += (s, e) => { cashPanel.Visible = !rbCard.Checked; cardPanel.Visible = rbCard.Checked; UpdateTotals(); UpdateChange(); };

            // Checkout button
            var btnCheckout = new Button
            {
                Text = "✔  COMPLETE CHECKOUT",
                Location = new Point(0, 396),
                Size = new Size(260, 48)
            };
            UITheme.StylePrimary(btnCheckout);
            btnCheckout.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnCheckout.Click += (s, e) =>
            {
                if (lvCart.Items.Count == 0)
                {
                    MessageBox.Show("Cart is empty!", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal currentTotal = CalculateTotalWithFees();

                if (rbCash.Checked)
                {
                    if (!decimal.TryParse(txtTender.Text, out decimal tender) || tender < currentTotal)
                    {
                        MessageBox.Show("Insufficient cash tender.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else // Card payment
                {
                    // Basic validation for card details
                    if (txtCardNum.Text.Replace("-", "").Length < 13 || txtCardNum.Text.Replace("-", "").Length > 19 || !long.TryParse(txtCardNum.Text.Replace("-", ""), out _))
                    {
                        MessageBox.Show("Please enter a valid card number.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (txtExpiry.Text.Length != 5 || !txtExpiry.Text.Contains("/") || !int.TryParse(txtExpiry.Text.Substring(0, 2), out int month) || !int.TryParse(txtExpiry.Text.Substring(3, 2), out int year) || month < 1 || month > 12 || year < DateTime.Now.Year % 100)
                    {
                        MessageBox.Show("Please enter a valid expiry date (MM/YY).", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (txtCvv.Text.Length < 3 || txtCvv.Text.Length > 4 || !int.TryParse(txtCvv.Text, out _))
                    {
                        MessageBox.Show("Please enter a valid CVV.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                MessageBox.Show("Transaction Completed Successfully!", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Clear cart
                lvCart.Items.Clear();
                subtotal = 0;
                txtTender.Clear();
                UpdateTotals();
                UpdateChange();
            };

            rightPad.Controls.AddRange(new Control[]
            {
                lblCheckoutTitle, totalsCard,
                lblPayMethod, rbCash, rbCard,
                cashPanel, cardPanel, btnCheckout
            });
            splitContainer.Panel2.Controls.Add(rightPad);

            this.Controls.Add(splitContainer);
            this.Load += (s, e) => {
                 UpdateTotals();
                 UpdateChange();
            };
        }

        private void AddCartItem(string name, int qty, decimal unitPrice)
        {
            // Check if item already in cart
            foreach (ListViewItem match in lvCart.Items)
            {
                if (match.Text == name)
                {
                    int currentQty = int.Parse(match.SubItems[1].Text);
                    currentQty += qty;
                    match.SubItems[1].Text = currentQty.ToString();
                    match.SubItems[3].Text = $"Rs. {(currentQty * unitPrice):F2}";
                    subtotal += qty * unitPrice;
                    UpdateTotals();
                    UpdateChange();
                    return;
                }
            }

            // New item
            var item = new ListViewItem(new[]
            {
                name,
                qty.ToString(),
                $"Rs. {unitPrice:F2}",
                $"Rs. {qty * unitPrice:F2}",
                "❌"
            });
            item.BackColor = lvCart.Items.Count % 2 == 0 ? UITheme.ColCard : UITheme.ColRowAlt;
            lvCart.Items.Add(item);
            subtotal += qty * unitPrice;
            UpdateTotals();
            UpdateChange();
        }

        private void MakeTotalRow(Panel parent, string label, int top, out Label valLbl, Color valColor)
        {
            var lbl = new Label
            {
                Text = label,
                Font = UITheme.FontBody,
                ForeColor = UITheme.ColTextMuted,
                Location = new Point(16, top),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            valLbl = new Label
            {
                Text = "Rs. 0.00",
                Font = UITheme.FontBold10,
                ForeColor = valColor,
                Location = new Point(150, top),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(valLbl);
        }

        private void UpdateTotals()
        {
            decimal tax = subtotal * TAX_RATE;
            decimal totalLkr = subtotal + tax;
            
            if (rbCard.Checked)
            {
                decimal fee = totalLkr * GATEWAY_CHARGE_RATE;
                totalLkr += fee;
                
                decimal totalUsd = totalLkr / LKR_TO_USD;
                lblUsdTotalLabel.Text = $"USD Total: ${totalUsd:F2}";
                lblUsdTotalLabel.Visible = true;
            }
            else
            {
                lblUsdTotalLabel.Visible = false;
            }

            lblSubtotal.Text = $"Rs. {subtotal:F2}";
            lblTax.Text      = $"Rs. {tax:F2}";
            lblTotal.Text    = $"Rs. {totalLkr:F2}";
        }

        private decimal CalculateTotalWithFees()
        {
            decimal tax = subtotal * TAX_RATE;
            decimal total = subtotal + tax;
            if (rbCard.Checked)
            {
                total += (total * GATEWAY_CHARGE_RATE);
            }
            return total;
        }

        private void UpdateChange()
        {
            decimal tax   = subtotal * TAX_RATE;
            decimal total = subtotal + tax;
            if (decimal.TryParse(txtTender.Text, out decimal tender))
            {
                decimal change = tender - total;
                lblChange.Text      = change >= 0 ? $"Change: Rs. {change:F2}" : "Insufficient tender";
                lblChange.ForeColor = change >= 0 ? UITheme.ColGreen : UITheme.ColRed;
            }
            else
            {
                lblChange.Text      = "Change: —";
                lblChange.ForeColor = UITheme.ColTextMuted;
            }
        }
    }
}
