using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FileManagerApp.Core;
using FileManagerApp.Models;
using FileManagerApp.ViewModels;

namespace FileManagerApp
{
    public partial class MainForm : Form
    {
        private readonly MainViewModel _viewModel;

        // UI Controls
        private ToolStrip toolStrip = null!;
        private ToolStripButton btnAddFile = null!;
        private ToolStripButton btnAddDir = null!;
        private ToolStripButton btnClear = null!;
        private ToolStripButton btnGenerate = null!;
        private ToolStripButton btnClipboard = null!;
        private ToolStripButton btnCancel = null!;

        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel lblStatus = null!;
        private ToolStripProgressBar progressBar = null!;
        private ToolStripStatusLabel lblStats = null!;

        private SplitContainer mainSplitContainer = null!;
        private SplitContainer leftSplitContainer = null!;

        private Panel searchPanel = null!;
        private TextBox txtSearch = null!;
        private Label lblSearch = null!;

        private ListBox listBoxFiles = null!;
        private TextBox txtPreview = null!;
        private Panel statsPanel = null!;
        private Label lblStatsTitle = null!;
        private Label lblTotalFiles = null!;
        private Label lblTotalSize = null!;
        private Label lblTextFiles = null!;
        private Label lblBinaryFiles = null!;

        public MainForm()
        {
            _viewModel = ServiceContainer.GetService<MainViewModel>();
            InitializeComponent();
            SetupDataBindings();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            this.Text = "File Manager Pro";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Create ToolStrip
            toolStrip = new ToolStrip
            {
                ImageScalingSize = new Size(24, 24),
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Padding = new Padding(10, 5, 10, 5)
            };

            btnAddFile = CreateToolStripButton("Add Files", "Add individual files");
            btnAddDir = CreateToolStripButton("Add Directory", "Scan and add directory");
            toolStrip.Items.Add(new ToolStripSeparator());
            btnClear = CreateToolStripButton("Clear", "Clear all files");
            toolStrip.Items.Add(new ToolStripSeparator());
            btnGenerate = CreateToolStripButton("Generate File", "Export to text file");
            btnClipboard = CreateToolStripButton("Copy to Clipboard", "Copy combined content");
            toolStrip.Items.Add(new ToolStripSeparator());
            btnCancel = CreateToolStripButton("Cancel", "Cancel current operation");
            btnCancel.Enabled = false;

            // Create StatusStrip
            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };

            lblStatus = new ToolStripStatusLabel
            {
                Text = "Ready",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White
            };

            progressBar = new ToolStripProgressBar
            {
                Size = new Size(200, 20),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            lblStats = new ToolStripStatusLabel
            {
                Text = "Files: 0",
                ForeColor = Color.White
            };

            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, progressBar, lblStats });

            // Create main split container (Left: Files & Stats | Right: Preview)
            mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 700,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Create left split container (Top: Files | Bottom: Stats)
            leftSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 500,
                BorderStyle = BorderStyle.None
            };

            // Search Panel
            searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(10, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            txtSearch = new TextBox
            {
                Location = new Point(80, 12),
                Width = 500,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Search files by name, path, or extension..."
            };

            searchPanel.Controls.AddRange(new Control[] { lblSearch, txtSearch });

            // File ListBox
            listBoxFiles = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                AllowDrop = true
            };

            var filesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            filesPanel.Controls.Add(listBoxFiles);

            leftSplitContainer.Panel1.Controls.Add(filesPanel);
            leftSplitContainer.Panel1.Controls.Add(searchPanel);

            // Stats Panel
            statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding = new Padding(15)
            };

            lblStatsTitle = new Label
            {
                Text = "Statistics",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 48)
            };

            lblTotalFiles = CreateStatsLabel("Total Files: 0", 50);
            lblTotalSize = CreateStatsLabel("Total Size: 0 B", 80);
            lblTextFiles = CreateStatsLabel("Text Files: 0", 110);
            lblBinaryFiles = CreateStatsLabel("Binary Files: 0", 140);

            statsPanel.Controls.AddRange(new Control[]
            {
                lblStatsTitle, lblTotalFiles, lblTotalSize, lblTextFiles, lblBinaryFiles
            });

            leftSplitContainer.Panel2.Controls.Add(statsPanel);

            // Preview Panel
            var previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblPreview = new Label
            {
                Text = "File Preview",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(230, 230, 230),
                Padding = new Padding(10, 5, 10, 5)
            };

            txtPreview = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9F),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                WordWrap = false
            };

            previewPanel.Controls.Add(txtPreview);
            previewPanel.Controls.Add(lblPreview);

            mainSplitContainer.Panel1.Controls.Add(leftSplitContainer);
            mainSplitContainer.Panel2.Controls.Add(previewPanel);

            // Add all to form
            this.Controls.Add(mainSplitContainer);
            this.Controls.Add(statusStrip);
            this.Controls.Add(toolStrip);
        }

        private ToolStripButton CreateToolStripButton(string text, string tooltip)
        {
            return new ToolStripButton
            {
                Text = text,
                ToolTipText = tooltip,
                ForeColor = Color.White,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(10, 5, 10, 5)
            };
        }

        private Label CreateStatsLabel(string text, int top)
        {
            return new Label
            {
                Text = text,
                Location = new Point(15, top),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
        }

        private void SetupDataBindings()
        {
            // Bind ViewModel properties to UI
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
            listBoxFiles.Items.Clear();
            foreach (var file in _viewModel.FilteredFiles)
            {
                listBoxFiles.Items.Add(file.FullPath);
            }
        }

        private void UpdateStatistics()
        {
            var stats = _viewModel.Statistics;
            lblTotalFiles.Text = $"Total Files: {stats.TotalFiles}";
            lblTotalSize.Text = $"Total Size: {stats.TotalSizeFormatted}";
            lblTextFiles.Text = $"Text Files: {stats.TextFiles}";
            lblBinaryFiles.Text = $"Binary Files: {stats.BinaryFiles}";
            lblStats.Text = stats.GetSummary();
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

            listBoxFiles.SelectedIndexChanged += (s, e) =>
            {
                if (listBoxFiles.SelectedIndex >= 0 && listBoxFiles.SelectedIndex < _viewModel.FilteredFiles.Count)
                {
                    _viewModel.SelectedFile = _viewModel.FilteredFiles[listBoxFiles.SelectedIndex];
                }
            };

            listBoxFiles.DragEnter += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                    e.Effect = DragDropEffects.Copy;
            };

            listBoxFiles.DragDrop += async (s, e) =>
            {
                if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
                {
                    await _viewModel.HandleFilesDroppedAsync(files);
                }
            };

            this.FormClosing += (s, e) =>
            {
                // Save window state
                // Additional cleanup if needed
            };
        }
    }
}
