using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace POSWinForms.Views
{
    public class CashierForm : Form
    {
        private ListView lvCart;
        private Label lblSubtotal, lblTax, lblTotal;
        private RadioButton rbCash, rbCard;
        private TextBox txtTender;
        private Label lblChange;

        private decimal subtotal = 0m;
        private const decimal TAX_RATE = 0.05m;

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
                SplitterDistance = 580,
                FixedPanel = FixedPanel.None,
                Panel1MinSize = 400,
                Panel2MinSize = 280,
                SplitterWidth = 1,
                BackColor = UITheme.ColBorder
            };
            splitContainer.Panel1.BackColor = UITheme.ColBackground;
            splitContainer.Panel2.BackColor = UITheme.ColBackground;

            // ═══════════════════════════════════════════════
            // LEFT PANEL — Product search + Cart
            // ═══════════════════════════════════════════════

            var leftPad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 12, 16), BackColor = UITheme.ColBackground };

            var lblCartTitle = new Label
            {
                Text = "Current Transaction",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };

            // Search bar
            var searchPanel = new Panel
            {
                Location = new Point(0, 34),
                Size = new Size(520, 38),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            searchPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(UITheme.ColBorder, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, searchPanel.Width - 1, searchPanel.Height - 1);
            };
            var lblSearchIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 11),
                Location = new Point(8, 7),
                AutoSize = true,
                BackColor = Color.White
            };
            var txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = UITheme.FontBody,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(36, 10),
                Size = new Size(460, 20),
                BackColor = Color.White,
            };
            searchPanel.Controls.Add(lblSearchIcon);
            searchPanel.Controls.Add(txtSearch);

            // Cart ListView
            lvCart = new ListView
            {
                Location = new Point(0, 82),
                Size = new Size(520, 340),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = UITheme.ColCard,
                Font = UITheme.FontBody,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lvCart.Columns.Add("Product",  200);
            lvCart.Columns.Add("Qty",       60);
            lvCart.Columns.Add("Unit Price",100);
            lvCart.Columns.Add("Subtotal",  100);
            lvCart.Columns.Add("Remove",     60);

            // Sample cart items
            AddCartItem("Espresso Beans",  2, 18.50m);
            AddCartItem("Whole Milk 1L",   1,  2.10m);
            AddCartItem("Croissants x6",   1, 12.00m);

            // Cart header with border
            var cartHeader = new Panel
            {
                Location = new Point(0, 76),
                Size = new Size(520, 6),
                BackColor = Color.Transparent
            };
            cartHeader.Paint += (s, e) =>
            {
                using var brush = new SolidBrush(UITheme.ColAccent);
                e.Graphics.FillRectangle(brush, 0, 5, 520, 1);
            };

            // Clear cart button
            var btnClear = new Button
            {
                Text = "🗑  Clear Cart",
                Location = new Point(0, lvCart.Bottom + 10),
                Size = new Size(130, 34),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            UITheme.StyleDanger(btnClear);
            btnClear.Click += (s, e) => { lvCart.Items.Clear(); subtotal = 0; UpdateTotals(); };

            leftPad.Controls.AddRange(new Control[] { lblCartTitle, searchPanel, lvCart, btnClear });
            splitContainer.Panel1.Controls.Add(leftPad);

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
            totalsCard.Paint += (s, e) =>
            {
                using var pen = new Pen(UITheme.ColBorder, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, totalsCard.Width - 1, totalsCard.Height - 1);
            };

            MakeTotalRow(totalsCard, "Subtotal",      16,  out lblSubtotal, UITheme.ColTextPrimary);
            var divRow = new Panel { Location = new Point(16, 66), Size = new Size(228, 1), BackColor = UITheme.ColBorder };
            MakeTotalRow(totalsCard, $"Tax ({TAX_RATE * 100:0}%)", 76, out lblTax,      UITheme.ColTextMuted);
            var divRow2 = new Panel { Location = new Point(16, 117), Size = new Size(228, 1), BackColor = UITheme.ColBorder };
            MakeTotalRow(totalsCard, "Total",       124, out lblTotal,    UITheme.ColGreen);
            lblTotal.Font = new Font("Segoe UI", 16, FontStyle.Bold);

            totalsCard.Controls.Add(divRow);
            totalsCard.Controls.Add(divRow2);
            UpdateTotals();

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

            // Tender
            var lblTenderLbl = new Label
            {
                Text = "Tender Amount",
                Font = UITheme.FontBold10,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 282),
                AutoSize = true
            };
            txtTender = new TextBox
            {
                Location = new Point(0, 304),
                Width = 260,
                Height = 36,
                Font = UITheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtTender.TextChanged += (s, e) => UpdateChange();

            lblChange = new Label
            {
                Text = "Change: $0.00",
                Font = UITheme.FontBold10,
                ForeColor = UITheme.ColGreen,
                Location = new Point(0, 346),
                AutoSize = true
            };

            // Checkout button
            var btnCheckout = new Button
            {
                Text = "✔  COMPLETE CHECKOUT",
                Location = new Point(0, 390),
                Size = new Size(260, 48)
            };
            UITheme.StylePrimary(btnCheckout);
            btnCheckout.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            rightPad.Controls.AddRange(new Control[]
            {
                lblCheckoutTitle, totalsCard,
                lblPayMethod, rbCash, rbCard,
                lblTenderLbl, txtTender, lblChange, btnCheckout
            });
            splitContainer.Panel2.Controls.Add(rightPad);

            this.Controls.Add(splitContainer);
        }

        private void AddCartItem(string name, int qty, decimal unitPrice)
        {
            var item = new ListViewItem(new[]
            {
                name,
                qty.ToString(),
                $"${unitPrice:F2}",
                $"${qty * unitPrice:F2}",
                "❌"
            });
            item.BackColor = lvCart.Items.Count % 2 == 0 ? UITheme.ColCard : UITheme.ColRowAlt;
            lvCart.Items.Add(item);
            subtotal += qty * unitPrice;
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
                Text = "$0.00",
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
            decimal tax   = subtotal * TAX_RATE;
            decimal total = subtotal + tax;
            lblSubtotal.Text = $"${subtotal:F2}";
            lblTax.Text      = $"${tax:F2}";
            lblTotal.Text    = $"${total:F2}";
        }

        private void UpdateChange()
        {
            decimal tax   = subtotal * TAX_RATE;
            decimal total = subtotal + tax;
            if (decimal.TryParse(txtTender.Text, out decimal tender))
            {
                decimal change = tender - total;
                lblChange.Text      = change >= 0 ? $"Change: ${change:F2}" : "Insufficient tender";
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
