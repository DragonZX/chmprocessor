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

namespace ProcesadorHtml
{
	/// <summary>
	/// Dialog to show the CHM generation status progress
	/// </summary>
	public class GenerationDialog : System.Windows.Forms.Form , UserInterface
	{
        private System.Windows.Forms.Button btnAceptar;
		/// <summary>
		/// Variable del diseñador requerida.
		/// </summary>
		private System.ComponentModel.Container components = null;

        /// <summary>
        /// CHM project to generate
        /// </summary>
        private ChmProject project;

        private DocumentProcessor procesador;

        private System.Windows.Forms.PictureBox pic;

        bool logToConsole;
        bool exitAfterEnd;
        private BackgroundWorker bgWorker;

        /// <summary>
        /// Generation process finished?
        /// </summary>
        bool finished;

        /// <summary>
        /// Generation process failed?
        /// </summary>
        bool failed;

        /// <summary>
        /// If generation process failed, this is the exception generated.
        /// </summary>
        Exception exceptionFail;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TextBox txtLog;
        private TabPage tabPage2;
        private ListBox lstErrors;
        private Button btnErrorDetails;

        bool askConfirmations;

        // This delegate enables asynchronous calls for adding the text log.
        delegate void SetTextCallback(string text);
        delegate void SetEnabledCallback();

        public void ThreadProc() 
        {
            try
            {
                DateTime startTime = DateTime.Now;

                procesador.GenerateHelp();
                if (CancellRequested())
                {
                    log("PROCESS CANCELLED");
                    return;
                }

                log("DONE!");

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                log("Total time: " + duration.ToString());
            }
            catch (Exception ex)
            {
                failed = true;
                exceptionFail = ex;
            }
        }

        //public GenerationDialog(DocumentProcessor procesador, string archivo, bool compilar, string archivoAyuda, bool abrirProyecto, string commandLine, bool logToConsole, bool exitAfterEnd, bool askConfirmations )
        public GenerationDialog( ChmProject project ,  bool logToConsole, bool exitAfterEnd, bool askConfirmations)
		{
			//
			// Necesario para admitir el Diseñador de Windows Forms
			//
			InitializeComponent();

            this.project = project;

            this.procesador = new DocumentProcessor(project);

            this.exitAfterEnd = exitAfterEnd;
            this.logToConsole = logToConsole;
            this.askConfirmations = askConfirmations;

            procesador.ui = this;
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
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lstErrors = new System.Windows.Forms.ListBox();
            this.btnErrorDetails = new System.Windows.Forms.Button();
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
            // lstErrors
            // 
            this.lstErrors.FormattingEnabled = true;
            this.lstErrors.Location = new System.Drawing.Point(6, 6);
            this.lstErrors.Name = "lstErrors";
            this.lstErrors.Size = new System.Drawing.Size(541, 290);
            this.lstErrors.TabIndex = 0;
            // 
            // btnErrorDetails
            // 
            this.btnErrorDetails.Location = new System.Drawing.Point(6, 308);
            this.btnErrorDetails.Name = "btnErrorDetails";
            this.btnErrorDetails.Size = new System.Drawing.Size(127, 23);
            this.btnErrorDetails.TabIndex = 1;
            this.btnErrorDetails.Text = "Show error details";
            this.btnErrorDetails.UseVisualStyleBackColor = true;
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

        #region Miembros de Logger

        public void log(string text)
        {
            bgWorker.ReportProgress(0, text);
        }

        public bool CancellRequested()
        {
            return bgWorker.CancellationPending;
        }

        /// <summary>
        /// Called by the generation process to add an exception to the log.
        /// </summary>
        /// <param name="text">Exception to log</param>
        public void log(Exception exception)
        {
            bgWorker.ReportProgress(0, exception);
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
                    //t.Abort();
                    bgWorker.CancelAsync();
                }
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ThreadProc();
        }

        private void WriteLog(string text)
        {
            txtLog.AppendText(text + "\r\n");
            if (logToConsole)
                Console.WriteLine(text);
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
            {
                string text = (string)e.UserState;
                WriteLog(text);
            }
            else if (e.UserState is Exception)
            {
            }
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
                try { pic.Image = new Bitmap(Application.StartupPath + Path.DirectorySeparatorChar + "dialog-error.png"); }
                catch { }
                if (exceptionFail != null)
                {
                    WriteLog("ERROR: " + exceptionFail.Message);
                    if (askConfirmations)
                        new ExceptionMessageBox(exceptionFail).ShowDialog(this);
                }
                else
                    WriteLog("Failed");
            }
            else
            {
                try { pic.Image = new Bitmap(Application.StartupPath + Path.DirectorySeparatorChar + "dialog-information.png"); }
                catch { }
            }
            if (exitAfterEnd)
                Close();
        }
    }
}
