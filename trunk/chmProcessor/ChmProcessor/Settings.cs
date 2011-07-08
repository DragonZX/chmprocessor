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
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using ChmProcessorLib;

namespace ChmProcessor
{
    /// <summary>
    /// Window with the application settings.
    /// </summary>
    public partial class Settings : Form
    {

        public Settings()
        {
            InitializeComponent();
            txtCompilerPath.Text = AppSettings.CompilerPath;
            chkTidyInput.Checked = AppSettings.UseTidyOverInput;
            chtTidyOutput.Checked = AppSettings.UseTidyOverOutput;
            txtJdk.Text = AppSettings.JdkHome;
            txtJavaHelpPath.Text = AppSettings.JavaHelpPath;
            chkSaveRelativePaths.Checked = AppSettings.SaveRelativePaths;
            chkReplaceBrokenLinks.Checked = AppSettings.ReplaceBrokenLinks;

            chkTidyInput.Visible = false;       // Its not working well. Some bug with the encodings... TODO
            chtTidyOutput.Visible = false;      // By now disabled, problems with encoding names... TODO
                
            chkUseAppLocale.Checked = AppSettings.UseAppLocale;
            txtAppLocalePath.Text = AppSettings.AppLocalePath;

            chkUseAppLocate_CheckedChanged(null, null);

        }

        private void btnSelFile_Click(object sender, EventArgs e)
        {
            dlgSelectCompiler.FileName = txtCompilerPath.Text;
            
            if (DialogResult.OK == dlgSelectCompiler.ShowDialog())
            {
                txtCompilerPath.Text = dlgSelectCompiler.FileName;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if ( !txtCompilerPath.Text.Equals("") && !File.Exists(txtCompilerPath.Text) )
            {
                MessageBox.Show("The selected compiler path does not exist");
                txtCompilerPath.Focus();
                return;
            }

            if (!txtJdk.Text.Equals("") )
            {
                if( !Directory.Exists(txtJdk.Text) ) {
                    MessageBox.Show("The selected JDK path does not exist");
                    txtJdk.Focus();
                    return;
                }
                if( !File.Exists(txtJdk.Text + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + 
                    "jar.exe") ) {
                        MessageBox.Show("The selected JDK path does not contains a bin/jar.exe file");
                    txtJdk.Focus();
                    return;
                }
                if (!File.Exists(txtJdk.Text + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + 
                    "java.exe") ) {
                    MessageBox.Show("The selected JDK path does not contains a bin/java.exe file");
                    txtJdk.Focus();
                    return;
                }
            }

            if (!txtJavaHelpPath.Text.Equals("") ) 
            {
                if (!Directory.Exists(txtJavaHelpPath.Text))
                {
                    MessageBox.Show("The selected Java Help path does not exist");
                    txtJavaHelpPath.Focus();
                    return;
                }

                char sp = Path.DirectorySeparatorChar;
                string viewerPath = txtJavaHelpPath.Text + sp + "demos" + sp + "bin" + sp + "hsviewer.jar";
                if (!File.Exists(viewerPath))
                {
                    MessageBox.Show("The selected Java Help path does not contains a demos/bin/hsviewer.jar file");
                    txtJavaHelpPath.Focus();
                    return;
                }
            }

            if (chkUseAppLocale.Checked && !File.Exists(txtAppLocalePath.Text))
            {
                MessageBox.Show("The selected AppLocate path does not exist");
                return;
            }

            AppSettings.UseAppLocale = chkUseAppLocale.Checked;
            AppSettings.AppLocalePath = txtAppLocalePath.Text;
            AppSettings.CompilerPath = txtCompilerPath.Text;
            AppSettings.UseTidyOverInput = chkTidyInput.Checked;
            AppSettings.UseTidyOverOutput = chtTidyOutput.Checked;
            AppSettings.JdkHome = txtJdk.Text;
            AppSettings.JavaHelpPath = txtJavaHelpPath.Text;
            AppSettings.SaveRelativePaths = chkSaveRelativePaths.Checked;
            AppSettings.ReplaceBrokenLinks = chkReplaceBrokenLinks.Checked;

            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://msdn2.microsoft.com/en-us/library/ms669985.aspx");
        }

        private void Settings_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            Help.ShowHelp(this.Parent, Application.StartupPath + Path.DirectorySeparatorChar + "chmProcessor.chm", HelpNavigator.KeywordIndex, "Settings");
        }

        private void btnSelJdk_Click(object sender, EventArgs e)
        {
            if (dlgSelJdk.ShowDialog() == DialogResult.OK)
                txtJdk.Text = dlgSelJdk.SelectedPath;
        }

        private void lnkJdk_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.oracle.com/technetwork/java/javase/downloads/index.html");
        }

        private void lnkJavaHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://javahelp.java.net/");
        }

        private void btnSelJavaHelp_Click(object sender, EventArgs e)
        {
            if (dlgSelJdk.ShowDialog() == DialogResult.OK)
                txtJavaHelpPath.Text = dlgSelJdk.SelectedPath;
        }

        private void chkUseAppLocate_CheckedChanged(object sender, EventArgs e)
        {
            txtAppLocalePath.Enabled = chkUseAppLocale.Checked;
            btnSelAppLocate.Enabled = chkUseAppLocale.Checked;
        }

        private void lnkAppLocate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/download/en/details.aspx?id=13209");
        }

        private void btnSelAppLocate_Click(object sender, EventArgs e)
        {
            dlgSelectAppLocale.FileName = txtAppLocalePath.Text;
            if (DialogResult.OK == dlgSelectAppLocale.ShowDialog())
                txtAppLocalePath.Text = dlgSelectAppLocale.FileName;
        }

    }
}