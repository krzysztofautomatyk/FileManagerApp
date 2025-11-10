using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FileManagerApp.Controls
{
    /// <summary>
    /// Modern flat button with rounded corners, gradient, and smooth hover effects
    /// </summary>
    public class ModernButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;
        private int borderRadius = 8;
        private Color hoverColor = Color.FromArgb(70, 130, 180);
        private Color normalColor = Color.FromArgb(52, 152, 219);
        private Color pressedColor = Color.FromArgb(41, 128, 185);
        private Color borderColor = Color.FromArgb(41, 128, 185);

        public int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = value; Invalidate(); }
        }

        public Color HoverColor
        {
            get => hoverColor;
            set { hoverColor = value; Invalidate(); }
        }

        public Color NormalColor
        {
            get => normalColor;
            set { normalColor = value; Invalidate(); }
        }

        public Color PressedColor
        {
            get => pressedColor;
            set { pressedColor = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            Cursor = Cursors.Hand;
            Size = new Size(120, 40);

            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Determine color based on state
            Color bgColor = normalColor;
            if (!Enabled)
                bgColor = Color.FromArgb(180, 180, 180);
            else if (isPressed)
                bgColor = pressedColor;
            else if (isHovered)
                bgColor = hoverColor;

            // Create rounded rectangle path
            GraphicsPath path = GetRoundedRectangle(ClientRectangle, borderRadius);

            // Draw shadow
            if (Enabled && !isPressed)
            {
                using (GraphicsPath shadowPath = GetRoundedRectangle(
                    new Rectangle(2, 2, Width - 4, Height - 4), borderRadius))
                {
                    using (PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath))
                    {
                        shadowBrush.CenterColor = Color.FromArgb(50, 0, 0, 0);
                        shadowBrush.SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) };
                        g.FillPath(shadowBrush, shadowPath);
                    }
                }
            }

            // Draw button background with gradient
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect,
                bgColor,
                ControlPaint.Dark(bgColor, 0.1f),
                LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            // Draw border
            using (Pen pen = new Pen(borderColor, 1.5f))
            {
                g.DrawPath(pen, path);
            }

            // Draw text
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                ClientRectangle,
                Enabled ? ForeColor : Color.FromArgb(120, 120, 120),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            path.Dispose();
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
