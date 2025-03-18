using System.Text;

namespace FileManagerApp
{
    public partial class FileManagerApp : Form
    {
        private List<string> files = new List<string>();

        public FileManagerApp()
        {
            InitializeComponent();
            // Obsługa przeciągania i upuszczania
            listBoxFiles.DragEnter += ListBoxFiles_DragEnter;
            listBoxFiles.DragDrop += ListBoxFiles_DragDrop;

            // Przypisanie obsługi zdarzeń przyciskom
            btnAddFile.Click += BtnAddFile_Click;
            btnAddDir.Click += BtnAddDir_Click;
            btnClear.Click += BtnClear_Click;
            btnGenerate.Click += BtnGenerate_Click;
            btnClipboard.Click += BtnClipboard_Click;
        }

        private void BtnAddFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in ofd.FileNames)
                    {
                        AddFile(file);
                    }
                }
            }
        }

        private void BtnAddDir_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    AddDirectory(fbd.SelectedPath);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            files.Clear();
            listBoxFiles.Items.Clear();
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (files.Count == 0)
            {
                MessageBox.Show("Brak plików do przetworzenia.", "Uwaga", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Pliki tekstowe (*.txt)|*.txt";
                sfd.DefaultExt = "txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    GenerateTextFile(sfd.FileName);
                    MessageBox.Show("Plik tekstowy został wygenerowany.", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnClipboard_Click(object sender, EventArgs e)
        {
            if (files.Count == 0)
            {
                MessageBox.Show("Brak plików do przetworzenia.", "Uwaga", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var content = GetCombinedFileContent();
            Clipboard.SetText(content);

            MessageBox.Show("Zawartość została skopiowana do schowka.", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ListBoxFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void ListBoxFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string item in droppedItems)
            {
                if (Directory.Exists(item))
                    AddDirectory(item);
                else if (File.Exists(item))
                    AddFile(item);
            }
        }

        private void AddFile(string filePath)
        {
            if (!files.Contains(filePath))
            {
                files.Add(filePath);
                listBoxFiles.Items.Add(filePath);
            }
        }

        private void AddDirectory(string dirPath)
        {
            var allFiles = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
                AddFile(file);
        }

        private void GenerateTextFile(string outputPath)
        {
            var content = GetCombinedFileContent();

            File.WriteAllText(outputPath, content, Encoding.UTF8);
        }

        private string GetCombinedFileContent()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var file in files)
            {
                sb.AppendLine($"--- {file} ---");

                try
                {
                    string text = File.ReadAllText(file, Encoding.UTF8);
                    sb.AppendLine(text);
                }
                catch (Exception ex)
                {
                    sb.AppendLine("[Plik binarny lub nieczytelny - zawartość pominięta]");
                    sb.AppendLine($"[Błąd: {ex.Message}]");
                }

                sb.AppendLine(); // dodatkowa linia odstępu
            }

            return sb.ToString();
        }
    }
}