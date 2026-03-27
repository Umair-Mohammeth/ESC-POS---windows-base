using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace POSWinForms
{
    /// <summary>
    /// Central design system — colours, fonts, and control stylizers.
    /// </summary>
    public static class UITheme
    {
        // ── Palette ────────────────────────────────────────────────────────────
        public static readonly Color ColSidebar       = Color.FromArgb(30, 35, 48);
        public static readonly Color ColSidebarHover  = Color.FromArgb(42, 48, 69);
        public static readonly Color ColAccent        = Color.FromArgb(0, 191, 165);   // teal
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

        // ── Fonts ──────────────────────────────────────────────────────────────
        public static readonly Font FontH1       = new Font("Segoe UI", 20, FontStyle.Bold,   GraphicsUnit.Point);
        public static readonly Font FontH2       = new Font("Segoe UI", 14, FontStyle.Bold,   GraphicsUnit.Point);
        public static readonly Font FontH3       = new Font("Segoe UI", 11, FontStyle.Bold,   GraphicsUnit.Point);
        public static readonly Font FontBody     = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontSmall    = new Font("Segoe UI",  8, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontBold10   = new Font("Segoe UI", 10, FontStyle.Bold,   GraphicsUnit.Point);
        public static readonly Font FontSidebar  = new Font("Segoe UI", 10, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font FontLogo     = new Font("Segoe UI", 17, FontStyle.Bold,   GraphicsUnit.Point);
        public static readonly Font FontStat     = new Font("Segoe UI", 22, FontStyle.Bold,   GraphicsUnit.Point);

        // ── Button Stylers ─────────────────────────────────────────────────────

        /// <summary>Full-width teal primary button.</summary>
        public static void StylePrimary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = ColAccent;
            b.ForeColor = Color.White;
            b.Font = FontBold10;
            b.Height = 40;
            b.Cursor = Cursors.Hand;
            b.Region = RoundedRegion(b.Width, b.Height, 6);
            b.Resize += (s, e) => b.Region = RoundedRegion(b.Width, b.Height, 6);
            b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(0, 168, 145);
            b.MouseLeave += (s, e) => b.BackColor = ColAccent;
        }

        /// <summary>Outlined secondary button.</summary>
        public static void StyleSecondary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = ColBorder;
            b.FlatAppearance.BorderSize = 1;
            b.BackColor = Color.White;
            b.ForeColor = ColTextPrimary;
            b.Font = FontBody;
            b.Height = 36;
            b.Cursor = Cursors.Hand;
        }

        /// <summary>Danger/destructive outlined button.</summary>
        public static void StyleDanger(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = ColRed;
            b.FlatAppearance.BorderSize = 1;
            b.BackColor = Color.White;
            b.ForeColor = ColRed;
            b.Font = FontBody;
            b.Height = 36;
            b.Cursor = Cursors.Hand;
        }

        /// <summary>Small coloured action button (e.g., for table rows).</summary>
        public static void StyleActionButton(Button b, Color col)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = col;
            b.ForeColor = Color.White;
            b.Font = FontSmall;
            b.Height = 26;
            b.Cursor = Cursors.Hand;
        }

        // ── Card Paint ─────────────────────────────────────────────────────────

        /// <summary>Paints a soft drop-shadow around a Panel card.</summary>
        public static void PaintCardShadow(object sender, PaintEventArgs e)
        {
            var ctrl = (Control)sender;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Light shadow
            using var shadowBrush = new SolidBrush(Color.FromArgb(18, 0, 0, 0));
            g.FillRectangle(shadowBrush, new Rectangle(3, 3, ctrl.Width - 3, ctrl.Height - 3));
        }

        // ── ListView Styler ────────────────────────────────────────────────────

        /// <summary>Applies consistent styling to a ListView.</summary>
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
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Font = FontBody;
            tb.BackColor = Color.White;
            tb.ForeColor = ColTextPrimary;
            tb.Height = 32;
        }

        // ── ComboBox Styler ────────────────────────────────────────────────────

        public static void StyleComboBox(ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.Font = FontBody;
            cb.BackColor = Color.White;
            cb.ForeColor = ColTextPrimary;
        }

        // ── Rounded Region Helper ──────────────────────────────────────────────

        public static System.Drawing.Region RoundedRegion(int width, int height, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(width - radius * 2, height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return new System.Drawing.Region(path);
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
