using ClosedXML.Excel;
using DevExpress.XtraEditors;
using SaovietTax.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class Vanguard : DevExpress.XtraEditors.XtraForm
    {
        public class WarningData
        {
            public int Thang { get; set; }
            public string Hoadonthieu { get; set; } 
            public string Importloi { get; set; }
            public string Hangam {  get; set; } 
            public string HethongTK { get; set; }   
            public string HoaDonThua {  get; set; } 
        }
        
        public Vanguard()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;

            // Đặt kích thước form (tuỳ chỉnh) 

            // Đặt form ở góc phải dưới
            this.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Right - this.Width,
                Screen.PrimaryScreen.WorkingArea.Bottom - this.Height
            );
        }
        string savedPath = "";
        public string password, connectionString;
        public string pathThumuc = "";
        public string dbPath = "";
        List<WarningData> warningDatas = new List<WarningData>();
        public HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)> lookupHoaDonCT { get; }
        = new HashSet<(string Mst, string SoHD, string KyHieu, DateTime NLap, int Type)>();
        DataTable dtChungtu { get; set; }
        DataTable tbImport { get; set; }
        public class HoaDonNhap
        {
            public string SoHD { get; set; }
            public DateTime NLap { get; set; }
        }
        public DataTable GetHeThongTK(int thang, int nam)
        {
            DataTable dt = new DataTable();

            // Tạo tên cột động theo tháng
            string colDkNo = $"DuNo_{thang - 1}";  // Dư nợ đầu kỳ
            string colDkCo = $"DuCo_{thang - 1}";  // Dư có đầu kỳ
            string colPsNo = $"No_{thang}";        // Phát sinh Nợ
            string colPsCo = $"Co_{thang}";        // Phát sinh Có
            string colCkNo = $"DuNo_{thang}";      // Dư nợ cuối kỳ
            string colCkCo = $"DuCo_{thang}";      // Dư có cuối kỳ

            string query = $@"
        SELECT DISTINCTROW 
            HeThongTK.SoHieu, 
            HeThongTK.Cap, 
            HeThongTK.Ten, 
            HeThongTK.Kieu, 
            HeThongTK.Loai, 
            HeThongTK.{colDkNo} AS DkNo, 
            HeThongTK.{colDkCo} AS DkCo, 
            HeThongTK.{colPsNo} AS PsNo, 
            HeThongTK.{colPsCo} AS PsCo, 
            HeThongTK.KC_N, 
            HeThongTK.KC_C, 
            HeThongTK.{colCkNo} AS CkNo, 
            HeThongTK.{colCkCo} AS CkCo
        FROM HeThongTK
        WHERE (
            (HeThongTK.MaTC = 0 OR HeThongTK.MaTC = HeThongTK.MaSo) 
            OR (HeThongTK.TK_ID3 MOD 10 >= 1)
        ) 
        AND (HeThongTK.Loai > 0)  
        AND HeThongTK.Cap <= 2 
        AND (
            HeThongTK.{colCkNo} <> 0 
            OR HeThongTK.{colCkCo} <> 0 
            OR HeThongTK.{colPsNo} <> 0 
            OR HeThongTK.{colPsCo} <> 0
        )";

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        conn.Open();
                        using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }

            return dt;
        }

        // Sử dụng: 
        private void Vanguard_Load(object sender, EventArgs e)
        {
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "hoadon", "dpPath.txt");
            pathThumuc = Path.Combine(rootDirectory);
            //MessageBox.Show(pathThumuc);
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
            string query = "SELECT * FROM tbRegister";
            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            var kq = ExecuteQuery(query, null);
            try
            {
                if (kq.Rows.Count > 0)
                {
                    savedPath = kq.Rows[0]["Hoadonpath"].ToString();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
                        string queryct = @"
                                       SELECT 
                        hd.SoHD,
                        hd.KyHieu,
                        hd.NgayPH,
                        hd.MaKhachHang,
                        kh.MST, 
                        ct.NgayCT,
                        ct.MaLoai
                    FROM 
                        ((Hoadon hd 
                        INNER JOIN 
                        Chungtu ct ON hd.MaSo = ct.MaSo)
                        INNER JOIN 
                        KhachHang kh ON hd.MaKhachHang = kh.MaSo)
                    WHERE 
                        hd.KyHieu <> '...'";

            dtChungtu = ExecuteQuery(queryct);

            lookupHoaDonCT.Clear();

            foreach (DataRow item in dtChungtu.Rows)
            {
                string soHD = Helpers.RemoveLeadingZeros(
                    item["SoHD"]?.ToString() ?? ""
                ).Trim();
                string KyHieu = item["KyHieu"]?.ToString() ?? "";
                DateTime ngayPH = ((DateTime)item["NgayCT"]).Date;

                int maKhachHang = (int)item["MaKhachHang"];

                string mst = item["MST"]?.ToString() ?? "";
                int Maloai = int.Parse(item["MaLoai"]?.ToString());
                if (Maloai == 8)
                {
                    Maloai = 2;
                }
                else
                {
                    Maloai = 1;
                }
                soHD = soHD.Replace(".", "").Trim();
                soHD = RemoveLeadingZeros(soHD);
                lookupHoaDonCT.Add((mst, soHD, KyHieu, ngayPH, Maloai));
            }

            string qrip = "SELECT * FROM tbImport";
            tbImport= ExecuteQuery(qrip);

            for (int i = DateTime.Now.Month; i >=1; i--)
            {
                //Kiem tra bảng cân đối ps
                        string qrac = @"
                SELECT DISTINCTROW 
                    HeThongTK.SoHieu, 
                    HeThongTK.Cap, 
                    HeThongTK.Ten, 
                    HeThongTK.Kieu, 
                    HeThongTK.Loai, 
                    HeThongTK.DuNo_7 AS DkNo, 
                    HeThongTK.DuCo_7 AS DkCo, 
                    HeThongTK.No_8 AS PsNo, 
                    HeThongTK.Co_8 AS PsCo, 
                    HeThongTK.KC_N, 
                    HeThongTK.KC_C, 
                    HeThongTK.DuNo_8 AS CkNo, 
                    HeThongTK.DuCo_8 AS CkCo
                FROM HeThongTK
                WHERE (
                    (HeThongTK.MaTC = 0 OR HeThongTK.MaTC = HeThongTK.MaSo) 
                    OR (HeThongTK.TK_ID3 Mod 10 >= 1)
                ) 
                AND (HeThongTK.Loai > 0)  
                AND HeThongTK.Cap <= 2 
                AND (
                    HeThongTK.DuNo_8 <> 0 
                    OR HeThongTK.DuCo_8 <> 0 
                    OR HeThongTK.No_8 <> 0 
                    OR HeThongTK.Co_8 <> 0
                )";

                var result = GetHeThongTK(i, DateTime.Now.Year);
                // Tổng DkNo
                var sumDkNo = result.AsEnumerable()
                    .Where(m => m["DkNo"] != DBNull.Value
                                && m["DkNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["DkNo"]));

                // Tổng DkCo
                var sumDkCo = result.AsEnumerable()
                    .Where(m => m["DkCo"] != DBNull.Value
                                && m["DkCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["DkCo"]));

                // Tổng PsNo
                var sumPsNo = result.AsEnumerable()
                    .Where(m => m["PsNo"] != DBNull.Value
                                && m["PsNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["PsNo"]));

                // Tổng PsCo
                var sumPsCo = result.AsEnumerable()
                    .Where(m => m["PsCo"] != DBNull.Value
                                && m["PsCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["PsCo"]));

                // Tổng CkNo
                var sumCkNo = result.AsEnumerable()
                    .Where(m => m["CkNo"] != DBNull.Value
                                && m["CkNo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["CkNo"]));

                // Tổng CkCo
                var sumCkCo = result.AsEnumerable()
                    .Where(m => m["CkCo"] != DBNull.Value
                                && m["CkCo"] != null
                                && m["Cap"].ToString() == "0")
                    .Sum(m => Convert.ToDecimal(m["CkCo"]));


                int hdchuanhapvao = 0;
                int hdchuanhapra = 0;
                int tongvao = 0;
                int tongra = 0;
                string importvaoloi = "";
                string importraloi = "";
                string hdnhapduDauvao = "";
                string hdnhapduDaura = "";
                List<HoaDonNhap> lstvao = new List<HoaDonNhap>();
                List<HoaDonNhap> lstRa = new List<HoaDonNhap>();
                //Lấy danh sách hoá đơn import lỗi
                var qr = tbImport.AsEnumerable()
                .Where(m => m.Field<DateTime>("NLap").Date.Month == i)
                .Where(m => m["Status"].ToString() == "2");

                DataTable getimportloi=new DataTable();
                if (qr.Any())
                {
                    getimportloi = qr.CopyToDataTable();
                }
                else
                {
                    // Tạo DataTable rỗng với cấu trúc giống tbImport
                    getimportloi = tbImport.Clone();
                }

                //Tìm đọc file excel từng tháng
                string pathYear = $"HD{DateTime.Now.Year}";
                string directoryPath2 = Path.Combine(savedPath, pathYear, "HDVao", i.ToString());

                var excelFiles = Directory.EnumerateFiles(directoryPath2, "*.xlsx", SearchOption.AllDirectories).ToList();

                int j = 1;
                foreach (var excelFile in excelFiles)
                {
                    using (var workbook = new XLWorkbook(excelFile))

                    {
                        var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                        foreach (var row in worksheet.RowsUsed().Skip(3)) // Bỏ qua 6 hàng đầu tiên
                        {
                            string khhd = row.Cell("B").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                            string getSHHD = row.Cell("C").Value.ToString(); // Lấy giá trị của cột A trong hàng hiện tại
                            string getSohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                            string GetNLap = row.Cell("E").Value.ToString();
                            string mstnb = row.Cell("F").Value.ToString();
                            DateTime getdate = DateTime.Parse(GetNLap);
                            HoaDonNhap HoaDonNhap = new HoaDonNhap();
                            HoaDonNhap.SoHD = getSohd;
                            HoaDonNhap.NLap = getdate;
                            lstvao.Add(HoaDonNhap);

                            if (!KiemtrahoadonCT(getSohd, getSHHD, getdate, mstnb, 1))
                            {
                                hdchuanhapvao+=1;
                            }
                            tongvao += 1;
                          
                        }
                    }
                    j++;
                }
                var getchungtuthang = lookupHoaDonCT.Where(m => m.NLap.Month == i && m.Type==1).ToList();
                foreach(var it in getchungtuthang)
                {
                    var check = !lstvao.Any(m => m.SoHD == it.SoHD);
                    if (check)
                    {
                        hdnhapduDauvao += it.SoHD + ",";
                    }
                }

                string directoryPathra = Path.Combine(savedPath, pathYear, "HDRa", i.ToString());
                var excelFilesra = Directory.EnumerateFiles(directoryPathra, "*.xlsx", SearchOption.AllDirectories).ToList();

                foreach (var excelFile in excelFilesra)
                {
                    using (var workbook = new XLWorkbook(excelFile))
                    {
                        var worksheet = workbook.Worksheet(1);
                        foreach (var row in worksheet.RowsUsed().Skip(3))
                        {
                            string GetNLap = row.Cell("E").Value.ToString();
                            string getSohd = Helpers.RemoveLeadingZeros(row.Cell("D").Value.ToString()); // Lấy giá trị của cột C trong hàng hiện tại 
                            string getkhhd = row.Cell("C").Value.ToString();
                            if (getSohd == "104")
                            {
                                int dngg = 10;
                            }
                            string mstnm = row.Cell("H").Value.ToString(); 
                          
                            if (DateTime.TryParse(GetNLap, out DateTime getdate))
                            {
                                DateTime gd = DateTime.Parse(GetNLap);
                                HoaDonNhap HoaDonNhap = new HoaDonNhap();
                                HoaDonNhap.SoHD = getSohd;
                                HoaDonNhap.NLap = gd;
                                lstRa.Add(HoaDonNhap);
                                if (!KiemtrahoadonCT(getSohd, getkhhd, getdate, mstnm,2))
                                {
                                    hdchuanhapra += 1;
                                }
                            }
                            tongra += 1;
                        }
                    }
                }

                var getchungtuthangra = lookupHoaDonCT.Where(m => m.NLap.Month == i && m.Type == 2).ToList();
                foreach (var it in getchungtuthangra)
                {
                    var check = !lstRa.Any(m => m.SoHD == it.SoHD);
                    if (check)
                    {
                        hdnhapduDaura += it.SoHD + ",";
                    }
                }
                if (excelFiles.Count > 0 || excelFilesra.Count>0)
                {
                    WarningData warningData = new WarningData();
                    warningData.Thang = i;
                    warningData.Hoadonthieu = $"{hdchuanhapvao} hd đầu vào, {hdchuanhapra} hd đầu ra";

                    //Import lỗi 
                    if(getimportloi.Rows.Count > 0)
                    {
                        foreach(DataRow item in getimportloi.Rows)
                        {
                            if (item["Type"].ToString() == "1")
                                importvaoloi += item["SHDon"].ToString() + ",";
                            else
                                importraloi += item["SHDon"].ToString() + ",";
                        }
                    }
                    warningData.Importloi = !string.IsNullOrEmpty(importvaoloi)? $"Đầu vào  : {importvaoloi}":"";
                    warningData.Importloi += !string.IsNullOrEmpty(importraloi) ? $" Đầu ra  : {importraloi}" : "";
                    warningData.HoaDonThua = !string.IsNullOrEmpty(hdnhapduDauvao)? $"Đv : {hdnhapduDauvao}":"";
                    warningData.HoaDonThua += !string.IsNullOrEmpty(hdnhapduDaura) ? $"Đr : {hdnhapduDaura}" : "";

                    if(sumDkNo!= sumDkCo)
                    {
                        warningData.HethongTK += $"Số dư đầu kỳ chưa cân {sumDkNo} -  {sumDkCo}";
                    }
                    if (sumPsNo != sumPsCo)
                    {
                        warningData.HethongTK += $"Số dư trong kỳ chưa cân {sumPsNo} -  {sumPsCo}";
                    }
                    if (sumCkNo != sumCkCo)
                    {
                        warningData.HethongTK += $"Số dư cuối kỳ chưa cân {sumCkNo} -  {sumCkCo}";
                    }
                    warningDatas.Add(warningData);
                } 
            }
          
            gridControl1.DataSource = warningDatas.OrderByDescending(m=>m.Thang); 
        }
        private bool KiemtrahoadonCT(string SoHD, string KyHieu, DateTime NLap, string Mst, int type)
        {
            if (Mst == "KL")
                Mst = "00";
            if (Mst.Length < 10)
                return lookupHoaDonCT.Any(m => m.SoHD == SoHD && m.KyHieu == KyHieu && m.NLap == NLap && m.Type == type);
            return lookupHoaDonCT.Contains((Mst, SoHD, KyHieu, NLap, type));
        }
        public static string RemoveLeadingZeros(string invoiceNumber)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
                return invoiceNumber;

            return Regex.Replace(invoiceNumber, "^0+", "");
        }
        public System.Data.DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();

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

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();    
        }
    }
}