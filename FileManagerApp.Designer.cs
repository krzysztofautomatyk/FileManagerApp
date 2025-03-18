namespace FileManagerApp
{
    partial class FileManagerApp
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileManagerApp));
            listBoxFiles = new ListBox();
            btnAddFile = new Button();
            btnAddDir = new Button();
            btnClear = new Button();
            btnGenerate = new Button();
            btnClipboard = new Button();
            SuspendLayout();
            // 
            // listBoxFiles
            // 
            listBoxFiles.AllowDrop = true;
            listBoxFiles.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxFiles.FormattingEnabled = true;
            listBoxFiles.Location = new Point(12, 74);
            listBoxFiles.Name = "listBoxFiles";
            listBoxFiles.Size = new Size(776, 364);
            listBoxFiles.TabIndex = 0;
            listBoxFiles.DragDrop += ListBoxFiles_DragDrop;
            listBoxFiles.DragEnter += ListBoxFiles_DragEnter;
            // 
            // btnAddFile
            // 
            btnAddFile.Cursor = Cursors.Hand;
            btnAddFile.Location = new Point(12, 12);
            btnAddFile.Name = "btnAddFile";
            btnAddFile.Size = new Size(110, 39);
            btnAddFile.TabIndex = 1;
            btnAddFile.Text = "Add File";
            btnAddFile.UseVisualStyleBackColor = true;
            // 
            // btnAddDir
            // 
            btnAddDir.Cursor = Cursors.Hand;
            btnAddDir.Location = new Point(142, 12);
            btnAddDir.Name = "btnAddDir";
            btnAddDir.Size = new Size(110, 39);
            btnAddDir.TabIndex = 2;
            btnAddDir.Text = "Add Dir";
            btnAddDir.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            btnClear.Cursor = Cursors.Hand;
            btnClear.Location = new Point(270, 12);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(110, 39);
            btnClear.TabIndex = 3;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnGenerate
            // 
            btnGenerate.Cursor = Cursors.Hand;
            btnGenerate.Location = new Point(397, 12);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(166, 39);
            btnGenerate.TabIndex = 4;
            btnGenerate.Text = "Generate file";
            btnGenerate.UseVisualStyleBackColor = true;
            // 
            // btnClipboard
            // 
            btnClipboard.Cursor = Cursors.Hand;
            btnClipboard.Location = new Point(625, 12);
            btnClipboard.Name = "btnClipboard";
            btnClipboard.Size = new Size(163, 39);
            btnClipboard.TabIndex = 5;
            btnClipboard.Text = "Copy to Clipboard";
            btnClipboard.UseVisualStyleBackColor = true;
            // 
            // FileManagerApp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnClipboard);
            Controls.Add(btnGenerate);
            Controls.Add(btnClear);
            Controls.Add(btnAddDir);
            Controls.Add(btnAddFile);
            Controls.Add(listBoxFiles);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FileManagerApp";
            Text = "FileManagerApp";
            TopMost = true;
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBoxFiles;
        private Button btnAddFile;
        private Button btnAddDir;
        private Button btnClear;
        private Button btnGenerate;
        private Button btnClipboard;
    }
}
