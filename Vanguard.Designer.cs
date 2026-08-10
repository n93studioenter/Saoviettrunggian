namespace SaovietTax
{
    partial class Vanguard
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
            this.components = new System.ComponentModel.Container();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.warningDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colThang = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHoadonthieu = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colImportloi = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHangam = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHethongTK = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colHoadonthua = new DevExpress.XtraGrid.Columns.GridColumn();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningDataBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.DataSource = this.warningDataBindingSource;
            this.gridControl1.Location = new System.Drawing.Point(-1, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(956, 282);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // warningDataBindingSource
            // 
            this.warningDataBindingSource.DataSource = typeof(SaovietTax.Vanguard.WarningData);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colThang,
            this.colHoadonthieu,
            this.colImportloi,
            this.colHangam,
            this.colHethongTK,
            this.colHoadonthua});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // colThang
            // 
            this.colThang.Caption = "Tháng";
            this.colThang.FieldName = "Thang";
            this.colThang.MinWidth = 25;
            this.colThang.Name = "colThang";
            this.colThang.Visible = true;
            this.colThang.VisibleIndex = 0;
            this.colThang.Width = 59;
            // 
            // colHoadonthieu
            // 
            this.colHoadonthieu.Caption = "Hoá đơn chưa nhập";
            this.colHoadonthieu.FieldName = "Hoadonthieu";
            this.colHoadonthieu.MinWidth = 25;
            this.colHoadonthieu.Name = "colHoadonthieu";
            this.colHoadonthieu.Visible = true;
            this.colHoadonthieu.VisibleIndex = 1;
            this.colHoadonthieu.Width = 254;
            // 
            // colImportloi
            // 
            this.colImportloi.Caption = "Import lỗi";
            this.colImportloi.FieldName = "Importloi";
            this.colImportloi.MinWidth = 25;
            this.colImportloi.Name = "colImportloi";
            this.colImportloi.Visible = true;
            this.colImportloi.VisibleIndex = 2;
            this.colImportloi.Width = 170;
            // 
            // colHangam
            // 
            this.colHangam.Caption = "Hàng âm";
            this.colHangam.FieldName = "Hangam";
            this.colHangam.MinWidth = 25;
            this.colHangam.Name = "colHangam";
            this.colHangam.Visible = true;
            this.colHangam.VisibleIndex = 3;
            this.colHangam.Width = 120;
            // 
            // colHethongTK
            // 
            this.colHethongTK.Caption = "Hệ thống tài khoản chưa cân";
            this.colHethongTK.FieldName = "HethongTK";
            this.colHethongTK.MinWidth = 25;
            this.colHethongTK.Name = "colHethongTK";
            this.colHethongTK.Visible = true;
            this.colHethongTK.VisibleIndex = 4;
            this.colHethongTK.Width = 210;
            // 
            // colHoadonthua
            // 
            this.colHoadonthua.Caption = "Hoá đơn thừa";
            this.colHoadonthua.FieldName = "HoaDonThua";
            this.colHoadonthua.MinWidth = 25;
            this.colHoadonthua.Name = "colHoadonthua";
            this.colHoadonthua.Visible = true;
            this.colHoadonthua.VisibleIndex = 5;
            this.colHoadonthua.Width = 120;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.ImageOptions.SvgImage = global::SaovietTax.Properties.Resources.clearheaderandfooter1;
            this.simpleButton1.Location = new System.Drawing.Point(910, 246);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(45, 38);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // Vanguard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(958, 285);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.gridControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Vanguard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Vanguard";
            this.Load += new System.EventHandler(this.Vanguard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningDataBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.BindingSource warningDataBindingSource;
        private DevExpress.XtraGrid.Columns.GridColumn colHoadonthieu;
        private DevExpress.XtraGrid.Columns.GridColumn colImportloi;
        private DevExpress.XtraGrid.Columns.GridColumn colHangam;
        private DevExpress.XtraGrid.Columns.GridColumn colHethongTK;
        private DevExpress.XtraGrid.Columns.GridColumn colThang;
        private DevExpress.XtraGrid.Columns.GridColumn colHoadonthua;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
    }
}