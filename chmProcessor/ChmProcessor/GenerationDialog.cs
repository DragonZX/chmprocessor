/* 
 * chmProcessor - Word converter to CHM
 * Copyright (C) 2008 Toni Bennasar Obrador
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using ChmProcessorLib;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace ChmProcessor
{
	/// <summary>
	/// Dialog to show the CHM generation status progress
	/// </summary>
	public class GenerationDialog : System.Windows.Forms.Form
	{
        /// <summary>
        /// Item to show on the errors list
        /// </summary>
        private class ExceptionItem
        {
            public Exception exception;
            public override string ToString() { return exception.Message; }
            public ExceptionItem(Exception exception) { this.exception = exception; }
        }

        /// <summary>
        /// CHM project to generate
        /// </summary>
        private ChmProject project;

        /// <summary>
        /// Help generator
        /// </summary>
        private DocumentProcessor processor;

        /// <summary>
        /// Should we close the window after the generation ends?
        /// </summary>
        private bool exitAfterEnd;

        /// <summary>
        /// Worker for the generation process, to avoid get the user interface hang up.
        /// </summary>
        private BackgroundWorker bgWorker;

        /// <summary>
        /// Generation process finished?
        /// </summary>
        private bool finished;

        /// <summary>
        /// Generation process failed?
        /// </summary>
        private bool failed;

        /// <summary>
        /// If generation process failed, this is the exception generated.
        /// </summary>
        private Exception exceptionFail;

        /// <summary>
        /// User interface to show the log on the window
        /// </summary>
        private GenerationDialogUserInterface UI;

        /// <summary>
        /// Can we show prompts and dialogs to the user?
        /// </summary>
        private bool askConfirmations;

        #region User interface controls
        private System.ComponentModel.Container components = null;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TextBox txtLog;
        private TabPage tabPage2;
        private ListBox lstErrors;
        private Button btnErrorDetails;
        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.PictureBox pic;
        #endregion

        public GenerationDialog(ChmProject project, bool exitAfterEnd, bool askConfirmations, int LogLevel)
		{
			InitializeComponent();

            this.project = project;
            this.exitAfterEnd = exitAfterEnd;
            this.askConfirmations = askConfirmations;

            this.UI = new GenerationDialogUserInterface(this);
            this.UI.LogLevel = LogLevel;

            this.processor = new DocumentProcessor(project);
            this.processor.UI = UI;

            bgWorker.RunWorkerAsync();
		}

		/// <summary>
		/// Limpiar los recursos que se estén utilizando.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Código generado por el Diseñador de Windows Forms
		/// <summary>
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido del método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenerationDialog));
            this.btnAceptar = new System.Windows.Forms.Button();
            this.pic = new System.Windows.Forms.PictureBox();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnErrorDetails = new System.Windows.Forms.Button();
            this.lstErrors = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAceptar
            // 
            this.btnAceptar.Image = ((System.Drawing.Image)(resources.GetObject("btnAceptar.Image")));
            this.btnAceptar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAceptar.Location = new System.Drawing.Point(206, 381);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(173, 40);
            this.btnAceptar.TabIndex = 0;
            this.btnAceptar.Text = "Cancel";
            this.btnAceptar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // pic
            // 
            this.pic.Location = new System.Drawing.Point(144, 381);
            this.pic.Name = "pic";
            this.pic.Size = new System.Drawing.Size(56, 40);
            this.pic.TabIndex = 3;
            this.pic.TabStop = false;
            // 
            // bgWorker
            // 
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(561, 363);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtLog);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(553, 337);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(6, 6);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(541, 325);
            this.txtLog.TabIndex = 5;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnErrorDetails);
            this.tabPage2.Controls.Add(this.lstErrors);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(553, 337);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Errors";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnErrorDetails
            // 
            this.btnErrorDetails.Location = new System.Drawing.Point(6, 308);
            this.btnErrorDetails.Name = "btnErrorDetails";
            this.btnErrorDetails.Size = new System.Drawing.Size(127, 23);
            this.btnErrorDetails.TabIndex = 1;
            this.btnErrorDetails.Text = "Show error details";
            this.btnErrorDetails.UseVisualStyleBackColor = true;
            this.btnErrorDetails.Click += new System.EventHandler(this.btnErrorDetails_Click);
            // 
            // lstErrors
            // 
            this.lstErrors.FormattingEnabled = true;
            this.lstErrors.HorizontalScrollbar = true;
            this.lstErrors.Location = new System.Drawing.Point(6, 6);
            this.lstErrors.Name = "lstErrors";
            this.lstErrors.Size = new System.Drawing.Size(541, 290);
            this.lstErrors.TabIndex = 0;
            this.lstErrors.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstErrors_MouseDoubleClick);
            // 
            // GenerationDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(585, 433);
            this.ControlBox = false;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.pic);
            this.Controls.Add(this.btnAceptar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "GenerationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "chmProcessor  - Generating help...";
            ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion

        private void btnAceptar_Click(object sender, System.EventArgs e)
        {
            if (finished)
                this.Close();
            else
            {
                if (MessageBox.Show("Are you sure you want to cancel the generation?", "Cancel Generation", MessageBoxButtons.YesNo )
                    == DialogResult.Yes)
                {
                    // Cancel the process:
                    btnAceptar.Enabled = false;
                    bgWorker.CancelAsync();
                }
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                DateTime startTime = DateTime.Now;

                processor.GenerateHelp();
                if (UI.CancellRequested())
                {
                    UI.log("PROCESS CANCELLED", 1);
                    return;
                }

                UI.log("DONE!", 1);

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                UI.log("Total time: " + duration.ToString(), 2);
            }
            catch (Exception ex)
            {
                //failed = true;
                exceptionFail = ex;
            }

            if (processor.GenerationExceptions.Count > 0)
                failed = true;
        }

        /// <summary>
        /// Writes a text on the window log
        /// </summary>
        /// <param name="text">Text to write</param>
        public void Log(string text)
        {
            if (bgWorker.IsBusy)
                bgWorker.ReportProgress(0, text);
            else
                InternalLog(text);
        }

        /// <summary>
        /// Writes a text on the window
        /// </summary>
        /// <param name="text">Text to write</param>
        private void InternalLog(string text)
        {
            txtLog.AppendText(text + "\r\n");
            Console.WriteLine(text);
        }

        /// <summary>
        /// Writes an exception on the window
        /// </summary>
        /// <param name="text">Text to write</param>
        public void Log(Exception exception)
        {
            if (bgWorker.IsBusy)
                bgWorker.ReportProgress(0, exception);
            else
                Log(exception);
        }

        /// <summary>
        /// Writes an exception on the window
        /// </summary>
        /// <param name="text">Text to write</param>
        private void InternalLog(Exception exception)
        {
            lstErrors.Items.Add(new ExceptionItem(exception));
            Console.WriteLine(exception.ToString());
        }

        /// <summary>
        /// User has pressed the cancel button?
        /// </summary>
        public bool CancellationPending
        {
            get
            {
                return bgWorker.CancellationPending;
            }
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
                InternalLog((string)e.UserState);
            else if (e.UserState is Exception)
                lstErrors.Items.Add(new ExceptionItem((Exception)e.UserState));
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            finished = true;
            btnAceptar.Enabled = true;
            btnAceptar.Text = "Accept";
            btnAceptar.Image = null;
            this.AcceptButton = btnAceptar;
            if (failed)
            {
                //try { pic.Image = new Bitmap(Application.StartupPath + Path.DirectorySeparatorChar + "dialog-error.png"); }
                try
                {
                    string path = Path.Combine(Application.StartupPath, "dialog-error.png");
                    if (File.Exists(path))
                        pic.Image = new Bitmap(path);
                }
                catch { }

                if (exceptionFail != null)
                {
                    UI.log("ERROR: " + exceptionFail.Message, 1);
                    if (askConfirmations)
                        new ExceptionMessageBox(exceptionFail).ShowDialog(this);
                }
                else
                    UI.log("Failed", 1);
            }
            else
            {
                //try { pic.Image = new Bitmap(Application.StartupPath + Path.DirectorySeparatorChar + "dialog-information.png"); }
                try
                {
                    string path = Path.Combine(Application.StartupPath, "dialog-information.png");
                    if (File.Exists(path))
                        pic.Image = new Bitmap(path);
                }
                catch { }
            }
            if (exitAfterEnd)
                Close();
        }

        #region Error list handling

        /// <summary>
        /// Show a dialog with the details of the selected exceptions.
        /// </summary>
        private void ShowErrorDetails()
        {
            foreach( ExceptionItem ex in lstErrors.SelectedItems ) 
                new ExceptionMessageBox(ex.exception).ShowDialog(this);
        }

        private void btnErrorDetails_Click(object sender, EventArgs e)
        {
            ShowErrorDetails();
        }

        private void lstErrors_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowErrorDetails();
        }

        #endregion

        

    }
}
