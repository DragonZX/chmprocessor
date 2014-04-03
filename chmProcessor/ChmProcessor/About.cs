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
using System.IO;
using System.Diagnostics;

namespace ChmProcessor
{
	/// <summary>
	/// Stupid About dialog.
	/// </summary>
	public class About : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLicencia;
        private System.Windows.Forms.Label labLicencia;
        private System.Windows.Forms.LinkLabel lnkWebsite;
        private Label label3;
        private LinkLabel lnkTidy;
        private LinkLabel lnkSqlite;
        private LinkLabel lnkTango;
        private PictureBox pictureBox1;
        private LinkLabel LnkJQuery;
        private LinkLabel LnkLayout;
        private LinkLabel LnkJsTree;
		/// <summary>
		/// Variable del diseñador requerida.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public About()
		{
			//
			// Necesario para admitir el Diseñador de Windows Forms
			//
			InitializeComponent();

            try 
            {
                StreamReader reader = new StreamReader( Application.StartupPath + Path.DirectorySeparatorChar + "license.txt");
                txtLicencia.Text = reader.ReadToEnd().Replace( "\n" , "\r\n" );
                reader.Close();
            }
            catch 
            {
                labLicencia.Visible = false;
                txtLicencia.Visible = false;
            }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labLicencia = new System.Windows.Forms.Label();
            this.lnkWebsite = new System.Windows.Forms.LinkLabel();
            this.txtLicencia = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lnkTidy = new System.Windows.Forms.LinkLabel();
            this.lnkSqlite = new System.Windows.Forms.LinkLabel();
            this.lnkTango = new System.Windows.Forms.LinkLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.LnkJQuery = new System.Windows.Forms.LinkLabel();
            this.LnkLayout = new System.Windows.Forms.LinkLabel();
            this.LnkJsTree = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.Location = new System.Drawing.Point(225, 400);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 32);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(86, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(441, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "chmProcessor - A Html / Word converter to Compiled HTML Help v1.7.3";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(86, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(441, 34);
            this.label2.TabIndex = 2;
            this.label2.Text = "(C) 2008 Toni Bennasar Obrador, Mike Hudgell, Paolo Moretti, Jozsef Bekes, Luc Va" +
                "n Asch";
            // 
            // labLicencia
            // 
            this.labLicencia.Location = new System.Drawing.Point(11, 81);
            this.labLicencia.Name = "labLicencia";
            this.labLicencia.Size = new System.Drawing.Size(100, 19);
            this.labLicencia.TabIndex = 3;
            this.labLicencia.Text = "License:";
            // 
            // lnkWebsite
            // 
            this.lnkWebsite.Location = new System.Drawing.Point(86, 62);
            this.lnkWebsite.Name = "lnkWebsite";
            this.lnkWebsite.Size = new System.Drawing.Size(200, 16);
            this.lnkWebsite.TabIndex = 4;
            this.lnkWebsite.TabStop = true;
            this.lnkWebsite.Text = "http://chmprocessor.sourceforge.net/";
            this.lnkWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWebsite_LinkClicked);
            // 
            // txtLicencia
            // 
            this.txtLicencia.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLicencia.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLicencia.Location = new System.Drawing.Point(11, 103);
            this.txtLicencia.Multiline = true;
            this.txtLicencia.Name = "txtLicencia";
            this.txtLicencia.ReadOnly = true;
            this.txtLicencia.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLicencia.Size = new System.Drawing.Size(516, 246);
            this.txtLicencia.TabIndex = 5;
            this.txtLicencia.WordWrap = false;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 362);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "This software uses";
            // 
            // lnkTidy
            // 
            this.lnkTidy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkTidy.AutoSize = true;
            this.lnkTidy.Location = new System.Drawing.Point(112, 362);
            this.lnkTidy.Name = "lnkTidy";
            this.lnkTidy.Size = new System.Drawing.Size(27, 13);
            this.lnkTidy.TabIndex = 7;
            this.lnkTidy.TabStop = true;
            this.lnkTidy.Text = "Tidy";
            this.lnkTidy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkTidy_LinkClicked);
            // 
            // lnkSqlite
            // 
            this.lnkSqlite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkSqlite.AutoSize = true;
            this.lnkSqlite.Location = new System.Drawing.Point(145, 362);
            this.lnkSqlite.Name = "lnkSqlite";
            this.lnkSqlite.Size = new System.Drawing.Size(33, 13);
            this.lnkSqlite.TabIndex = 8;
            this.lnkSqlite.TabStop = true;
            this.lnkSqlite.Text = "Sqlite";
            this.lnkSqlite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSqlite_LinkClicked);
            // 
            // lnkTango
            // 
            this.lnkTango.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkTango.AutoSize = true;
            this.lnkTango.Location = new System.Drawing.Point(184, 362);
            this.lnkTango.Name = "lnkTango";
            this.lnkTango.Size = new System.Drawing.Size(67, 13);
            this.lnkTango.TabIndex = 9;
            this.lnkTango.TabStop = true;
            this.lnkTango.Text = "Tango Icons";
            this.lnkTango.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkTango_LinkClicked);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(11, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(69, 66);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // LnkJQuery
            // 
            this.LnkJQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LnkJQuery.AutoSize = true;
            this.LnkJQuery.Location = new System.Drawing.Point(258, 361);
            this.LnkJQuery.Name = "LnkJQuery";
            this.LnkJQuery.Size = new System.Drawing.Size(37, 13);
            this.LnkJQuery.TabIndex = 11;
            this.LnkJQuery.TabStop = true;
            this.LnkJQuery.Text = "jQuery";
            this.LnkJQuery.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkJQuery_LinkClicked);
            // 
            // LnkLayout
            // 
            this.LnkLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LnkLayout.AutoSize = true;
            this.LnkLayout.Location = new System.Drawing.Point(301, 362);
            this.LnkLayout.Name = "LnkLayout";
            this.LnkLayout.Size = new System.Drawing.Size(121, 13);
            this.LnkLayout.TabIndex = 12;
            this.LnkLayout.TabStop = true;
            this.LnkLayout.Text = "jQuery UI Layout Plug-in";
            this.LnkLayout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkLayout_LinkClicked);
            // 
            // LnkJsTree
            // 
            this.LnkJsTree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LnkJsTree.AutoSize = true;
            this.LnkJsTree.Location = new System.Drawing.Point(428, 362);
            this.LnkJsTree.Name = "LnkJsTree";
            this.LnkJsTree.Size = new System.Drawing.Size(36, 13);
            this.LnkJsTree.TabIndex = 13;
            this.LnkJsTree.TabStop = true;
            this.LnkJsTree.Text = "jsTree";
            this.LnkJsTree.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkJsTree_LinkClicked);
            // 
            // About
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(539, 444);
            this.Controls.Add(this.LnkJsTree);
            this.Controls.Add(this.LnkLayout);
            this.Controls.Add(this.LnkJQuery);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lnkTango);
            this.Controls.Add(this.lnkSqlite);
            this.Controls.Add(this.lnkTidy);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtLicencia);
            this.Controls.Add(this.lnkWebsite);
            this.Controls.Add(this.labLicencia);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOk);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "About";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About chmProcessor...";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void lnkWebsite_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            Process.Start( lnkWebsite.Text );
        }

        private void lnkTidy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://tidy.sourceforge.net");
        }

        private void lnkSqlite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://sqlite.phxsoftware.com");
        }

        private void lnkTango_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://tango.freedesktop.org/Tango_Desktop_Project");
        }

        private void LnkJQuery_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://jquery.com");
        }

        private void LnkLayout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://layout.jquery-dev.net/index.cfm");
        }

        private void LnkJsTree_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.jstree.com/");
        }

	}
}
