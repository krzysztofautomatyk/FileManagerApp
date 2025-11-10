using System.Drawing;

namespace FileManagerApp.Core
{
    /// <summary>
    /// Modern color theme with professional palette
    /// </summary>
    public static class ModernTheme
    {
        // Primary Colors - Professional Blues
        public static Color PrimaryColor = Color.FromArgb(0, 120, 215);      // Microsoft Blue
        public static Color PrimaryDark = Color.FromArgb(0, 99, 177);
        public static Color PrimaryLight = Color.FromArgb(76, 154, 255);

        // Accent Colors - Professional Palette
        public static Color AccentGreen = Color.FromArgb(16, 124, 16);       // Professional Green
        public static Color AccentOrange = Color.FromArgb(202, 80, 16);      // Professional Orange
        public static Color AccentRed = Color.FromArgb(196, 43, 28);         // Professional Red
        public static Color AccentPurple = Color.FromArgb(135, 100, 184);    // Professional Purple
        public static Color AccentYellow = Color.FromArgb(255, 185, 0);      // Professional Yellow
        public static Color AccentTeal = Color.FromArgb(0, 183, 195);        // Professional Teal
        public static Color AccentGray = Color.FromArgb(96, 94, 92);         // Professional Gray

        // Neutral Colors
        public static Color BackgroundLight = Color.FromArgb(248, 249, 250);
        public static Color BackgroundWhite = Color.White;
        public static Color BackgroundGray = Color.FromArgb(236, 240, 241);

        // Text Colors
        public static Color TextDark = Color.FromArgb(44, 62, 80);
        public static Color TextMedium = Color.FromArgb(127, 140, 141);
        public static Color TextLight = Color.FromArgb(149, 165, 166);
        public static Color TextWhite = Color.White;

        // Border Colors
        public static Color BorderLight = Color.FromArgb(220, 230, 240);
        public static Color BorderMedium = Color.FromArgb(200, 210, 220);
        public static Color BorderDark = Color.FromArgb(180, 190, 200);

        // Gradient Backgrounds
        public static Color GradientStart = Color.FromArgb(245, 247, 250);
        public static Color GradientEnd = Color.FromArgb(255, 255, 255);

        // Hover/Active States
        public static Color HoverBackground = Color.FromArgb(245, 247, 250);
        public static Color ActiveBackground = Color.FromArgb(52, 152, 219);

        // Shadow
        public static Color ShadowColor = Color.FromArgb(30, 0, 0, 0);

        // Status Colors
        public static Color SuccessColor = Color.FromArgb(39, 174, 96);
        public static Color WarningColor = Color.FromArgb(243, 156, 18);
        public static Color ErrorColor = Color.FromArgb(192, 57, 43);
        public static Color InfoColor = Color.FromArgb(52, 152, 219);

        // Card Colors
        public static Color CardBackground = Color.White;
        public static Color CardBorder = Color.FromArgb(220, 230, 240);
        public static Color CardShadow = Color.FromArgb(20, 0, 0, 0);

        // Sidebar
        public static Color SidebarBackground = Color.FromArgb(44, 62, 80);
        public static Color SidebarHover = Color.FromArgb(52, 73, 94);

        // File Type Colors
        public static Color FileTypeCode = Color.FromArgb(149, 117, 205);
        public static Color FileTypeDocument = Color.FromArgb(52, 152, 219);
        public static Color FileTypeImage = Color.FromArgb(46, 204, 113);
        public static Color FileTypeArchive = Color.FromArgb(230, 126, 34);
        public static Color FileTypeGeneric = Color.FromArgb(149, 165, 166);

        // Fonts
        public static Font TitleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static Font HeadingFont = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static Font SubheadingFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        public static Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static Font SmallFont = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static Font CodeFont = new Font("Consolas", 10F, FontStyle.Regular);

        /// <summary>
        /// Get color by file extension
        /// </summary>
        public static Color GetFileTypeColor(string extension)
        {
            return extension.ToLower() switch
            {
                ".cs" or ".cpp" or ".h" or ".py" or ".java" => FileTypeCode,
                ".txt" or ".md" or ".json" or ".xml" => FileTypeDocument,
                ".png" or ".jpg" or ".jpeg" or ".gif" => FileTypeImage,
                ".zip" or ".rar" or ".7z" => FileTypeArchive,
                _ => FileTypeGeneric
            };
        }

        /// <summary>
        /// Get icon by file extension (Unicode emoji)
        /// </summary>
        public static string GetFileIcon(string extension)
        {
            return extension.ToLower() switch
            {
                ".cs" or ".cpp" or ".h" or ".py" or ".java" => "💻",
                ".txt" => "📝",
                ".md" => "📄",
                ".json" or ".xml" => "⚙️",
                ".png" or ".jpg" or ".jpeg" or ".gif" => "🖼️",
                ".zip" or ".rar" or ".7z" => "📦",
                ".pdf" => "📕",
                ".html" or ".css" or ".js" => "🌐",
                ".exe" or ".dll" => "⚡",
                _ => "📄"
            };
        }

        /// <summary>
        /// Create a lighter version of a color
        /// </summary>
        public static Color Lighten(Color color, float amount)
        {
            return Color.FromArgb(
                color.A,
                (int)(color.R + (255 - color.R) * amount),
                (int)(color.G + (255 - color.G) * amount),
                (int)(color.B + (255 - color.B) * amount));
        }

        /// <summary>
        /// Create a darker version of a color
        /// </summary>
        public static Color Darken(Color color, float amount)
        {
            return Color.FromArgb(
                color.A,
                (int)(color.R * (1 - amount)),
                (int)(color.G * (1 - amount)),
                (int)(color.B * (1 - amount)));
        }
    }
}
