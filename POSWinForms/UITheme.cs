using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace POSWinForms
{
    public static class UITheme
    {
        // ── Palette ────────────────────────────────────────────────────────────
        public static readonly Color ColSidebar       = Color.FromArgb(30, 35, 48);
        public static readonly Color ColSidebarHover  = Color.FromArgb(42, 48, 69);
        public static readonly Color ColAccent        = Color.FromArgb(0, 191, 165);
        public static readonly Color ColTopBar        = Color.FromArgb(255, 255, 255);
        public static readonly Color ColBackground    = Color.FromArgb(240, 244, 248);
        public static readonly Color ColCard          = Color.FromArgb(255, 255, 255);
        public static readonly Color ColTextPrimary   = Color.FromArgb(26, 32, 44);
        public static readonly Color ColTextMuted     = Color.FromArgb(113, 128, 150);
        public static readonly Color ColGreen         = Color.FromArgb(56, 161, 105);
        public static readonly Color ColBlue          = Color.FromArgb(49, 130, 206);
        public static readonly Color ColRed           = Color.FromArgb(229, 62, 62);
        public static readonly Color ColPurple        = Color.FromArgb(128, 90, 213);
        public static readonly Color ColOrange        = Color.FromArgb(237, 137, 54);
        public static readonly Color ColRowAlt        = Color.FromArgb(247, 250, 252);
        public static readonly Color ColBorder        = Color.FromArgb(226, 232, 240);
        public static readonly Color ColLowStock      = Color.FromArgb(254, 235, 200);

        // ── Corner Radius Constants ─────────────────────────────────────────────
        public const int RadiusButton = 10;
        public const int RadiusCard   = 14;
        public const int RadiusInput  = 8;
        public const int RadiusBadge  = 6;

        // ── Fonts ──────────────────────────────────────────────────────────────
        public static readonly Font FontH1      = new Font("Segoe UI", 20, FontStyle.Bold,    GraphicsUnit.Point);
        public static readonly Font FontH2      = new Font("Segoe UI", 14, FontStyle.Bold,    GraphicsUnit.Point);
        public static readonly Font FontH3      = new Font("Segoe UI", 11, FontStyle.Bold,    GraphicsUnit.Point);
        public static readonly Font FontBody    = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontSmall   = new Font("Segoe UI",  8, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontBold10  = new Font("Segoe UI", 10, FontStyle.Bold,    GraphicsUnit.Point);
        public static readonly Font FontSidebar = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontLogo    = new Font("Segoe UI", 17, FontStyle.Bold,    GraphicsUnit.Point);
        public static readonly Font FontStat    = new Font("Segoe UI", 22, FontStyle.Bold,    GraphicsUnit.Point);

        // ── Rounded Path Helper ────────────────────────────────────────────────
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X,              bounds.Y,               d, d, 180, 90);
            path.AddArc(bounds.Right - d,       bounds.Y,               d, d, 270, 90);
            path.AddArc(bounds.Right - d,       bounds.Bottom - d,      d, d,   0, 90);
            path.AddArc(bounds.X,              bounds.Bottom - d,      d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        public static System.Drawing.Region RoundedRegion(int width, int height, int radius)
        {
            return new System.Drawing.Region(RoundedRect(new Rectangle(0, 0, width, height), radius));
        }

        // ── Button Stylers ─────────────────────────────────────────────────────
        public static void StylePrimary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = ColAccent;
            b.ForeColor = Color.White;
            b.Font = FontBold10;
            b.Height = 40;
            b.Cursor = Cursors.Hand;
            b.Paint += (s, e) =>
            {
                var btn = (Button)s;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, btn.Width, btn.Height), RadiusButton);
                using var brush = new SolidBrush(btn.BackColor);
                g.FillPath(brush, path);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(btn.Text, btn.Font, Brushes.White, new RectangleF(0, 0, btn.Width, btn.Height), sf);
                btn.Region = new System.Drawing.Region(path);
            };
            b.MouseEnter += (s, e) => { b.BackColor = Color.FromArgb(0, 168, 145); b.Invalidate(); };
            b.MouseLeave += (s, e) => { b.BackColor = ColAccent; b.Invalidate(); };
        }

        public static void StyleSecondary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.White;
            b.ForeColor = ColTextPrimary;
            b.Font = FontBody;
            b.Height = 36;
            b.Cursor = Cursors.Hand;
            b.Paint += (s, e) =>
            {
                var btn = (Button)s;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, btn.Width, btn.Height), RadiusButton);
                using var bg = new SolidBrush(btn.BackColor);
                g.FillPath(bg, path);
                using var pen = new Pen(ColBorder, 1.5f);
                g.DrawPath(pen, path);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var textBrush = new SolidBrush(btn.ForeColor);
                g.DrawString(btn.Text, btn.Font, textBrush, new RectangleF(0, 0, btn.Width, btn.Height), sf);
                btn.Region = new System.Drawing.Region(path);
            };
            b.MouseEnter += (s, e) => { b.BackColor = ColRowAlt; b.Invalidate(); };
            b.MouseLeave += (s, e) => { b.BackColor = Color.White; b.Invalidate(); };
        }

        public static void StyleDanger(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Color.White;
            b.ForeColor = ColRed;
            b.Font = FontBody;
            b.Height = 36;
            b.Cursor = Cursors.Hand;
            b.Paint += (s, e) =>
            {
                var btn = (Button)s;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, btn.Width, btn.Height), RadiusButton);
                using var bg = new SolidBrush(btn.BackColor);
                g.FillPath(bg, path);
                using var pen = new Pen(ColRed, 1.5f);
                g.DrawPath(pen, path);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var textBrush = new SolidBrush(ColRed);
                g.DrawString(btn.Text, btn.Font, textBrush, new RectangleF(0, 0, btn.Width, btn.Height), sf);
                btn.Region = new System.Drawing.Region(path);
            };
            b.MouseEnter += (s, e) => { b.BackColor = Color.FromArgb(255, 245, 245); b.Invalidate(); };
            b.MouseLeave += (s, e) => { b.BackColor = Color.White; b.Invalidate(); };
        }

        public static void StyleActionButton(Button b, Color col)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = col;
            b.ForeColor = Color.White;
            b.Font = FontSmall;
            b.Height = 26;
            b.Cursor = Cursors.Hand;
            b.Paint += (s, e) =>
            {
                var btn = (Button)s;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, btn.Width, btn.Height), RadiusBadge);
                using var brush = new SolidBrush(btn.BackColor);
                g.FillPath(brush, path);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(btn.Text, btn.Font, Brushes.White, new RectangleF(0, 0, btn.Width, btn.Height), sf);
                btn.Region = new System.Drawing.Region(path);
            };
        }

        // ── Rounded Card Panel Painter ─────────────────────────────────────────
        /// <summary>Attaches rounded-card paint with soft shadow to any Panel.</summary>
        public static void StyleRoundedCard(Panel p, int radius = RadiusCard)
        {
            p.BackColor = ColCard;
            p.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Soft drop shadow
                using var shadowBrush = new SolidBrush(Color.FromArgb(14, 0, 0, 0));
                using var shadowPath = RoundedRect(new Rectangle(3, 4, p.Width - 5, p.Height - 5), radius);
                g.FillPath(shadowBrush, shadowPath);
                // Card fill
                using var cardBrush = new SolidBrush(p.BackColor);
                using var cardPath = RoundedRect(new Rectangle(0, 0, p.Width - 3, p.Height - 4), radius);
                g.FillPath(cardBrush, cardPath);
                p.Region = new System.Drawing.Region(cardPath);
            };
        }

        public static void PaintCardShadow(object sender, PaintEventArgs e)
        {
            var ctrl = (Control)sender;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var shadowBrush = new SolidBrush(Color.FromArgb(16, 0, 0, 0));
            using var shadowPath = RoundedRect(new Rectangle(3, 4, ctrl.Width - 5, ctrl.Height - 5), RadiusCard);
            g.FillPath(shadowBrush, shadowPath);
            using var cardBrush = new SolidBrush(ColCard);
            using var cardPath = RoundedRect(new Rectangle(0, 0, ctrl.Width - 3, ctrl.Height - 4), RadiusCard);
            g.FillPath(cardBrush, cardPath);
            ((Control)sender).Region = new System.Drawing.Region(cardPath);
        }

        // ── ListView Styler ────────────────────────────────────────────────────
        public static void StyleListView(ListView lv)
        {
            lv.OwnerDraw = false;
            lv.FullRowSelect = true;
            lv.GridLines = false;
            lv.BorderStyle = BorderStyle.None;
            lv.BackColor = ColCard;
            lv.Font = FontBody;
            lv.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        // ── TextBox Styler ─────────────────────────────────────────────────────
        public static void StyleTextBox(TextBox tb)
        {
            tb.BorderStyle = BorderStyle.None;
            tb.Font = FontBody;
            tb.BackColor = Color.White;
            tb.ForeColor = ColTextPrimary;
            tb.Height = 34;
        }

        /// <summary>Wraps a TextBox in a rounded border panel.</summary>
        public static Panel WrapTextBox(TextBox tb, int radius = RadiusInput)
        {
            tb.BorderStyle = BorderStyle.None;
            tb.Font = FontBody;
            var wrap = new Panel { BackColor = Color.White, Padding = new Padding(8, 6, 8, 6) };
            wrap.Controls.Add(tb);
            tb.Dock = DockStyle.Fill;
            wrap.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(wrap.BackColor);
                using var path = RoundedRect(new Rectangle(0, 0, wrap.Width - 1, wrap.Height - 1), radius);
                g.FillPath(brush, path);
                using var pen = new Pen(ColBorder, 1.5f);
                g.DrawPath(pen, path);
                wrap.Region = new System.Drawing.Region(path);
            };
            return wrap;
        }

        // ── ComboBox Styler ────────────────────────────────────────────────────
        public static void StyleComboBox(ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.Font = FontBody;
            cb.BackColor = Color.White;
            cb.ForeColor = ColTextPrimary;
        }

        // ── Colour-coded Role Badge ────────────────────────────────────────────
        public static Color RoleColour(string role)
        {
            return role?.ToLower() switch
            {
                "admin"     => ColRed,
                "manager"   => ColAccent,
                "cashier"   => ColBlue,
                "inventory" => ColOrange,
                _           => ColTextMuted
            };
        }
    }
}
