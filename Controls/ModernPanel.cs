using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FileManagerApp.Controls
{
    /// <summary>
    /// Modern panel with rounded corners, shadow, and gradient background
    /// </summary>
    public class ModernPanel : Panel
    {
        private int borderRadius = 12;
        private Color shadowColor = Color.FromArgb(30, 0, 0, 0);
        private int shadowDepth = 5;
        private bool showShadow = true;
        private Color gradientStart = Color.White;
        private Color gradientEnd = Color.FromArgb(245, 247, 250);
        private bool useGradient = true;
        private Color borderColor = Color.FromArgb(220, 230, 240);
        private int borderWidth = 1;

        public int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = value; Invalidate(); }
        }

        public bool ShowShadow
        {
            get => showShadow;
            set { showShadow = value; Invalidate(); }
        }

        public int ShadowDepth
        {
            get => shadowDepth;
            set { shadowDepth = value; Invalidate(); }
        }

        public Color GradientStart
        {
            get => gradientStart;
            set { gradientStart = value; Invalidate(); }
        }

        public Color GradientEnd
        {
            get => gradientEnd;
            set { gradientEnd = value; Invalidate(); }
        }

        public bool UseGradient
        {
            get => useGradient;
            set { useGradient = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public int BorderWidth
        {
            get => borderWidth;
            set { borderWidth = value; Invalidate(); }
        }

        public ModernPanel()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Padding = new Padding(15);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Draw shadow
            if (showShadow)
            {
                for (int i = shadowDepth; i >= 0; i--)
                {
                    int alpha = (int)(30 - (i * 5));
                    if (alpha < 0) alpha = 0;

                    Rectangle shadowRect = new Rectangle(
                        i, i,
                        Width - (i * 2) - 1,
                        Height - (i * 2) - 1);

                    using (GraphicsPath shadowPath = GetRoundedRectangle(shadowRect, borderRadius))
                    {
                        using (Brush shadowBrush = new SolidBrush(Color.FromArgb(alpha, shadowColor)))
                        {
                            g.FillPath(shadowBrush, shadowPath);
                        }
                    }
                }
            }

            // Main panel rectangle
            using (GraphicsPath path = GetRoundedRectangle(rect, borderRadius))
            {
                // Draw background
                if (useGradient && gradientStart != gradientEnd)
                {
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        rect,
                        gradientStart,
                        gradientEnd,
                        LinearGradientMode.Vertical))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(gradientStart))
                    {
                        g.FillPath(brush, path);
                    }
                }

                // Draw border
                if (borderWidth > 0)
                {
                    using (Pen pen = new Pen(borderColor, borderWidth))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;
            if (diameter < 1) diameter = 1;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
