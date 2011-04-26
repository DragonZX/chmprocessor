namespace ChmProcessor
{
    partial class ExceptionMessageBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.btnAccept = new System.Windows.Forms.Button();
            this.lnkReportBug = new System.Windows.Forms.LinkLabel();
            this.txtInner = new System.Windows.Forms.TextBox();
            this.lnkInner = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Error message";
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.Location = new System.Drawing.Point(95, 9);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.Size = new System.Drawing.Size(595, 61);
            this.txtMessage.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Error details";
            // 
            // txtDetails
            // 
            this.txtDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDetails.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDetails.Location = new System.Drawing.Point(95, 76);
            this.txtDetails.MaxLength = 0;
            this.txtDetails.Multiline = true;
            this.txtDetails.Name = "txtDetails";
            this.txtDetails.ReadOnly = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDetails.Size = new System.Drawing.Size(595, 189);
            this.txtDetails.TabIndex = 3;
            this.txtDetails.WordWrap = false;
            // 
            // btnAccept
            // 
            this.btnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAccept.Location = new System.Drawing.Point(615, 297);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 23);
            this.btnAccept.TabIndex = 4;
            this.btnAccept.Text = "Accept";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // lnkReportBug
            // 
            this.lnkReportBug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkReportBug.AutoSize = true;
            this.lnkReportBug.Location = new System.Drawing.Point(92, 302);
            this.lnkReportBug.Name = "lnkReportBug";
            this.lnkReportBug.Size = new System.Drawing.Size(176, 13);
            this.lnkReportBug.TabIndex = 5;
            this.lnkReportBug.TabStop = true;
            this.lnkReportBug.Text = "This is a bug. Go to XXX to report it.";
            this.lnkReportBug.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkReportBug_LinkClicked);
            // 
            // txtInner
            // 
            this.txtInner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInner.Location = new System.Drawing.Point(95, 271);
            this.txtInner.Name = "txtInner";
            this.txtInner.ReadOnly = true;
            this.txtInner.Size = new System.Drawing.Size(595, 20);
            this.txtInner.TabIndex = 7;
            // 
            // lnkInner
            // 
            this.lnkInner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkInner.AutoSize = true;
            this.lnkInner.Location = new System.Drawing.Point(12, 274);
            this.lnkInner.Name = "lnkInner";
            this.lnkInner.Size = new System.Drawing.Size(44, 13);
            this.lnkInner.TabIndex = 8;
            this.lnkInner.TabStop = true;
            this.lnkInner.Text = "Reason";
            this.lnkInner.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkInner_LinkClicked);
            // 
            // ExceptionMessageBox
            // 
            this.AcceptButton = this.btnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnAccept;
            this.ClientSize = new System.Drawing.Size(702, 332);
            this.Controls.Add(this.lnkInner);
            this.Controls.Add(this.txtInner);
            this.Controls.Add(this.lnkReportBug);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.txtDetails);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.label1);
            this.MinimizeBox = false;
            this.Name = "ExceptionMessageBox";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Error";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.LinkLabel lnkReportBug;
        private System.Windows.Forms.TextBox txtInner;
        private System.Windows.Forms.LinkLabel lnkInner;
    }
}