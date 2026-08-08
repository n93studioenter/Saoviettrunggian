namespace SaovietTax
{
    partial class frmSetting
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
            this.chkSuggest = new DevExpress.XtraEditors.CheckEdit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSuggest.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // chkSuggest
            // 
            this.chkSuggest.Location = new System.Drawing.Point(12, 12);
            this.chkSuggest.Name = "chkSuggest";
            this.chkSuggest.Properties.Caption = "Hiện gợi ý sản phẩm";
            this.chkSuggest.Size = new System.Drawing.Size(172, 20);
            this.chkSuggest.TabIndex = 1;
            this.chkSuggest.CheckedChanged += new System.EventHandler(this.chkSuggest_CheckedChanged);
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 185);
            this.Controls.Add(this.chkSuggest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thiết lập";
            this.Load += new System.EventHandler(this.frmSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chkSuggest.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.CheckEdit chkSuggest;
    }
}