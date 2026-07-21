namespace SaovietTax
{
    partial class FrmAutoTaiSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAutoTaiSetting));
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.txtTime3 = new DevExpress.XtraEditors.TextEdit();
            this.txtTime2 = new DevExpress.XtraEditors.TextEdit();
            this.txtTime1 = new DevExpress.XtraEditors.TextEdit();
            this.txtSolantai = new DevExpress.XtraEditors.TextEdit();
            this.svgImageBox1 = new DevExpress.XtraEditors.SvgImageBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.chkTime3 = new DevExpress.XtraEditors.CheckEdit();
            this.chkTime2 = new DevExpress.XtraEditors.CheckEdit();
            this.chkTime1 = new DevExpress.XtraEditors.CheckEdit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTime3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTime2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTime1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSolantai.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.svgImageBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkTime3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkTime2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkTime1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Appearance.BackColor = System.Drawing.Color.White;
            this.panelControl1.Appearance.Options.UseBackColor = true;
            this.panelControl1.Controls.Add(this.txtTime3);
            this.panelControl1.Controls.Add(this.txtTime2);
            this.panelControl1.Controls.Add(this.txtTime1);
            this.panelControl1.Controls.Add(this.txtSolantai);
            this.panelControl1.Controls.Add(this.svgImageBox1);
            this.panelControl1.Controls.Add(this.radioButton1);
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Controls.Add(this.chkTime3);
            this.panelControl1.Controls.Add(this.chkTime2);
            this.panelControl1.Controls.Add(this.chkTime1);
            this.panelControl1.Location = new System.Drawing.Point(12, 12);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(289, 233);
            this.panelControl1.TabIndex = 0;
            // 
            // txtTime3
            // 
            this.txtTime3.Enabled = false;
            this.txtTime3.Location = new System.Drawing.Point(146, 159);
            this.txtTime3.Name = "txtTime3";
            this.txtTime3.Size = new System.Drawing.Size(125, 23);
            this.txtTime3.TabIndex = 34;
            this.txtTime3.EditValueChanged += new System.EventHandler(this.txtTime3_EditValueChanged);
            this.txtTime3.Validated += new System.EventHandler(this.txtTime3_Validated);
            // 
            // txtTime2
            // 
            this.txtTime2.Enabled = false;
            this.txtTime2.Location = new System.Drawing.Point(146, 120);
            this.txtTime2.Name = "txtTime2";
            this.txtTime2.Size = new System.Drawing.Size(125, 23);
            this.txtTime2.TabIndex = 33;
            this.txtTime2.EditValueChanged += new System.EventHandler(this.txtTime2_EditValueChanged);
            this.txtTime2.Validated += new System.EventHandler(this.txtTime2_Validated);
            // 
            // txtTime1
            // 
            this.txtTime1.Enabled = false;
            this.txtTime1.Location = new System.Drawing.Point(146, 80);
            this.txtTime1.Name = "txtTime1";
            this.txtTime1.Size = new System.Drawing.Size(125, 23);
            this.txtTime1.TabIndex = 32;
            this.txtTime1.EditValueChanged += new System.EventHandler(this.txtTime1_EditValueChanged);
            this.txtTime1.Validated += new System.EventHandler(this.txtTime1_Validated);
            // 
            // txtSolantai
            // 
            this.txtSolantai.Location = new System.Drawing.Point(146, 197);
            this.txtSolantai.Name = "txtSolantai";
            this.txtSolantai.Size = new System.Drawing.Size(125, 23);
            this.txtSolantai.TabIndex = 31;
            this.txtSolantai.EditValueChanged += new System.EventHandler(this.txtSolantai_EditValueChanged);
            // 
            // svgImageBox1
            // 
            this.svgImageBox1.Location = new System.Drawing.Point(10, 11);
            this.svgImageBox1.Name = "svgImageBox1";
            this.svgImageBox1.Size = new System.Drawing.Size(55, 48);
            this.svgImageBox1.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("svgImageBox1.SvgImage")));
            this.svgImageBox1.TabIndex = 27;
            this.svgImageBox1.Text = "svgImageBox1";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(118, 26);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(153, 20);
            this.radioButton1.TabIndex = 26;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Tải khi khởi động máy";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            this.radioButton1.Click += new System.EventHandler(this.radioButton1_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(63, 204);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(54, 16);
            this.labelControl1.TabIndex = 8;
            this.labelControl1.Text = "Số lần tải";
            // 
            // chkTime3
            // 
            this.chkTime3.Location = new System.Drawing.Point(10, 162);
            this.chkTime3.Name = "chkTime3";
            this.chkTime3.Properties.Caption = "Mốc thời gian 3";
            this.chkTime3.Size = new System.Drawing.Size(130, 20);
            this.chkTime3.TabIndex = 7;
            this.chkTime3.CheckedChanged += new System.EventHandler(this.chkTime3_CheckedChanged);
            // 
            // chkTime2
            // 
            this.chkTime2.Location = new System.Drawing.Point(10, 121);
            this.chkTime2.Name = "chkTime2";
            this.chkTime2.Properties.Caption = "Mốc thời gian 2";
            this.chkTime2.Size = new System.Drawing.Size(130, 20);
            this.chkTime2.TabIndex = 5;
            this.chkTime2.CheckedChanged += new System.EventHandler(this.chkTime2_CheckedChanged);
            // 
            // chkTime1
            // 
            this.chkTime1.Location = new System.Drawing.Point(10, 83);
            this.chkTime1.Name = "chkTime1";
            this.chkTime1.Properties.Caption = "Mốc thời gian 1";
            this.chkTime1.Size = new System.Drawing.Size(130, 20);
            this.chkTime1.TabIndex = 3;
            this.chkTime1.CheckedChanged += new System.EventHandler(this.chkTime1_CheckedChanged);
            // 
            // FrmAutoTaiSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(315, 254);
            this.Controls.Add(this.panelControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmAutoTaiSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thiết lập tải tự động";
            this.Load += new System.EventHandler(this.FrmAutoTaiSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTime3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTime2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTime1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSolantai.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.svgImageBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkTime3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkTime2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkTime1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.CheckEdit chkTime3;
        private DevExpress.XtraEditors.CheckEdit chkTime2;
        private DevExpress.XtraEditors.CheckEdit chkTime1;
        private DevExpress.XtraEditors.SvgImageBox svgImageBox1;
        private System.Windows.Forms.RadioButton radioButton1;
        private DevExpress.XtraEditors.TextEdit txtSolantai;
        private DevExpress.XtraEditors.TextEdit txtTime3;
        private DevExpress.XtraEditors.TextEdit txtTime2;
        private DevExpress.XtraEditors.TextEdit txtTime1;
    }
}