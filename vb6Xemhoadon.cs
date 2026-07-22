using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class vb6Xemhoadon : DevExpress.XtraEditors.XtraForm
    {
        public vb6Xemhoadon()
        {
            InitializeComponent();
        }

        private void vb6Xemhoadon_Load(object sender, EventArgs e)
        {
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
            string _content = File.ReadAllText(filePath);
            if(File.Exists(_content))
            {
                frmXemhoadonInvoicse frmXemhoadonInvoicse = new frmXemhoadonInvoicse();
                frmXemhoadonInvoicse.path= _content;
                frmXemhoadonInvoicse.ShowDialog();
            }
            else
            {
                _content = Path.ChangeExtension(_content, ".pdf");
                frmXemhoadonInvoicse frmXemhoadonInvoicse = new frmXemhoadonInvoicse();
                frmXemhoadonInvoicse.path = _content;
                frmXemhoadonInvoicse.ShowDialog();
            }
        }
    }
}