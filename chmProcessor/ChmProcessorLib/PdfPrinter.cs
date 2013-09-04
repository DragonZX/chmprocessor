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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ChmProcessorLib
{
    /// <summary>
    /// Tool to print a file to a PDF file.
    /// PdfCreator must to be installed, ONLY VERSION 1.2 IS SUPPORTED. Other versions have not been tested.
    /// Be sure to take exceptions if its not installed or to be in use.
    /// </summary>
    public class PdfPrinter
    {
        /// <summary>
        /// Currently supported version.
        /// </summary>
        public const string SUPPORTEDVERSION = "1.2";

        /// <summary>
        /// Maximum time for printing, in seconds
        /// </summary>
        private const int MAXPRINTTIME = 180;    // 3 min.

        /// <summary>
        /// Timer to check print hang
        /// </summary>
        private Timer PrintTimeoutTimer;

        /// <summary>
        /// This is set to true when the printing ends
        /// </summary>
        private bool PrintingFinished;

        private PDFCreator.clsPDFCreatorError pErr;

        private PDFCreator.clsPDFCreator _PDFCreator;

        public PdfPrinter()
        {
            PrintTimeoutTimer = new System.Windows.Forms.Timer();
            PrintTimeoutTimer.Tick += new System.EventHandler(this.PrintTimeoutTimer_Tick);
        }

        public void ConvertToPdf(string filePath, string pdfDestinationPath)
        {
            try
            {
                PrintingFinished = false;

                string parameters;

                pErr = new PDFCreator.clsPDFCreatorError();

                _PDFCreator = new PDFCreator.clsPDFCreator();
                _PDFCreator.eError += new PDFCreator.__clsPDFCreator_eErrorEventHandler(_PDFCreator_eError);
                _PDFCreator.eReady += new PDFCreator.__clsPDFCreator_eReadyEventHandler(_PDFCreator_eReady);

                parameters = "/NoProcessingAtStartup";

                if (!_PDFCreator.cStart(parameters, false))
                    throw new Exception("Error starting PdfCreator: [" + pErr.Number + "]: " + pErr.Description);

                if (!_PDFCreator.cIsPrintable(filePath))
                    throw new Exception("PdfCreator says that file '" + filePath + "' is not printable!");

                PDFCreator.clsPDFCreatorOptions opt;
                opt = _PDFCreator.cOptions;

                // Store previous option values:
                int useautosave = opt.UseAutosave;
                int useautosaveDir = opt.UseAutosaveDirectory;
                string autosaveDir = opt.AutosaveDirectory;
                int autosaveformat = opt.AutosaveFormat;
                string autosavefilename = opt.AutosaveFilename;

                // Set new options to save to the desired file:
                opt.UseAutosave = 1;
                opt.UseAutosaveDirectory = 1;
                opt.AutosaveDirectory = Path.GetDirectoryName(pdfDestinationPath);
                opt.AutosaveFormat = 0;
                opt.AutosaveFilename = Path.GetFileNameWithoutExtension(pdfDestinationPath);

                // Print:
                _PDFCreator.cOptions = opt;
                _PDFCreator.cClearCache();
                string DefaultPrinter = _PDFCreator.cDefaultPrinter;
                _PDFCreator.cDefaultPrinter = "PDFCreator";
                _PDFCreator.cPrintFile(filePath);
                _PDFCreator.cPrinterStop = false;

                // Wait until print ends
                PrintTimeoutTimer.Interval = MAXPRINTTIME * 1000;
                PrintTimeoutTimer.Enabled = true;
                while (!PrintingFinished && PrintTimeoutTimer.Enabled)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100); // Wait 100 miliseconds.
                }
                PrintTimeoutTimer.Enabled = false;

                // Restore previous options values:
                opt.UseAutosave = useautosave;
                opt.UseAutosaveDirectory = useautosaveDir;
                opt.AutosaveDirectory = autosaveDir;
                opt.AutosaveFormat = autosaveformat;
                opt.AutosaveFilename = autosavefilename;
                _PDFCreator.cOptions = opt;

                if (!PrintingFinished)
                    throw new Exception("PdfCreator printing timeout");

                _PDFCreator.cPrinterStop = true;
                _PDFCreator.cDefaultPrinter = DefaultPrinter;
            }
            finally
            {
                try
                {
                    // Close the PDF com object:
                    if (_PDFCreator != null)
                    {
                        _PDFCreator.cClose();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(_PDFCreator);
                        //_PDFCreator = null;
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pErr);
                        pErr = null;
                        GC.Collect();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Called when the timeout timer has exceeded its time
        /// </summary>
        private void PrintTimeoutTimer_Tick(object sender, System.EventArgs e)
        {
            PrintTimeoutTimer.Enabled = false;
        }

        /// <summary>
        /// Called when the printing ends
        /// </summary>
        private void _PDFCreator_eReady()
        {
            _PDFCreator.cPrinterStop = true;
            PrintingFinished = true;
        }

        /// <summary>
        /// Called when a PDFCreater error happens
        /// </summary>
        private void _PDFCreator_eError()
        {
            pErr = _PDFCreator.cError;
            PrintingFinished = true;
        }

    }
}
