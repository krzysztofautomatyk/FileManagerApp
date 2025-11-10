using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace FileManagerApp.Controls
{
    /// <summary>
    /// Modern custom list view with smooth scrolling, hover effects, and icons
    /// </summary>
    public class ModernListView : Control
    {
        private List<ListViewItem> items = new();
        private int hoveredIndex = -1;
        private int selectedIndex = -1;
        private VScrollBar vScrollBar;
        private int itemHeight = 45;
        private int scrollOffset = 0;
        private Font itemFont = new Font("Segoe UI", 10F);
        private Font subFont = new Font("Segoe UI", 8F);

        public class ListViewItem
        {
            public string MainText { get; set; } = string.Empty;
            public string SubText { get; set; } = string.Empty;
            public string Icon { get; set; } = "📄"; // Unicode icon
            public Color IconColor { get; set; } = Color.FromArgb(52, 152, 219);
            public object? Tag { get; set; }
        }

        public event EventHandler<int>? SelectedIndexChanged;

        public int ItemHeight
        {
            get => itemHeight;
            set { itemHeight = value; Invalidate(); }
        }

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    Invalidate();
                    SelectedIndexChanged?.Invoke(this, selectedIndex);
                }
            }
        }

        public List<ListViewItem> Items => items;

        public ModernListView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);

            BackColor = Color.White;
            BorderStyle = BorderStyle.None;

            // Setup scrollbar
            vScrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Width = 10,
                Visible = false
            };
            vScrollBar.Scroll += VScrollBar_Scroll;
            Controls.Add(vScrollBar);

            UpdateScrollBar();
        }

        public void AddItem(string mainText, string subText, string icon = "📄")
        {
            items.Add(new ListViewItem
            {
                MainText = mainText,
                SubText = subText,
                Icon = icon,
                IconColor = GetIconColorByExtension(mainText)
            });
            UpdateScrollBar();
            Invalidate();
        }

        public void Clear()
        {
            items.Clear();
            selectedIndex = -1;
            hoveredIndex = -1;
            scrollOffset = 0;
            UpdateScrollBar();
            Invalidate();
        }

        private Color GetIconColorByExtension(string filename)
        {
            string ext = System.IO.Path.GetExtension(filename).ToLower();
            return ext switch
            {
                ".cs" => Color.FromArgb(149, 117, 205),
                ".json" => Color.FromArgb(241, 196, 15),
                ".xml" => Color.FromArgb(230, 126, 34),
                ".txt" => Color.FromArgb(52, 152, 219),
                ".md" => Color.FromArgb(46, 204, 113),
                ".html" or ".css" or ".js" => Color.FromArgb(231, 76, 60),
                _ => Color.FromArgb(149, 165, 166)
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Background
            g.Clear(BackColor);

            int visibleItems = Height / itemHeight + 1;
            int startIndex = scrollOffset / itemHeight;
            int endIndex = Math.Min(startIndex + visibleItems, items.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                int y = (i * itemHeight) - scrollOffset;
                Rectangle itemRect = new Rectangle(0, y, Width - (vScrollBar.Visible ? vScrollBar.Width : 0), itemHeight);

                DrawItem(g, items[i], itemRect, i);
            }

            base.OnPaint(e);
        }

        private void DrawItem(Graphics g, ListViewItem item, Rectangle rect, int index)
        {
            bool isSelected = index == selectedIndex;
            bool isHovered = index == hoveredIndex;

            // Background
            if (isSelected)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect,
                    Color.FromArgb(52, 152, 219),
                    Color.FromArgb(41, 128, 185),
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush, rect);
                }
            }
            else if (isHovered)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(245, 247, 250)))
                {
                    g.FillRectangle(brush, rect);
                }
            }

            // Icon
            int iconSize = 24;
            Rectangle iconRect = new Rectangle(rect.X + 15, rect.Y + (rect.Height - iconSize) / 2, iconSize, iconSize);

            using (Font iconFont = new Font("Segoe UI Emoji", 14F))
            using (SolidBrush iconBrush = new SolidBrush(isSelected ? Color.White : item.IconColor))
            {
                g.DrawString(item.Icon, iconFont, iconBrush, iconRect.X, iconRect.Y);
            }

            // Text
            int textX = iconRect.Right + 15;
            int textY = rect.Y + 8;
            Color textColor = isSelected ? Color.White : Color.FromArgb(44, 62, 80);
            Color subTextColor = isSelected ? Color.FromArgb(220, 220, 220) : Color.FromArgb(127, 140, 141);

            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(item.MainText, itemFont, textBrush, textX, textY);
            }

            using (SolidBrush subBrush = new SolidBrush(subTextColor))
            {
                g.DrawString(item.SubText, subFont, subBrush, textX, textY + 20);
            }

            // Separator line
            if (!isSelected && index < items.Count - 1)
            {
                using (Pen pen = new Pen(Color.FromArgb(236, 240, 241), 1))
                {
                    g.DrawLine(pen, rect.X + 60, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int index = (e.Y + scrollOffset) / itemHeight;
            if (index >= 0 && index < items.Count && index != hoveredIndex)
            {
                hoveredIndex = index;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoveredIndex != -1)
            {
                hoveredIndex = -1;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int index = (e.Y + scrollOffset) / itemHeight;
            if (index >= 0 && index < items.Count)
            {
                SelectedIndex = index;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int delta = -e.Delta / 3;
            int newOffset = scrollOffset + delta;

            int maxScroll = Math.Max(0, (items.Count * itemHeight) - Height);
            newOffset = Math.Max(0, Math.Min(newOffset, maxScroll));

            if (newOffset != scrollOffset)
            {
                scrollOffset = newOffset;
                vScrollBar.Value = scrollOffset;
                Invalidate();
            }
        }

        private void VScrollBar_Scroll(object? sender, ScrollEventArgs e)
        {
            scrollOffset = e.NewValue;
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
        }

        private void UpdateScrollBar()
        {
            int contentHeight = items.Count * itemHeight;
            int visibleHeight = Height;

            if (contentHeight > visibleHeight)
            {
                vScrollBar.Visible = true;
                vScrollBar.Maximum = contentHeight - visibleHeight + vScrollBar.LargeChange - 1;
                vScrollBar.LargeChange = visibleHeight;
                vScrollBar.SmallChange = itemHeight;
            }
            else
            {
                vScrollBar.Visible = false;
                scrollOffset = 0;
            }

            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                itemFont?.Dispose();
                subFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
