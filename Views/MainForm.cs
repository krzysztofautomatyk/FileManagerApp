using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using FileManagerApp.Core;
using FileManagerApp.Models;
using FileManagerApp.ViewModels;
using FileManagerApp.Controls;

namespace FileManagerApp
{
    public partial class MainForm : Form
    {
        private readonly MainViewModel _viewModel;

        // Modern UI Controls
        private Panel topBar = null!;
        private Label lblTitle = null!;
        private ModernButton btnMinimize = null!;
        private ModernButton btnMaximize = null!;
        private ModernButton btnClose = null!;

        private Panel actionBar = null!;
        private ModernButton btnAddFile = null!;
        private ModernButton btnAddDir = null!;
        private ModernButton btnClear = null!;
        private ModernButton btnGenerate = null!;
        private ModernButton btnClipboard = null!;
        private ModernButton btnCancel = null!;

        private ModernTextBox txtSearch = null!;

        private ModernPanel mainContainer = null!;
        private ModernPanel filesPanel = null!;
        private ModernPanel previewPanel = null!;
        private ModernPanel statsPanel = null!;

        private ModernListView listViewFiles = null!;
        private TextBox txtPreview = null!;

        private Label lblStatsTitle = null!;
        private Label lblTotalFiles = null!;
        private Label lblTotalSize = null!;
        private Label lblTextFiles = null!;
        private Label lblBinaryFiles = null!;

        private Panel statusBar = null!;
        private Label lblStatus = null!;
        private ProgressBar progressBar = null!;

        public MainForm()
        {
            _viewModel = ServiceContainer.GetService<MainViewModel>();
            InitializeComponent();
            SetupDataBindings();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            // Form settings
            Text = "File Manager Pro";
            Size = new Size(1400, 900);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1200, 700);
            FormBorderStyle = FormBorderStyle.None;
            BackColor = ModernTheme.BackgroundLight;
            DoubleBuffered = true;

            // Top Bar (Custom Title Bar)
            topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White
            };
            topBar.Paint += TopBar_Paint;

            lblTitle = new Label
            {
                Text = "📁 File Manager Pro",
                Location = new Point(20, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ModernTheme.TextDark
            };

            // Window control buttons
            btnClose = CreateWindowButton("×", ModernTheme.AccentRed);
            btnClose.Location = new Point(topBar.Width - 50, 10);
            btnClose.Size = new Size(40, 30);
            btnClose.Click += (s, e) => Close();

            btnMaximize = CreateWindowButton("□", ModernTheme.TextMedium);
            btnMaximize.Location = new Point(topBar.Width - 95, 10);
            btnMaximize.Size = new Size(40, 30);
            btnMaximize.Click += (s, e) => WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;

            btnMinimize = CreateWindowButton("─", ModernTheme.TextMedium);
            btnMinimize.Location = new Point(topBar.Width - 140, 10);
            btnMinimize.Size = new Size(40, 30);
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;

            topBar.Controls.AddRange(new Control[] { lblTitle, btnClose, btnMaximize, btnMinimize });

            // Enable window dragging
            topBar.MouseDown += TopBar_MouseDown;
            topBar.MouseMove += TopBar_MouseMove;
            topBar.MouseUp += TopBar_MouseUp;
            lblTitle.MouseDown += TopBar_MouseDown;
            lblTitle.MouseMove += TopBar_MouseMove;
            lblTitle.MouseUp += TopBar_MouseUp;

            // Action Bar
            actionBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };
            actionBar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(ModernTheme.BorderLight, 1), 0, actionBar.Height - 1, actionBar.Width, actionBar.Height - 1);
            };

            int buttonX = 20;
            btnAddFile = CreateActionButton("➕ Add Files", ModernTheme.PrimaryColor, buttonX);
            buttonX += 140;
            btnAddDir = CreateActionButton("📁 Add Directory", ModernTheme.AccentGreen, buttonX);
            buttonX += 160;
            btnClear = CreateActionButton("🗑️ Clear", ModernTheme.TextMedium, buttonX);
            buttonX += 120;
            btnGenerate = CreateActionButton("💾 Generate", ModernTheme.AccentPurple, buttonX);
            buttonX += 140;
            btnClipboard = CreateActionButton("📋 Copy", ModernTheme.AccentOrange, buttonX);
            buttonX += 120;
            btnCancel = CreateActionButton("⛔ Cancel", ModernTheme.AccentRed, buttonX);
            btnCancel.Enabled = false;

            actionBar.Controls.AddRange(new Control[] {
                btnAddFile, btnAddDir, btnClear, btnGenerate, btnClipboard, btnCancel
            });

            // Search Box
            txtSearch = new ModernTextBox
            {
                Location = new Point(20, 45),
                Width = 400,
                PlaceholderText = "Search files by name, path, or extension...",
                Icon = "🔍"
            };
            actionBar.Controls.Add(txtSearch);

            // Main Container
            mainContainer = new ModernPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ShowShadow = false,
                UseGradient = false,
                GradientStart = ModernTheme.BackgroundLight,
                BorderColor = Color.Transparent,
                BorderWidth = 0
            };

            // Files Panel (Left)
            filesPanel = new ModernPanel
            {
                Location = new Point(20, 20),
                Size = new Size(700, 600),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            Label lblFilesTitle = new Label
            {
                Text = "📂 Files",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = ModernTheme.SubheadingFont,
                ForeColor = ModernTheme.TextDark
            };

            listViewFiles = new ModernListView
            {
                Location = new Point(15, 50),
                Size = new Size(670, 535),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            listViewFiles.AllowDrop = true;
            listViewFiles.DragEnter += ListViewFiles_DragEnter;
            listViewFiles.DragDrop += ListViewFiles_DragDrop;

            filesPanel.Controls.AddRange(new Control[] { lblFilesTitle, listViewFiles });

            // Preview Panel (Right)
            previewPanel = new ModernPanel
            {
                Location = new Point(740, 20),
                Size = new Size(620, 380),
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            Label lblPreviewTitle = new Label
            {
                Text = "👁️ Preview",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = ModernTheme.SubheadingFont,
                ForeColor = ModernTheme.TextDark
            };

            txtPreview = new TextBox
            {
                Location = new Point(15, 50),
                Size = new Size(590, 315),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Font = ModernTheme.CodeFont,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = ModernTheme.TextDark,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            previewPanel.Controls.AddRange(new Control[] { lblPreviewTitle, txtPreview });

            // Stats Panel (Bottom Right)
            statsPanel = new ModernPanel
            {
                Location = new Point(740, 420),
                Size = new Size(620, 200),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                GradientStart = Color.FromArgb(245, 247, 250),
                GradientEnd = Color.White
            };

            lblStatsTitle = new Label
            {
                Text = "📊 Statistics",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = ModernTheme.SubheadingFont,
                ForeColor = ModernTheme.TextDark
            };

            lblTotalFiles = CreateStatsLabel("📄 Total Files: 0", 50);
            lblTotalSize = CreateStatsLabel("💾 Total Size: 0 B", 85);
            lblTextFiles = CreateStatsLabel("📝 Text Files: 0", 120);
            lblBinaryFiles = CreateStatsLabel("🔒 Binary Files: 0", 155);

            statsPanel.Controls.AddRange(new Control[] {
                lblStatsTitle, lblTotalFiles, lblTotalSize, lblTextFiles, lblBinaryFiles
            });

            mainContainer.Controls.AddRange(new Control[] { filesPanel, previewPanel, statsPanel });

            // Status Bar
            statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = ModernTheme.PrimaryColor
            };

            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(20, 10),
                AutoSize = true,
                Font = ModernTheme.BodyFont,
                ForeColor = Color.White
            };

            progressBar = new ProgressBar
            {
                Location = new Point(statusBar.Width - 220, 10),
                Size = new Size(200, 20),
                Style = ProgressBarStyle.Continuous,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            statusBar.Controls.AddRange(new Control[] { lblStatus, progressBar });
            statusBar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(ModernTheme.PrimaryDark, 2), 0, 0, statusBar.Width, 0);
            };

            // Add all to form
            Controls.Add(mainContainer);
            Controls.Add(statusBar);
            Controls.Add(actionBar);
            Controls.Add(topBar);

            // Handle resize for window buttons
            Resize += (s, e) =>
            {
                btnClose.Location = new Point(Width - 50, 10);
                btnMaximize.Location = new Point(Width - 95, 10);
                btnMinimize.Location = new Point(Width - 140, 10);
                progressBar.Location = new Point(Width - 220, 10);
            };
        }

        private ModernButton CreateWindowButton(string text, Color color)
        {
            return new ModernButton
            {
                Text = text,
                Size = new Size(40, 30),
                NormalColor = Color.Transparent,
                HoverColor = color,
                PressedColor = ModernTheme.Darken(color, 0.2f),
                BorderColor = Color.Transparent,
                BorderRadius = 0,
                ForeColor = ModernTheme.TextDark,
                Font = new Font("Segoe UI", 16F, FontStyle.Regular)
            };
        }

        private ModernButton CreateActionButton(string text, Color color, int x)
        {
            return new ModernButton
            {
                Text = text,
                Location = new Point(x, 10),
                Size = new Size(text.Length > 12 ? 150 : 110, 38),
                NormalColor = color,
                HoverColor = ModernTheme.Lighten(color, 0.1f),
                PressedColor = ModernTheme.Darken(color, 0.1f),
                BorderColor = ModernTheme.Darken(color, 0.2f),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };
        }

        private Label CreateStatsLabel(string text, int top)
        {
            return new Label
            {
                Text = text,
                Location = new Point(15, top),
                AutoSize = true,
                Font = ModernTheme.BodyFont,
                ForeColor = ModernTheme.TextDark
            };
        }

        private void TopBar_Paint(object? sender, PaintEventArgs e)
        {
            // Draw shadow under top bar
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new Rectangle(0, topBar.Height - 5, topBar.Width, 5),
                Color.FromArgb(20, 0, 0, 0),
                Color.FromArgb(0, 0, 0, 0),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, 0, topBar.Height - 5, topBar.Width, 5);
            }
        }

        // Window dragging
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void TopBar_MouseDown(object? sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = Location;
        }

        private void TopBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void TopBar_MouseUp(object? sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void ListViewFiles_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private async void ListViewFiles_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                await _viewModel.HandleFilesDroppedAsync(files);
            }
        }

        private void SetupDataBindings()
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(() => ViewModel_PropertyChanged(sender, e));
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(_viewModel.FilteredFiles):
                    UpdateFileList();
                    break;
                case nameof(_viewModel.StatusMessage):
                    lblStatus.Text = _viewModel.StatusMessage;
                    break;
                case nameof(_viewModel.IsProcessing):
                    btnCancel.Enabled = _viewModel.IsProcessing;
                    progressBar.Visible = _viewModel.IsProcessing;
                    btnAddFile.Enabled = !_viewModel.IsProcessing;
                    btnAddDir.Enabled = !_viewModel.IsProcessing;
                    btnGenerate.Enabled = !_viewModel.IsProcessing;
                    btnClipboard.Enabled = !_viewModel.IsProcessing;
                    break;
                case nameof(_viewModel.ProgressValue):
                    if (progressBar.Maximum > 0)
                        progressBar.Value = Math.Min(_viewModel.ProgressValue, progressBar.Maximum);
                    break;
                case nameof(_viewModel.ProgressMax):
                    progressBar.Maximum = Math.Max(_viewModel.ProgressMax, 1);
                    break;
                case nameof(_viewModel.PreviewContent):
                    txtPreview.Text = _viewModel.PreviewContent;
                    break;
                case nameof(_viewModel.Statistics):
                    UpdateStatistics();
                    break;
            }
        }

        private void UpdateFileList()
        {
            listViewFiles.Clear();
            foreach (var file in _viewModel.FilteredFiles)
            {
                string icon = ModernTheme.GetFileIcon(file.Extension);
                listViewFiles.AddItem(file.FileName, file.Directory, icon);
            }
        }

        private void UpdateStatistics()
        {
            var stats = _viewModel.Statistics;
            lblTotalFiles.Text = $"📄 Total Files: {stats.TotalFiles}";
            lblTotalSize.Text = $"💾 Total Size: {stats.TotalSizeFormatted}";
            lblTextFiles.Text = $"📝 Text Files: {stats.TextFiles}";
            lblBinaryFiles.Text = $"🔒 Binary Files: {stats.BinaryFiles}";
        }

        private void SetupEventHandlers()
        {
            btnAddFile.Click += async (s, e) => await _viewModel.AddFileCommand.ExecuteAsync(null);
            btnAddDir.Click += async (s, e) => await _viewModel.AddDirectoryCommand.ExecuteAsync(null);
            btnClear.Click += (s, e) => _viewModel.ClearFilesCommand.Execute(null);
            btnGenerate.Click += async (s, e) => await _viewModel.GenerateFileCommand.ExecuteAsync(null);
            btnClipboard.Click += async (s, e) => await _viewModel.CopyToClipboardCommand.ExecuteAsync(null);
            btnCancel.Click += (s, e) => _viewModel.CancelOperationCommand.Execute(null);

            txtSearch.TextChanged += (s, e) => _viewModel.SearchText = txtSearch.Text;

            listViewFiles.SelectedIndexChanged += (s, index) =>
            {
                if (index >= 0 && index < _viewModel.FilteredFiles.Count)
                {
                    _viewModel.SelectedFile = _viewModel.FilteredFiles[index];
                }
            };
        }
    }
}
