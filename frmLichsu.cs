﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Data.OleDb;
using System.Reflection;
using System.IO;

namespace SaovietTax
{
    public partial class frmLichsu : DevExpress.XtraEditors.XtraForm
    {
        public frmLichsu()
        {
            InitializeComponent();
        }
        public frmMain frmMain { get; set; }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            string queryCheckVatTu = @"SELECT * FROM tbimport   WHERE  Status <> 0 and Status <> -1 AND Type = ?";
            int type=radioButton1.Checked ? 1 : 2; // Kiểm tra xem checkbox có được chọn hay không 
            var parameterss = new OleDbParameter[]
            {
                 new OleDbParameter("?", type)
            };
            var kq = ExecuteQuery(queryCheckVatTu, parameterss);
            if (kq.Rows.Count > 0)
            {

                var filteredRows = kq.AsEnumerable()
                   .Where(row => DateTime.TryParse(row["NLap"].ToString(), out DateTime nl)
                                 && nl.Date >= dtTungay.DateTime.Date && nl.Date <= dtDenngay.DateTime.Date);

                // Kiểm tra xem có hàng nào sau khi lọc không
                if (filteredRows.Any())
                {
                    kq = filteredRows.CopyToDataTable();
                }
                else
                {
                    // Xử lý trường hợp không có hàng nào
                    // Ví dụ: tạo DataTable rỗng hoặc thông báo cho người dùng
                    kq = kq.Clone(); // Tạo một DataTable rỗng với cấu trúc giống như kq
                }
            }
            foreach(DataRow  row in kq.Rows)
            {
                row["Ten"] = Helpers.ConvertVniToUnicode(row["Ten"].ToString().Trim()); 
            }
            gridControl1.DataSource = kq;
        }
        string dbPath = "";
        string password, connectionString;
        private DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            try
            {
                string content = File.ReadAllText(filePaths);
                dbPath = content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi đọc file: " + ex.Message);
            }
            string password = "1@35^7*9)1";
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Thêm các tham số vào command
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command))
                        {
                            dataAdapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

            return dataTable; // Trả về DataTable chứa dữ liệu
        }
        private void frmLichsu_Load(object sender, EventArgs e)
        {
            dtTungay.DateTime = frmMain.dtFrom;
            dtDenngay.DateTime = frmMain.dtTo;
        }
    }
}