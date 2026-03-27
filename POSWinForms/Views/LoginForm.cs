using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using POSWinForms.Services;
using POSWinForms.Models;
using POSWinForms.Data;
using POSWinForms.Views;

namespace POSWinForms.Views
{
    public class LoginForm : Form
    {
        private TextBox txtPin;
        private Button btnLogin;
        private Button btnTogglePin;
        private Label lblError;
        private AuthService _auth;

        public LoginForm()
        {
            this.Text = "ESC-POS — Login";
            this.Size = new Size(460, 580);
            this.MinimumSize = new Size(460, 580);
            this.MaximumSize = new Size(460, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.ColBackground;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = UITheme.FontBody;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // ── Outer card panel ──────────────────────────────────────────
            var card = new Panel
            {
                Size = new Size(360, 420),
                BackColor = UITheme.ColCard,
                Location = new Point(50, 70)
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(UITheme.ColBorder, 1);
                g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            // ── Logo icon ─────────────────────────────────────────────────
            var lblIcon = new Label
            {
                Text = "🛒",
                Font = new Font("Segoe UI Emoji", 36),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(360, 70),
                Location = new Point(0, 25),
                BackColor = Color.Transparent
            };

            // ── App name ──────────────────────────────────────────────────
            var lblApp = new Label
            {
                Text = "ESC-POS",
                Font = UITheme.FontLogo,
                ForeColor = UITheme.ColAccent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(360, 36),
                Location = new Point(0, 90),
                BackColor = Color.Transparent
            };

            // ── Subtitle ──────────────────────────────────────────────────
            var lblSub = new Label
            {
                Text = "Point of Sale System",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.ColTextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(360, 20),
                Location = new Point(0, 126),
                BackColor = Color.Transparent
            };

            // ── Divider ───────────────────────────────────────────────────
            var divider = new Panel
            {
                Height = 1,
                Width = 280,
                Location = new Point(40, 158),
                BackColor = UITheme.ColBorder
            };

            // ── PIN label ─────────────────────────────────────────────────
            var lblPin = new Label
            {
                Text = "Enter Your PIN",
                Font = UITheme.FontBold10,
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(40, 178),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // ── PIN container row ─────────────────────────────────────────
            var pinRow = new Panel
            {
                Location = new Point(40, 202),
                Size = new Size(280, 38),
                BackColor = Color.White
            };
            pinRow.Paint += (s, e) =>
            {
                using var pen = new Pen(UITheme.ColBorder, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pinRow.Width - 1, pinRow.Height - 1);
            };

            txtPin = new TextBox
            {
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 14),
                ForeColor = UITheme.ColTextPrimary,
                Location = new Point(10, 8),
                Size = new Size(228, 24),
                BackColor = Color.White
            };
            txtPin.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) PerformLogin(); };

            btnTogglePin = new Button
            {
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                Location = new Point(243, 5),
                Size = new Size(32, 28),
                Font = new Font("Segoe UI Emoji", 10),
                BackColor = Color.White,
                ForeColor = UITheme.ColTextMuted,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnTogglePin.FlatAppearance.BorderSize = 0;
            btnTogglePin.Click += (s, e) =>
            {
                txtPin.UseSystemPasswordChar = !txtPin.UseSystemPasswordChar;
                btnTogglePin.ForeColor = txtPin.UseSystemPasswordChar
                    ? UITheme.ColTextMuted
                    : UITheme.ColAccent;
            };

            pinRow.Controls.Add(txtPin);
            pinRow.Controls.Add(btnTogglePin);

            // ── Inline error label ────────────────────────────────────────
            lblError = new Label
            {
                Text = "",
                ForeColor = UITheme.ColRed,
                Font = UITheme.FontSmall,
                Location = new Point(40, 246),
                Size = new Size(280, 18),
                BackColor = Color.Transparent
            };

            // ── Login button ──────────────────────────────────────────────
            btnLogin = new Button
            {
                Text = "LOGIN",
                Location = new Point(40, 272),
                Size = new Size(280, 42),
            };
            UITheme.StylePrimary(btnLogin);
            btnLogin.Click += (s, e) => PerformLogin();

            // ── PIN hint ──────────────────────────────────────────────────
            var lblHint = new Label
            {
                Text = "Contact your administrator if you forgot your PIN.",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.ColTextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(280, 34),
                Location = new Point(40, 332),
                BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[]
            {
                lblIcon, lblApp, lblSub, divider, lblPin,
                pinRow, lblError, btnLogin, lblHint
            });

            // ── Version footer ────────────────────────────────────────────
            var lblVersion = new Label
            {
                Text = "ESC-POS v1.0.0 — © 2026",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.ColTextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(360, 20),
                Location = new Point(50, 510)
            };

            this.Controls.Add(card);
            this.Controls.Add(lblVersion);

            txtPin.Focus();
        }

        private void PerformLogin()
        {
            lblError.Text = "";
            if (_auth == null) _auth = new AuthService();
            var pin = txtPin.Text.Trim();
            if (string.IsNullOrEmpty(pin))
            {
                lblError.Text = "⚠ Please enter your PIN.";
                return;
            }
            var user = _auth.ValidatePin(pin);
            if (user != null)
            {
                var main = new MainForm(user);
                this.Hide();
                // When the main window is closed (logout), show the login screen again
                main.FormClosed += (s, e) => {
                    this.txtPin.Clear();
                    this.lblError.Text = "";
                    this.Show();
                };
                main.Show();
            }
            else
            {
                lblError.Text = "⚠ Invalid PIN. Please try again.";
                txtPin.Clear();
                txtPin.Focus();
            }
        }
    }
}
