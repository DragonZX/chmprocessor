namespace ChmProcessor
{
    partial class Settings
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAccept = new System.Windows.Forms.Button();
            this.txtCompilerPath = new System.Windows.Forms.TextBox();
            this.btnSelFile = new System.Windows.Forms.Button();
            this.dlgSelectCompiler = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.chkTidyInput = new System.Windows.Forms.CheckBox();
            this.chtTidyOutput = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtJdk = new System.Windows.Forms.TextBox();
            this.btnSelJdk = new System.Windows.Forms.Button();
            this.dlgSelJdk = new System.Windows.Forms.FolderBrowserDialog();
            this.lnkJdk = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSelJavaHelp = new System.Windows.Forms.Button();
            this.txtJavaHelpPath = new System.Windows.Forms.TextBox();
            this.lnkJavaHelp = new System.Windows.Forms.LinkLabel();
            this.chkSaveRelativePaths = new System.Windows.Forms.CheckBox();
            this.chkReplaceBrokenLinks = new System.Windows.Forms.CheckBox();
            this.chkUseAppLocale = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtAppLocalePath = new System.Windows.Forms.TextBox();
            this.btnSelAppLocate = new System.Windows.Forms.Button();
            this.lnkAppLocate = new System.Windows.Forms.LinkLabel();
            this.dlgSelectAppLocale = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(282, 323);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Microsoft Help Workshop Compiler Path";
            // 
            // btnAccept
            // 
            this.btnAccept.Location = new System.Drawing.Point(201, 323);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 12;
            this.btnAccept.Text = "Accept";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // txtCompilerPath
            // 
            this.txtCompilerPath.Location = new System.Drawing.Point(213, 6);
            this.txtCompilerPath.Name = "txtCompilerPath";
            this.txtCompilerPath.Size = new System.Drawing.Size(296, 20);
            this.txtCompilerPath.TabIndex = 1;
            // 
            // btnSelFile
            // 
            this.btnSelFile.Location = new System.Drawing.Point(515, 4);
            this.btnSelFile.Name = "btnSelFile";
            this.btnSelFile.Size = new System.Drawing.Size(27, 23);
            this.btnSelFile.TabIndex = 2;
            this.btnSelFile.Text = "...";
            this.btnSelFile.UseVisualStyleBackColor = true;
            this.btnSelFile.Click += new System.EventHandler(this.btnSelFile_Click);
            // 
            // dlgSelectCompiler
            // 
            this.dlgSelectCompiler.FileName = "hhc.exe";
            this.dlgSelectCompiler.Filter = "Help Compiler|hhc.exe|All the files|*.*";
            this.dlgSelectCompiler.Title = "Select Help Compiler";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(210, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(335, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "(Usual should be \"C:\\Program Files\\HTML Help Workshop\\hhc.exe\")";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(210, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(165, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "If you dont have it yet, get it here:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(381, 42);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(114, 13);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "HTML Help Workshop";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // chkTidyInput
            // 
            this.chkTidyInput.AutoSize = true;
            this.chkTidyInput.Location = new System.Drawing.Point(213, 58);
            this.chkTidyInput.Name = "chkTidyInput";
            this.chkTidyInput.Size = new System.Drawing.Size(169, 17);
            this.chkTidyInput.TabIndex = 4;
            this.chkTidyInput.Text = "Use Tidy over the source files.";
            this.chkTidyInput.UseVisualStyleBackColor = true;
            // 
            // chtTidyOutput
            // 
            this.chtTidyOutput.AutoSize = true;
            this.chtTidyOutput.Location = new System.Drawing.Point(213, 81);
            this.chtTidyOutput.Name = "chtTidyOutput";
            this.chtTidyOutput.Size = new System.Drawing.Size(188, 17);
            this.chtTidyOutput.TabIndex = 5;
            this.chtTidyOutput.Text = "Use Tidy over the split HTML files.";
            this.chtTidyOutput.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Sun JDK Path (Optional)";
            // 
            // txtJdk
            // 
            this.txtJdk.Location = new System.Drawing.Point(213, 103);
            this.txtJdk.Name = "txtJdk";
            this.txtJdk.Size = new System.Drawing.Size(296, 20);
            this.txtJdk.TabIndex = 6;
            // 
            // btnSelJdk
            // 
            this.btnSelJdk.Location = new System.Drawing.Point(515, 101);
            this.btnSelJdk.Name = "btnSelJdk";
            this.btnSelJdk.Size = new System.Drawing.Size(27, 23);
            this.btnSelJdk.TabIndex = 7;
            this.btnSelJdk.Text = "...";
            this.btnSelJdk.UseVisualStyleBackColor = true;
            this.btnSelJdk.Click += new System.EventHandler(this.btnSelJdk_Click);
            // 
            // lnkJdk
            // 
            this.lnkJdk.AutoSize = true;
            this.lnkJdk.Location = new System.Drawing.Point(210, 126);
            this.lnkJdk.Name = "lnkJdk";
            this.lnkJdk.Size = new System.Drawing.Size(145, 13);
            this.lnkJdk.TabIndex = 8;
            this.lnkJdk.TabStop = true;
            this.lnkJdk.Text = "Jdk can be downloaded here";
            this.lnkJdk.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkJdk_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(170, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Java Help directory path (Optional)";
            // 
            // btnSelJavaHelp
            // 
            this.btnSelJavaHelp.Location = new System.Drawing.Point(515, 145);
            this.btnSelJavaHelp.Name = "btnSelJavaHelp";
            this.btnSelJavaHelp.Size = new System.Drawing.Size(27, 23);
            this.btnSelJavaHelp.TabIndex = 10;
            this.btnSelJavaHelp.Text = "...";
            this.btnSelJavaHelp.UseVisualStyleBackColor = true;
            this.btnSelJavaHelp.Click += new System.EventHandler(this.btnSelJavaHelp_Click);
            // 
            // txtJavaHelpPath
            // 
            this.txtJavaHelpPath.Location = new System.Drawing.Point(213, 147);
            this.txtJavaHelpPath.Name = "txtJavaHelpPath";
            this.txtJavaHelpPath.Size = new System.Drawing.Size(296, 20);
            this.txtJavaHelpPath.TabIndex = 9;
            // 
            // lnkJavaHelp
            // 
            this.lnkJavaHelp.AutoSize = true;
            this.lnkJavaHelp.Location = new System.Drawing.Point(210, 170);
            this.lnkJavaHelp.Name = "lnkJavaHelp";
            this.lnkJavaHelp.Size = new System.Drawing.Size(176, 13);
            this.lnkJavaHelp.TabIndex = 11;
            this.lnkJavaHelp.TabStop = true;
            this.lnkJavaHelp.Text = "Java Help can be downloaded here";
            this.lnkJavaHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkJavaHelp_LinkClicked);
            // 
            // chkSaveRelativePaths
            // 
            this.chkSaveRelativePaths.AutoSize = true;
            this.chkSaveRelativePaths.Location = new System.Drawing.Point(213, 196);
            this.chkSaveRelativePaths.Name = "chkSaveRelativePaths";
            this.chkSaveRelativePaths.Size = new System.Drawing.Size(172, 17);
            this.chkSaveRelativePaths.TabIndex = 15;
            this.chkSaveRelativePaths.Text = "Save relative paths on projects";
            this.chkSaveRelativePaths.UseVisualStyleBackColor = true;
            // 
            // chkReplaceBrokenLinks
            // 
            this.chkReplaceBrokenLinks.AutoSize = true;
            this.chkReplaceBrokenLinks.Location = new System.Drawing.Point(213, 220);
            this.chkReplaceBrokenLinks.Name = "chkReplaceBrokenLinks";
            this.chkReplaceBrokenLinks.Size = new System.Drawing.Size(203, 17);
            this.chkReplaceBrokenLinks.TabIndex = 16;
            this.chkReplaceBrokenLinks.Text = "Remove/replace broken internal links";
            this.chkReplaceBrokenLinks.UseVisualStyleBackColor = true;
            // 
            // chkUseAppLocale
            // 
            this.chkUseAppLocale.AutoSize = true;
            this.chkUseAppLocale.Location = new System.Drawing.Point(213, 243);
            this.chkUseAppLocale.Name = "chkUseAppLocale";
            this.chkUseAppLocale.Size = new System.Drawing.Size(244, 17);
            this.chkUseAppLocale.TabIndex = 17;
            this.chkUseAppLocale.Text = "Use Microsoft AppLocale to compile CHM files";
            this.chkUseAppLocale.UseVisualStyleBackColor = true;
            this.chkUseAppLocale.CheckedChanged += new System.EventHandler(this.chkUseAppLocate_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 269);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "AppLocale path";
            // 
            // txtAppLocalePath
            // 
            this.txtAppLocalePath.Location = new System.Drawing.Point(213, 266);
            this.txtAppLocalePath.Name = "txtAppLocalePath";
            this.txtAppLocalePath.Size = new System.Drawing.Size(296, 20);
            this.txtAppLocalePath.TabIndex = 19;
            // 
            // btnSelAppLocate
            // 
            this.btnSelAppLocate.Location = new System.Drawing.Point(515, 263);
            this.btnSelAppLocate.Name = "btnSelAppLocate";
            this.btnSelAppLocate.Size = new System.Drawing.Size(27, 23);
            this.btnSelAppLocate.TabIndex = 20;
            this.btnSelAppLocate.Text = "...";
            this.btnSelAppLocate.UseVisualStyleBackColor = true;
            this.btnSelAppLocate.Click += new System.EventHandler(this.btnSelAppLocate_Click);
            // 
            // lnkAppLocate
            // 
            this.lnkAppLocate.AutoSize = true;
            this.lnkAppLocate.Location = new System.Drawing.Point(210, 289);
            this.lnkAppLocate.Name = "lnkAppLocate";
            this.lnkAppLocate.Size = new System.Drawing.Size(202, 13);
            this.lnkAppLocate.TabIndex = 21;
            this.lnkAppLocate.TabStop = true;
            this.lnkAppLocate.Text = "AppLocale can be downloaded from here";
            this.lnkAppLocate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkAppLocate_LinkClicked);
            // 
            // dlgSelectAppLocale
            // 
            this.dlgSelectAppLocale.FileName = "AppLoc.exe";
            this.dlgSelectAppLocale.Filter = "AppLocale executable|AppLoc.exe|All the files|*.*";
            this.dlgSelectAppLocale.Title = "Select Help Compiler";
            // 
            // Settings
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(559, 358);
            this.Controls.Add(this.lnkAppLocate);
            this.Controls.Add(this.btnSelAppLocate);
            this.Controls.Add(this.txtAppLocalePath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkUseAppLocale);
            this.Controls.Add(this.chkReplaceBrokenLinks);
            this.Controls.Add(this.chkSaveRelativePaths);
            this.Controls.Add(this.lnkJavaHelp);
            this.Controls.Add(this.btnSelJavaHelp);
            this.Controls.Add(this.txtJavaHelpPath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lnkJdk);
            this.Controls.Add(this.btnSelJdk);
            this.Controls.Add(this.txtJdk);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chtTidyOutput);
            this.Controls.Add(this.chkTidyInput);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelFile);
            this.Controls.Add(this.txtCompilerPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAccept);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.Settings_HelpRequested);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.TextBox txtCompilerPath;
        private System.Windows.Forms.Button btnSelFile;
        private System.Windows.Forms.OpenFileDialog dlgSelectCompiler;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox chkTidyInput;
        private System.Windows.Forms.CheckBox chtTidyOutput;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtJdk;
        private System.Windows.Forms.Button btnSelJdk;
        private System.Windows.Forms.FolderBrowserDialog dlgSelJdk;
        private System.Windows.Forms.LinkLabel lnkJdk;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSelJavaHelp;
        private System.Windows.Forms.TextBox txtJavaHelpPath;
        private System.Windows.Forms.LinkLabel lnkJavaHelp;
        private System.Windows.Forms.CheckBox chkSaveRelativePaths;
        private System.Windows.Forms.CheckBox chkReplaceBrokenLinks;
        private System.Windows.Forms.CheckBox chkUseAppLocale;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtAppLocalePath;
        private System.Windows.Forms.Button btnSelAppLocate;
        private System.Windows.Forms.LinkLabel lnkAppLocate;
        private System.Windows.Forms.OpenFileDialog dlgSelectAppLocale;
    }
}