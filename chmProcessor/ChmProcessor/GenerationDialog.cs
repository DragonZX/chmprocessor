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
	public class GenerationDialog : System.Windows.Forms.Form , DocumentProcessor.UserInterface
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

        //private bool compilar;
        //private string archivoAyuda;

        //private string archivo;
        private System.Windows.Forms.PictureBox pic;
        private System.Windows.Forms.TextBox txtLog;
        //private bool abrirProyecto;

        /*
        // TODO: Remove this member and user AppSettings.CompilerPath instead
        /// <summary>
        /// Path to the microsoft help workshop compiler.
        /// </summary>
        private string compilerPath;
        */

        /*
        /// <summary>
        /// Command line to execute after generation ends.
        /// </summary>
        private string cmdLine;
        */

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

        bool askConfirmations;

        // This delegate enables asynchronous calls for adding the text log.
        delegate void SetTextCallback(string text);
        delegate void SetEnabledCallback();

        public void ThreadProc() 
        {
            try
            {
                DateTime startTime = DateTime.Now;

                string proyectoAyuda = procesador.Generate();
                if (CancellRequested())
                {
                    log("PROCESS CANCELLED");
                    return;
                }
                //if (compilar)
                if ( project.Compile )
                {
                    // Due to some strange bug, if we have as current drive a network drive, the generated
                    // help dont show the images... So, change it to the system drive:
                    string cwd = Directory.GetCurrentDirectory();
                    //string tempDirectory = Path.GetDirectoryName(procesador.PartsDirectory);
                    string tempDirectory = Path.GetDirectoryName(procesador.Configuration.HelpProjectDirectory);
                    Directory.SetCurrentDirectory(tempDirectory);
                    procesador.Compile(project.ArchivoAyuda, AppSettings.CompilerPath);
                    Directory.SetCurrentDirectory(cwd);
                }
                else if (project.AbrirProyecto)
                {
                    try
                    {
                        // Abrir el proyecto de la ayuda
                        Process proceso = new Process();
                        proceso.StartInfo.FileName = proyectoAyuda;
                        proceso.Start();
                    }
                    catch
                    {
                        log("The project " + proyectoAyuda + " cannot be opened" +
                            ". Have you installed the Microsoft Help Workshop ?");
                    }
                }
                if (CancellRequested())
                {
                    log("PROCESS CANCELLED");
                    return;
                }
                if ( procesador.Configuration.GeneratePdf )
                    BuildPdf();

                if (procesador.Configuration.GenerateXps)
                    BuildXps();

                if (CancellRequested())
                {
                    log("PROCESS CANCELLED");
                    return;
                }

                //if (cmdLine != null && !cmdLine.Trim().Equals(""))
                if (project.CommandLine != null && !project.CommandLine.Trim().Equals(""))
                {
                    // Execute the command line:
                    log("Executing '" + project.CommandLine.Trim() + "'");
                    string strCmdLine = "/C " + project.CommandLine.Trim();
                    ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo("CMD.exe", strCmdLine);
                    si.CreateNoWindow = false;
                    si.UseShellExecute = false;
                    si.RedirectStandardOutput = true;
                    si.RedirectStandardError = true;
                    Process p = new Process();
                    p.StartInfo = si;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    log(output);
                    log(error);
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

        /// <summary>
        /// Generate a XPS file for the document.
        /// </summary>
        private void BuildXps()
        {
            log("Generating XPS file");
            try
            {
                MSWord word = new MSWord();
                //word.SaveWordToXps(archivo, procesador.Configuration.XpsPath);
                word.SaveWordToXps(project.ArchivoOrigen, procesador.Configuration.XpsPath);
            }
            catch (Exception ex)
            {
                log(ex.Message);
                log("Something wrong happened with the XPS generation. Remember you must to have Microsoft Office 2007 and the" +
                        "pdf/xps generation add-in (http://www.microsoft.com/downloads/details.aspx?FamilyID=4D951911-3E7E-4AE6-B059-A2E79ED87041&displaylang=en)");
            }
        }

        private void BuildPdf()
        {
            try
            {
                log("Generating PDF file");
                if (procesador.Configuration.PdfGeneration == ChmProject.PdfGenerationWay.OfficeAddin)
                {
                    MSWord word = new MSWord();
                    //word.SaveWordToPdf(archivo, procesador.Configuration.PdfPath);
                    word.SaveWordToPdf(project.ArchivoOrigen, procesador.Configuration.PdfPath);
                }
                else
                {
                    PdfPrinter pdfPrinter = new PdfPrinter();
                    //pdfPrinter.ConvertToPdf(archivo, procesador.Configuration.PdfPath);
                    pdfPrinter.ConvertToPdf(project.ArchivoOrigen, procesador.Configuration.PdfPath);
                }
              }
            catch (Exception ex)
            {
                log(ex.Message);
                if (procesador.Configuration.PdfGeneration == ChmProject.PdfGenerationWay.OfficeAddin)
                    log("Something wrong happened with the PDF generation. Remember you must to have Microsoft Office 2007 and the" +
                        "pdf/xps generation add-in (http://www.microsoft.com/downloads/details.aspx?FamilyID=4D951911-3E7E-4AE6-B059-A2E79ED87041&displaylang=en)");
                else
                    log("Something wrong happened with the PDF generation. Remember you must to have PdfCreator (version 0.9.3 tested only) installed into your computer to " +
                        "generate a PDF file. You can download it from http://www.pdfforge.org/products/pdfcreator/download");
                throw ex;
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

            //this.procesador = procesador;
            this.procesador = new DocumentProcessor(project);

            //this.archivo = archivo;
            //this.compilar = compilar;
            //this.archivoAyuda = archivoAyuda;
            //this.abrirProyecto = abrirProyecto;
            //this.compilerPath = AppSettings.CompilerPath;
            //this.cmdLine = commandLine;
            this.exitAfterEnd = exitAfterEnd;
            this.logToConsole = logToConsole;
            this.askConfirmations = askConfirmations;

            procesador.ui = this;
            //t = new Thread(new ThreadStart(ThreadProc));
            //t.Start();
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
            this.txtLog = new System.Windows.Forms.TextBox();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pic)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAceptar
            // 
            this.btnAceptar.Image = ((System.Drawing.Image)(resources.GetObject("btnAceptar.Image")));
            this.btnAceptar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAceptar.Location = new System.Drawing.Point(178, 324);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(173, 40);
            this.btnAceptar.TabIndex = 0;
            this.btnAceptar.Text = "Cancel";
            this.btnAceptar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // pic
            // 
            this.pic.Location = new System.Drawing.Point(96, 324);
            this.pic.Name = "pic";
            this.pic.Size = new System.Drawing.Size(56, 40);
            this.pic.TabIndex = 3;
            this.pic.TabStop = false;
            // 
            // txtLog
            // 
            this.txtLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(12, 12);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(504, 296);
            this.txtLog.TabIndex = 4;
            // 
            // bgWorker
            // 
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            // 
            // GenerationDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(530, 383);
            this.ControlBox = false;
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.pic);
            this.Controls.Add(this.btnAceptar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "GenerationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "chmProcessor  - Generating help...";
            ((System.ComponentModel.ISupportInitialize)(this.pic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
            string text = (string)e.UserState;
            WriteLog(text);
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
                    if( askConfirmations ) 
                        MessageBox.Show(exceptionFail.Message + "\n" + exceptionFail.StackTrace);
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
