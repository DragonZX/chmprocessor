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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ChmProcessor
{
    /// <summary>
    /// Dialog to show to the user details about an exception.
    /// </summary>
    public partial class ExceptionMessageBox : Form
    {

        /// <summary>
        /// URL where the user can report bugs.
        /// If its not null, a link to this URL will be showed on the dialog.
        /// </summary>
        static public string UrlBugReport = null;

        /// <summary>
        /// Exception about we show details
        /// </summary>
        private Exception exception;

        /// <summary>
        /// Called from constructors.
        /// </summary>
        /// <param name="message">General information for the user.</param>
        /// <param name="exception">Exception about we show details</param>
        private void Initialize(string message, Exception exception)
        {
            InitializeComponent();

            this.exception = exception;

            if (UrlBugReport != null)
                lnkReportBug.Text = "'I think this is a bug'. Click here to go to the bug tracker.";
            else
                lnkReportBug.Visible = false;

            if (message == null)
                txtMessage.Text = exception.Message;
            else
                txtMessage.Text = message + "\r\n" + exception.Message;

            try
            {
                appendDetailText("Stack trace", exception.StackTrace);
                appendDetailText("Class", exception.GetType());
                appendDetailText("Data", exception.Data);
                appendDetailText("Help link", exception.HelpLink);
                appendDetailText("Source", exception.Source);
                appendDetailText("Target site", exception.TargetSite);
            }
            catch { }

            if (exception.InnerException != null)
                txtInner.Text = exception.InnerException.Message;
            else
            {
                lnkInner.Visible = false;
                txtInner.Visible = false;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exception">Exception about we show details</param>
        public ExceptionMessageBox(Exception exception)
        {
            Initialize(null, exception);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">General information for the user.</param>
        /// <param name="exception">Exception about we show details</param>
        public ExceptionMessageBox(string message, Exception exception)
        {
            Initialize(message, exception);
        }

        private void appendDetailText(string title, object data)
        {
            if (data == null)
                return;
            string text = data.ToString();
            if (text != null && text.Length > 0)
            {
                string txt = "";
                if (txtDetails.Text.Length > 0)
                    txt += "\n\n";
                txt += title + ":\n" + text;
                txtDetails.AppendText(txt.Replace("\n", "\r\n"));
            }
        }

        /// <summary>
        /// Report bug link clicked.
        /// </summary>
        private void lnkReportBug_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(UrlBugReport);
            }
            catch (Exception ex)
            {
                new ExceptionMessageBox(ex).ShowDialog(this);
            }
        }

        /// <summary>
        /// Inner exception link clicked.
        /// </summary>
        private void lnkInner_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new ExceptionMessageBox(exception.InnerException).ShowDialog(this);
        }

        /// <summary>
        /// Accept button clicked
        /// </summary>
        private void btnAccept_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
