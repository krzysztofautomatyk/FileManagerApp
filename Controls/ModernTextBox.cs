using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FileManagerApp.Controls
{
    /// <summary>
    /// Modern textbox with rounded corners, icon, and placeholder
    /// </summary>
    public class ModernTextBox : Control
    {
        private TextBox textBox;
        private string placeholderText = "Search...";
        private string icon = "🔍";
        private bool showIcon = true;
        private Color borderColor = Color.FromArgb(200, 210, 220);
        private Color focusBorderColor = Color.FromArgb(52, 152, 219);
        private int borderRadius = 8;
        private bool isFocused = false;

        public event EventHandler? TextChanged;

        public string PlaceholderText
        {
            get => placeholderText;
            set { placeholderText = value; Invalidate(); }
        }

        public string Icon
        {
            get => icon;
            set { icon = value; Invalidate(); }
        }

        public bool ShowIcon
        {
            get => showIcon;
            set { showIcon = value; UpdateTextBoxPadding(); Invalidate(); }
        }

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public Color FocusBorderColor
        {
            get => focusBorderColor;
            set { focusBorderColor = value; Invalidate(); }
        }

        public override string Text
        {
            get => textBox.Text;
            set => textBox.Text = value;
        }

        public ModernTextBox()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(300, 40);

            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11F),
                Location = new Point(45, 10),
                BackColor = Color.White
            };

            textBox.TextChanged += (s, e) => TextChanged?.Invoke(this, e);
            textBox.Enter += (s, e) => { isFocused = true; Invalidate(); };
            textBox.Leave += (s, e) => { isFocused = false; Invalidate(); };

            Controls.Add(textBox);
            UpdateTextBoxPadding();
        }

        private void UpdateTextBoxPadding()
        {
            textBox.Location = new Point(showIcon ? 45 : 15, (Height - textBox.Height) / 2);
            textBox.Width = Width - textBox.Left - 15;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateTextBoxPadding();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Background
            using (GraphicsPath path = GetRoundedRectangle(rect, borderRadius))
            {
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    g.FillPath(brush, path);
                }

                // Border
                Color currentBorderColor = isFocused ? focusBorderColor : borderColor;
                using (Pen pen = new Pen(currentBorderColor, isFocused ? 2 : 1))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Icon
            if (showIcon)
            {
                using (Font iconFont = new Font("Segoe UI Emoji", 14F))
                using (SolidBrush iconBrush = new SolidBrush(Color.FromArgb(149, 165, 166)))
                {
                    g.DrawString(icon, iconFont, iconBrush, 12, (Height - 20) / 2);
                }
            }

            // Placeholder
            if (string.IsNullOrEmpty(textBox.Text) && !isFocused)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(149, 165, 166)))
                {
                    g.DrawString(placeholderText, textBox.Font, brush, textBox.Left, textBox.Top);
                }
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
