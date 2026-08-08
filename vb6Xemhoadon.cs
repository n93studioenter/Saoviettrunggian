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
                frmXemhoadonInvoicse.invoicse = this;
                frmXemhoadonInvoicse.ShowDialog();
            }
            else
            {
                string directory = Path.GetDirectoryName(_content);
                string fileName = Path.GetFileName(_content);

                int index = fileName.IndexOf('_');
                if (index >= 0)
                {
                    fileName = fileName.Substring(index + 1);
                }

                _content = Path.Combine(directory, fileName);
                if (File.Exists(_content))
                {
                    frmXemhoadonInvoicse frmXemhoadonInvoicse = new frmXemhoadonInvoicse();
                    frmXemhoadonInvoicse.path = _content;
                    frmXemhoadonInvoicse.invoicse = this;
                    frmXemhoadonInvoicse.ShowDialog();
                }
                else
                {
                    _content = Path.ChangeExtension(_content, ".pdf");
                    if (File.Exists(_content))
                    {

                        frmXemhoadonInvoicse frmXemhoadonInvoicse = new frmXemhoadonInvoicse();
                        frmXemhoadonInvoicse.path = _content;
                        frmXemhoadonInvoicse.invoicse = this;
                        frmXemhoadonInvoicse.ShowDialog();
                    }
                    else
                    {
                        //frmTaihoadonvb frmTaihoadonvb = new frmTaihoadonvb();
                        //_content = _content.Replace(".pdf", "");
                        //string fn = Path.GetFileName(_content);

                        //frmTaihoadonvb.hdlink= fn;

                        //frmTaihoadonvb.Show();
                    }
                }
                    
               
            }
        }

        private void vb6Xemhoadon_FormClosing(object sender, FormClosingEventArgs e)
        {
           
        }

        private void vb6Xemhoadon_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }
    }
}