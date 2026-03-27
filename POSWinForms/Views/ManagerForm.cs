using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace POSWinForms.Views
{
    public class ManagerForm : Form
    {
        private Panel contentArea;
        private string activePeriod = "Today";

        public ManagerForm()
        {
            this.BackColor = UITheme.ColBackground;
            this.Font = UITheme.FontBody;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var pad = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16), BackColor = UITheme.ColBackground };

            // ── Title ─────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "Sales Management",
                Font = UITheme.FontH2,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 0),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "View reports, KPIs, and export sales data.",
                Font = UITheme.FontBody,
                ForeColor = UITheme.ColTextMuted,
                Location = new Point(0, 28),
                AutoSize = true
            };

            // ── Period tab strip ──────────────────────────────────────────
            var tabStrip = new FlowLayoutPanel
            {
                Location = new Point(0, 64),
                Size = new Size(600, 38),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            foreach (var period in new[] { "Today", "Yesterday", "This Week", "This Month" })
            {
                var p = period; // capture
                var tabBtn = new Button
                {
                    Text = p,
                    Size = new Size(110, 36),
                    FlatStyle = FlatStyle.Flat,
                    Font = UITheme.FontBody,
                    BackColor = p == activePeriod ? UITheme.ColAccent : Color.White,
                    ForeColor = p == activePeriod ? Color.White : UITheme.ColTextMuted,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 6, 0)
                };
                tabBtn.FlatAppearance.BorderColor = p == activePeriod ? UITheme.ColAccent : UITheme.ColBorder;
                tabBtn.FlatAppearance.BorderSize = 1;
                tabBtn.Click += (s, e) =>
                {
                    activePeriod = p;
                    // Re-style all tab buttons
                    foreach (Control c in tabStrip.Controls)
                    {
                        if (c is Button tb)
                        {
                            tb.BackColor = tb.Text == activePeriod ? UITheme.ColAccent : Color.White;
                            tb.ForeColor = tb.Text == activePeriod ? Color.White : UITheme.ColTextMuted;
                            tb.FlatAppearance.BorderColor = tb.Text == activePeriod ? UITheme.ColAccent : UITheme.ColBorder;
                        }
                    }
                    RefreshContent();
                };
                tabStrip.Controls.Add(tabBtn);
            }
            // Export button
            var btnExport = new Button { Text = "⬇  Export CSV", Size = new Size(120, 36), Margin = new Padding(10, 0, 0, 0) };
            UITheme.StyleSecondary(btnExport);
            tabStrip.Controls.Add(btnExport);

            // ── Scrollable content area ───────────────────────────────────
            contentArea = new Panel
            {
                Location = new Point(0, 110),
                Size = new Size(900, 480),
                BackColor = Color.Transparent,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            pad.Controls.AddRange(new Control[] { lblTitle, lblSub, tabStrip, contentArea });
            this.Controls.Add(pad);

            RefreshContent();
        }

        private void RefreshContent()
        {
            contentArea.Controls.Clear();

            // KPI mini-cards row
            var kpiRow = new TableLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(880, 100),
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var (rev, txn, avg, topProd) = activePeriod switch
            {
                "Today"     => ("$1,420.50", "45",  "$31.56", "Espresso"),
                "Yesterday" => ("$2,105.00", "88",  "$23.92", "Cappuccino"),
                "This Week" => ("$9,204.80", "308", "$29.88", "Espresso"),
                _           => ("$34,120.00","1,402","$24.34","Espresso"),
            };

            kpiRow.Controls.Add(MakeKpiCard("💰 Revenue",         rev,      UITheme.ColGreen),  0, 0);
            kpiRow.Controls.Add(MakeKpiCard("🧾 Transactions",    txn,      UITheme.ColBlue),   1, 0);
            kpiRow.Controls.Add(MakeKpiCard("📈 Avg Order",       avg,      UITheme.ColPurple), 2, 0);
            kpiRow.Controls.Add(MakeKpiCard("⭐ Top Product",     topProd,   UITheme.ColOrange), 3, 0);
            contentArea.Controls.Add(kpiRow);

            // Section label
            var txnLabel = new Label
            {
                Text = $"Sales Report — {activePeriod}",
                Font = UITheme.FontH3,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(0, 116),
                AutoSize = true
            };
            contentArea.Controls.Add(txnLabel);

            // Sales table
            var lv = new ListView
            {
                Location = new Point(0, 144),
                Size = new Size(880, 260),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                BackColor = UITheme.ColCard,
                Font = UITheme.FontBody,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lv.Columns.Add("Date",         130);
            lv.Columns.Add("Time",         100);
            lv.Columns.Add("Cashier",      140);
            lv.Columns.Add("Items Sold",    90);
            lv.Columns.Add("Total",        110);
            lv.Columns.Add("Method",       100);

            var rows = new[]
            {
                new[] { "2026-03-27", "09:15", "Staff_01",  "3", "$55.00",  "Cash"  },
                new[] { "2026-03-27", "09:02", "Staff_01",  "1", "$18.50",  "Card"  },
                new[] { "2026-03-27", "08:47", "Admin",     "5", "$104.20", "Cash"  },
                new[] { "2026-03-27", "08:30", "Manager_X", "2", "$32.00",  "Card"  },
                new[] { "2026-03-27", "08:11", "Staff_01",  "4", "$76.80",  "Cash"  },
            };
            bool alt = false;
            foreach (var r in rows)
            {
                var item = new ListViewItem(r);
                item.BackColor = alt ? UITheme.ColRowAlt : UITheme.ColCard;
                alt = !alt;
                lv.Items.Add(item);
            }
            contentArea.Controls.Add(lv);
        }

        private Panel MakeKpiCard(string title, string value, Color accentColor)
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
                using var borderPen = new Pen(UITheme.ColBorder, 1);
                g.DrawRectangle(borderPen, 0, 0, card.Width - 1, card.Height - 1);
                using var ab = new SolidBrush(accentColor);
                g.FillRectangle(ab, 0, 0, 4, card.Height);
            };
            var lblT = new Label { Text = title, Font = UITheme.FontSmall, ForeColor = UITheme.ColTextMuted, Location = new Point(16, 14), AutoSize = true, BackColor = Color.Transparent };
            var lblV = new Label { Text = value, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = accentColor, Location = new Point(16, 34), AutoSize = true, BackColor = Color.Transparent };
            card.Controls.Add(lblT);
            card.Controls.Add(lblV);
            return card;
        }
    }
}
