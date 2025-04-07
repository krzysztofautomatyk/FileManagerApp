using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
// Dodaj using dla FileSystemGlobbing
using Microsoft.Extensions.FileSystemGlobbing;
// Potrzebujemy też abstrakcji, jeśli nie są globalnie dostępne
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace FileManagerApp
{
    // --- Upewnij się, że masz te klasy pomocnicze ---
    // (Jeśli używasz .NET Core/.NET 5+, powinny być dostępne z pakietu NuGet)
    // (Jeśli używasz .NET Framework, być może trzeba je dodać ręcznie lub znaleźć w pakiecie)
    // Proste implementacje poniżej, jeśli ich brakuje:
    #region FileSystemGlobbing Abstractions (jeśli potrzebne)

    public abstract class FileSystemInfoBase
    {
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public abstract DirectoryInfoBase ParentDirectory { get; }
    }

    public abstract class DirectoryInfoBase : FileSystemInfoBase
    {
        public abstract IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos();
        public abstract DirectoryInfoBase GetDirectory(string path);
        public abstract FileInfoBase GetFile(string path);
    }

    public class DirectoryInfoWrapper : DirectoryInfoBase
    {
        private readonly DirectoryInfo _info;
        public DirectoryInfoWrapper(DirectoryInfo info) { _info = info ?? throw new ArgumentNullException(nameof(info)); }
        public override string Name => _info.Name;
        public override string FullName => _info.FullName;
        public override DirectoryInfoBase ParentDirectory => _info.Parent == null ? null : new DirectoryInfoWrapper(_info.Parent);
        public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos() =>
            _info.EnumerateFileSystemInfos().Select<FileSystemInfo, FileSystemInfoBase>(info => info switch {
                FileInfo fileInfo => new FileInfoWrapper(fileInfo),
                DirectoryInfo dirInfo => new DirectoryInfoWrapper(dirInfo),
                _ => null // Ignoruj inne typy, jeśli istnieją
            }).Where(fsi => fsi != null); // Filtruj null

        // Uproszczone implementacje GetDirectory/GetFile - mogą wymagać dostosowania
        public override DirectoryInfoBase GetDirectory(string path)
        {
            try
            {
                var dirs = _info.GetDirectories(path);
                return dirs.Length > 0 ? new DirectoryInfoWrapper(dirs[0]) : null;
            }
            catch { return null; } // Obsługa błędów dostępu itp.
        }
        public override FileInfoBase GetFile(string path)
        {
            try
            {
                var files = _info.GetFiles(path);
                return files.Length > 0 ? new FileInfoWrapper(files[0]) : null;
            }
            catch { return null; } // Obsługa błędów dostępu itp.
        }
    }

    public abstract class FileInfoBase : FileSystemInfoBase { }

    public class FileInfoWrapper : FileInfoBase
    {
        private readonly FileInfo _info;
        public FileInfoWrapper(FileInfo info) { _info = info ?? throw new ArgumentNullException(nameof(info)); }
        public override string Name => _info.Name;
        public override string FullName => _info.FullName;
        public override DirectoryInfoBase ParentDirectory => _info.Directory == null ? null : new DirectoryInfoWrapper(_info.Directory);
    }

    #endregion


    public partial class FileManagerApp : Form
    {
        private List<string> files = new List<string>();
        // Usunięto cache Matcherów - upraszcza logikę, parsing on-the-fly
        // private Dictionary<string, Matcher> gitignoreMatcherCache = new Dictionary<string, Matcher>(StringComparer.OrdinalIgnoreCase);

        public FileManagerApp()
        {
            InitializeComponent();
            listBoxFiles.DragEnter += ListBoxFiles_DragEnter;
            listBoxFiles.DragDrop += ListBoxFiles_DragDrop;
            btnAddFile.Click += BtnAddFile_Click;
            btnAddDir.Click += BtnAddDir_Click;
            btnClear.Click += BtnClear_Click;
            btnGenerate.Click += BtnGenerate_Click;
            btnClipboard.Click += BtnClipboard_Click;
        }

        // --- Metody obsługi przycisków i Drag&Drop (bez zmian w logice wywołań IsIgnored) ---
        private void BtnAddFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in ofd.FileNames)
                    {
                        if (!IsIgnored(file)) AddFileToList(file);
                        else Console.WriteLine($"Ignored (manual add): {file}");
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
                    // Sprawdź sam folder przed przetworzeniem
                    if (!IsIgnored(fbd.SelectedPath)) ProcessDirectory(fbd.SelectedPath);
                    else Console.WriteLine($"Ignored directory (browse): {fbd.SelectedPath}");
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            files.Clear();
            listBoxFiles.Items.Clear();
            // Nie ma już cache'u do czyszczenia
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (files.Count == 0) { /* ... (bez zmian) ... */ return; }
            using (SaveFileDialog sfd = new SaveFileDialog()) { /* ... (bez zmian) ... */ }
        }

        private void BtnClipboard_Click(object sender, EventArgs e)
        {
            if (files.Count == 0) { /* ... (bez zmian) ... */ return; }
            var content = GetCombinedFileContent();
            Clipboard.SetText(content);
            MessageBox.Show("Zawartość została skopiowana do schowka.", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ListBoxFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void ListBoxFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string item in droppedItems)
            {
                if (Directory.Exists(item))
                {
                    if (!IsIgnored(item)) ProcessDirectory(item);
                    else Console.WriteLine($"Ignored directory (drag drop): {item}");
                }
                else if (File.Exists(item))
                {
                    if (!IsIgnored(item)) AddFileToList(item);
                    else Console.WriteLine($"Ignored file (drag drop): {item}");
                }
            }
        }

        // --- Metody pomocnicze (AddFileToList, ProcessDirectory - bez zmian) ---
        private void AddFileToList(string filePath)
        {
            if (!files.Contains(filePath))
            {
                files.Add(filePath);
                if (listBoxFiles.InvokeRequired) listBoxFiles.Invoke(new Action(() => listBoxFiles.Items.Add(filePath)));
                else listBoxFiles.Items.Add(filePath);
            }
        }

        private void ProcessDirectory(string dirPath)
        {
            Console.WriteLine($"Processing directory: {dirPath}");
            try
            {
                foreach (string filePath in Directory.EnumerateFiles(dirPath))
                {
                    if (!IsIgnored(filePath)) AddFileToList(filePath);
                    else Console.WriteLine($"Ignored file: {filePath}");
                }
                foreach (string subDirPath in Directory.EnumerateDirectories(dirPath))
                {
                    // WAŻNE: Sprawdź ignorowanie podkatalogu PRZED rekurencją
                    if (!IsIgnored(subDirPath)) ProcessDirectory(subDirPath);
                    else Console.WriteLine($"Ignored directory recursion: {subDirPath}");
                }
            }
            catch (UnauthorizedAccessException ex) { Console.WriteLine($"Access denied: {dirPath} - {ex.Message}"); }
            catch (Exception ex) { Console.WriteLine($"Error processing directory {dirPath}: {ex.Message}"); }
        }


        // --- NOWA Logika ignorowania ---
        private bool IsIgnored(string fullPath)
        {
            // --- Sprawdzenia bezwarunkowe ---

            string fileName = Path.GetFileName(fullPath); // Nazwa pliku lub ostatniego katalogu w ścieżce
            bool isDirectory = Directory.Exists(fullPath); // Sprawdź raz

            // 1. Ignoruj konkretne nazwy plików (case-insensitive)
            var ignoredFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".gitattributes",
                ".gitignore"
                // Możesz tu dodać więcej konkretnych nazw plików
            };
            if (!isDirectory && ignoredFileNames.Contains(fileName))
            {
                Console.WriteLine($"Ignored ({fileName}): Is an explicitly ignored file name.");
                return true;
            }

            // 2. Ignoruj pliki .db
            if (fullPath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Ignored ({fileName}): Matches *.db");
                return true;
            }

            // 3. Ignoruj konkretne katalogi i ich zawartość (case-insensitive)
            var ignoredDirNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".git",
                "bin",
                "obj",
                ".github",  // Dodano
                "Migrations" // Dodano
            };

            // Sprawdź, czy sama ścieżka jest jednym z ignorowanych katalogów
            if (isDirectory && ignoredDirNames.Contains(fileName))
            {
                Console.WriteLine($"Ignored ({fileName}): Is an explicitly ignored directory name.");
                return true;
            }

            // Sprawdź, czy ścieżka zawiera jeden z ignorowanych katalogów jako segment
            string normalizedPath = fullPath.Replace(Path.DirectorySeparatorChar, '/');
            foreach (string dirToIgnore in ignoredDirNames)
            {
                // Szukaj '/nazwa_katalogu/' w ścieżce
                if (normalizedPath.Contains($"/{dirToIgnore}/", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Ignored ({fileName}): Path contains '/{dirToIgnore}/' segment.");
                    return true;
                }
                // Sprawdź, czy ścieżka zaczyna się od 'nazwa_katalogu/'
                if (normalizedPath.StartsWith($"{dirToIgnore}/", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Ignored ({fileName}): Path starts with '{dirToIgnore}/'.");
                    return true;
                }
            }


            // --- Sprawdzenia oparte na .gitignore (jeśli nie zignorowano wcześniej) ---

            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directoryPath)) directoryPath = Environment.CurrentDirectory;
            else if (!Path.IsPathRooted(directoryPath)) directoryPath = Path.Combine(Environment.CurrentDirectory, directoryPath);

            string currentDir = directoryPath;
            string rootDir = Path.GetPathRoot(fullPath);
            if (string.IsNullOrEmpty(rootDir))
            {
                int levelsToRoot = 10; string tempDir = currentDir;
                while (levelsToRoot-- > 0 && !string.IsNullOrEmpty(Path.GetDirectoryName(tempDir))) { tempDir = Path.GetDirectoryName(tempDir); }
                rootDir = tempDir;
            }

            int level = 0;
            int lastIgnoreLevel = -1;
            int lastNegateLevel = -1;
            string lastIgnorePattern = null;
            string lastNegatePattern = null;
            bool itemIsDir = isDirectory; // Użyj wcześniej sprawdzonej wartości

            while (!string.IsNullOrEmpty(currentDir) && currentDir.Length >= rootDir.Length)
            {
                string gitignorePath = Path.Combine(currentDir, ".gitignore");
                if (File.Exists(gitignorePath))
                {
                    try
                    {
                        var lines = File.ReadAllLines(gitignorePath);
                        string lastMatchingPatternForThisFile = null;
                        bool lastMatchWasNegate = false;

                        foreach (var rawLine in lines)
                        {
                            string trimmedLine = rawLine.Trim();
                            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#")) continue;
                            bool isNegatePattern = trimmedLine.StartsWith("!");
                            string pattern = isNegatePattern ? trimmedLine.Substring(1) : trimmedLine;
                            if (DoesPathMatchPattern(currentDir, pattern, fullPath, itemIsDir))
                            {
                                lastMatchingPatternForThisFile = rawLine;
                                lastMatchWasNegate = isNegatePattern;
                            }
                        }
                        if (lastMatchingPatternForThisFile != null)
                        {
                            if (lastMatchWasNegate) { lastNegateLevel = level; lastNegatePattern = lastMatchingPatternForThisFile; }
                            else { lastIgnoreLevel = level; lastIgnorePattern = lastMatchingPatternForThisFile; }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine($"Error reading or processing {gitignorePath}: {ex.Message}"); }
                }
                if (currentDir.Equals(rootDir, StringComparison.OrdinalIgnoreCase)) break;
                string parentDir = Path.GetDirectoryName(currentDir);
                if (string.Equals(parentDir, currentDir, StringComparison.OrdinalIgnoreCase)) break;
                currentDir = parentDir;
                level++;
            }

            // Określ ostateczny status na podstawie poziomów .gitignore
            bool ignoredByGitignore;
            if (lastNegateLevel != -1 && lastNegateLevel <= lastIgnoreLevel) { ignoredByGitignore = false; /* Logika negacji */ }
            else if (lastIgnoreLevel != -1) { ignoredByGitignore = true; /* Logika ignorowania */ }
            else { ignoredByGitignore = false; /* Brak dopasowań */ }

            // Logowanie wyniku z .gitignore (opcjonalne, można usunąć lub dostosować)
            if (ignoredByGitignore) Console.WriteLine($"Final decision for {fileName} (gitignore): Ignored (Pattern: '{lastIgnorePattern}')");
            else if (lastNegateLevel > lastIgnoreLevel) Console.WriteLine($"Final decision for {fileName} (gitignore): Not Ignored (Negated by: '{lastNegatePattern}')");
            // else Console.WriteLine($"Final decision for {fileName} (gitignore): Not Ignored (No matching patterns)");


            return ignoredByGitignore; // Zwróć wynik z logiki .gitignore
        }

        // --- Funkcja pomocnicza DoesPathMatchPattern (bez zmian) ---
        private bool DoesPathMatchPattern(string basePath, string pattern, string fullPathToCheck, bool itemIsDir)
        {
            // ... (implementacja bez zmian) ...
            // Normalizuj wzorzec
            pattern = pattern.Trim();
            bool patternTargetsDir = pattern.EndsWith("/");
            pattern = pattern.TrimEnd('/');
            // Wzorzec zaczynający się od '/' jest zakotwiczony do basePath
            bool patternAnchored = pattern.StartsWith("/");
            pattern = pattern.TrimStart('/');

            // Oblicz ścieżkę względną
            string relativePath = Path.GetRelativePath(basePath, fullPathToCheck);
            relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/'); // Użyj '/'

            // Jeśli wzorzec jest zakotwiczony, ścieżka względna musi zaczynać się od niego
            if (patternAnchored)
            {
                // Użyj Matcher do sprawdzenia dopasowania zakotwiczonego
                var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
                matcher.AddInclude("/" + pattern); // Dodaj z powrotem '/' na początku

                // Sprawdź dopasowanie do ścieżki względnej (też z '/')
                // Matcher.Match oczekuje ścieżki bez wiodącego '/'
                return matcher.Match(relativePath).HasMatches;
            }
            else // Wzorzec niezakotwiczony - może pasować w dowolnym miejscu ścieżki
            {
                // Wzorzec może zawierać '**'
                // Użyj Matcher do obsługi globbingu (np. **, *)
                var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
                // Dodaj wzorzec tak, aby mógł pasować w dowolnym podkatalogu
                matcher.AddInclude("**/" + pattern); // Dopasuj w podkatalogach
                matcher.AddInclude(pattern);      // Dopasuj w bieżącym katalogu (basePath)

                bool matches = matcher.Match(relativePath).HasMatches;

                // Dodatkowa obsługa dla wzorców katalogów (np. 'logs' powinno pasować do 'logs/file.txt')
                if (!matches && itemIsDir && !pattern.Contains("/"))
                {
                    // Jeśli wzorzec to prosta nazwa (bez '/') i element jest katalogiem,
                    // sprawdź, czy nazwa katalogu pasuje do wzorca.
                    if (Path.GetFileName(fullPathToCheck).Equals(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        matches = true;
                    }
                }
                // Jeśli wzorzec jawnie celuje w katalog (kończył się na '/'),
                // a sprawdzany element nie jest katalogiem, to nie pasuje.
                if (patternTargetsDir && !itemIsDir)
                {
                    matches = false;
                }


                return matches;
            }
        }


        // --- Pozostałe metody (GetCombinedFileContent, GenerateTextFile - bez zmian) ---
        private void GenerateTextFile(string outputPath)
        {
            var content = GetCombinedFileContent();
            try { File.WriteAllText(outputPath, content, Encoding.UTF8); }
            catch (Exception ex) { MessageBox.Show($"Błąd zapisu pliku: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private string GetCombinedFileContent()
        {
            StringBuilder sb = new StringBuilder();
            var sortedFiles = files.OrderBy(f => f).ToList(); // Sortowanie dla spójności

            foreach (var file in sortedFiles)
            {
                sb.AppendLine($"--- {file} ---");
                try
                {
                    string text = "";
                    try
                    {
                        text = File.ReadAllText(file, Encoding.UTF8);
                        if (text.Contains('\0')) throw new Exception("Detected null character, likely binary.");
                    }
                    catch // Spróbuj domyślnego kodowania, jeśli UTF-8 zawiedzie lub wykryto binarny
                    {
                        try
                        {
                            text = File.ReadAllText(file, Encoding.Default);
                            if (text.Contains('\0')) throw new Exception("Detected null character, likely binary.");
                        }
                        catch (Exception innerEx)
                        {
                            sb.AppendLine("[Plik binarny lub nieczytelny - zawartość pominięta]");
                            sb.AppendLine($"[Błąd odczytu: {innerEx.Message}]");
                            sb.AppendLine(); continue;
                        }
                    }
                    sb.AppendLine(text);
                }
                catch (Exception ex) // Ogólny błąd (np. brak dostępu)
                {
                    sb.AppendLine("[Nie można odczytać pliku - zawartość pominięta]");
                    sb.AppendLine($"[Błąd: {ex.Message}]");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        // Pusta metoda - można usunąć
        private void btnClipboard_Click_1(object sender, EventArgs e) { }
    }
}