using ClosedXML.Excel;
using DevExpress.Utils;
using DevExpress.Utils.Extensions;
using DevExpress.XtraEditors;
using SaovietTax.Database;
using SaovietTax.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private void CreateFolder(string path)
        {
            try
            {
                // Kiểm tra đường dẫn có tồn tại không
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Tạo danh sách các tháng cần kiểm tra (1-12)
                for (int month = 1; month <= 12; month++)
                {
                    // Tạo tên thư mục với định dạng 2 chữ số (01, 02, ..., 12)
                    string folderName = month.ToString("D1");
                    string folderPath = Path.Combine(path, folderName);

                    // Kiểm tra nếu thư mục chưa tồn tại thì tạo mới
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        Console.WriteLine($"Đã tạo thư mục: {folderPath}");
                    }
                    else
                    {
                        Console.WriteLine($"Thư mục đã tồn tại: {folderPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo thư mục: {ex.Message}");
                throw;
            }
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

                    //Kiểm tra va tạo đầu vào
                    string pVao = Path.Combine(savedPath, $"HD{DateTime.Now.Year}", "HDVao");
                    CreateFolder(pVao);

                    //Kiểm tra va tạo đầu ra
                    string pRa = Path.Combine(savedPath, $"HD{DateTime.Now.Year}", "HDRa");
                    CreateFolder(pRa);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
            //Chạy code tải excel
            TaiExcel();
        }
        DevExpress.XtraEditors.LabelControl lblThongBao;

        private void LoadMeaasge()
        {
            var svgImageBox2 = new DevExpress.XtraEditors.SvgImageBox();
            svgImageBox2.Location = new System.Drawing.Point(5, 7);
            svgImageBox2.Name = "svgImageBox2";
            svgImageBox2.Size = new System.Drawing.Size(69, 56);

            // 👇 Set background trong suốt
            svgImageBox2.BackColor = Color.Transparent;

            // Icon Calendar màu xám
            string svgCalendarGreen = @"<svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24'>
    <rect x='3' y='4' width='18' height='18' rx='2' fill='none' stroke='#4CAF50' stroke-width='2'/>
    <line x1='3' y1='10' x2='21' y2='10' stroke='#4CAF50' stroke-width='2'/>
    <line x1='8' y1='2' x2='8' y2='6' stroke='#4CAF50' stroke-width='2'/>
    <line x1='16' y1='2' x2='16' y2='6' stroke='#4CAF50' stroke-width='2'/>
</svg>";

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgCalendarGreen)))
            {
                svgImageBox2.SvgImage = DevExpress.Utils.Svg.SvgImage.FromStream(stream);
            }

            svgImageBox2.TabIndex = 0;
            this.Controls.Add(svgImageBox2);

            var labelControl3 = new DevExpress.XtraEditors.LabelControl();
            labelControl3.Location = new System.Drawing.Point(81, 40);
            labelControl3.Name = "labelControl3";
            labelControl3.Size = new System.Drawing.Size(63, 16);
            labelControl3.TabIndex = 2;
            labelControl3.Text = "1 cảnh báo";
            // 
            // labelControl2
            // 
            var labelControl2 = new DevExpress.XtraEditors.LabelControl();
            labelControl2.Appearance.Font = new System.Drawing.Font("Tahoma", 8.8F, System.Drawing.FontStyle.Bold);
            labelControl2.Appearance.Options.UseFont = true;
            labelControl2.Location = new System.Drawing.Point(80, 16);
            labelControl2.Name = "labelControl2";
            labelControl2.Size = new System.Drawing.Size(69, 18);
            labelControl2.TabIndex = 1;
            labelControl2.Text = "07/2026";

            var panelControl2 = new DevExpress.XtraEditors.PanelControl();
            panelControl2.Appearance.BackColor = System.Drawing.Color.White;
            panelControl2.Appearance.Options.UseBackColor = true;
            panelControl2.Controls.Add(labelControl3);
            panelControl2.Controls.Add(labelControl2);
            panelControl2.Controls.Add(svgImageBox2);
            panelControl2.Location = new System.Drawing.Point(5, 5);
            panelControl2.Name = "panelControl2";
            panelControl2.Size = new System.Drawing.Size(494, 97);
            panelControl2.TabIndex = 0;



            lblThongBao = new DevExpress.XtraEditors.LabelControl();
            lblThongBao.Text = "🔴 12 Hóa đơn chưa nhập";
            lblThongBao.Font = new Font("Segoe UI", 7, FontStyle.Bold);
            lblThongBao.ForeColor = Color.Red;
            lblThongBao.Appearance.BackColor = Color.Transparent;
            lblThongBao.Appearance.Options.UseBackColor = true;
            lblThongBao.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblThongBao.Size = new Size(200, 35);
            int margin = (int)(panelControl2.Width * 0.3); // 20% của 400 = 80px

            lblThongBao.Location = new Point(margin, 10); // Đặt vị trí trên form
            lblThongBao.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // Thêm vào form
            panelControl2.AddControl(lblThongBao);

            //
            lblThongBao = new DevExpress.XtraEditors.LabelControl();
            lblThongBao.Text = "🔴 12 Hóa đơn chưa nhập";
            lblThongBao.Font = new Font("Segoe UI", 7, FontStyle.Bold);
            lblThongBao.ForeColor = Color.Blue;
            lblThongBao.Appearance.BackColor = Color.Transparent;
            lblThongBao.Appearance.Options.UseBackColor = true;
            lblThongBao.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblThongBao.Size = new Size(200, 35);

            lblThongBao.Location = new Point(margin, 40); // Đặt vị trí trên form
            lblThongBao.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            // Thêm vào form
            panelControl2.AddControl(lblThongBao);
            panelControl2.Height = 100;


            this.panelControl1.Controls.Add(panelControl2);
        }
        private void TaiExcel()
        { 

        } 
        private void LoadGrid()
        {
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
            tbImport = ExecuteQuery(qrip);


            string qrtonkho = "select * from TonKho";
            var dtTonKho = ExecuteQuery(qrtonkho);
            string qrvt = "select * from Vattu";
            var dtVattu = ExecuteQuery(qrvt);
            string hangam = "";
            for (int i = DateTime.Now.Month; i >= 1; i--)
            {
                //Kiểm tra tồn kho
                hangam = "";
                string columnName = $"Luong_{i}";
                foreach (DataRow row in dtTonKho.Rows)
                {
                    // Lấy giá trị theo tên cột động
                    object value = row[columnName];
                    if (value != DBNull.Value && value != null)
                    {
                        double soLuong = Convert.ToDouble(value);
                        if (soLuong < 0)
                        {
                            var getvattu = dtVattu.AsEnumerable().Where(m => m["MaSo"].ToString() == row["MaVatTu"].ToString()).FirstOrDefault();
                            if (getvattu != null)
                            {
                                hangam += getvattu["SoHieu"] + ",";
                            }
                        }
                    }
                }

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

                DataTable getimportloi = new DataTable();
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
                            try
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
                                    hdchuanhapvao += 1;
                                }
                                tongvao += 1;
                            }
                            catch (Exception ex)
                            {

                            }

                        }
                    }
                    j++;
                }
                var getchungtuthang = lookupHoaDonCT.Where(m => m.NLap.Month == i && m.Type == 1).ToList();
                foreach (var it in getchungtuthang)
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
                                if (!KiemtrahoadonCT(getSohd, getkhhd, getdate, mstnm, 2))
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
                if (excelFiles.Count > 0 || excelFilesra.Count > 0)
                {
                    WarningData warningData = new WarningData();
                    warningData.Thang = i;
                    warningData.Hoadonthieu = $"{hdchuanhapvao} hd đầu vào, {hdchuanhapra} hd đầu ra";

                    //Import lỗi 
                    if (getimportloi.Rows.Count > 0)
                    {
                        foreach (DataRow item in getimportloi.Rows)
                        {
                            if (item["Type"].ToString() == "1")
                                importvaoloi += item["SHDon"].ToString() + ",";
                            else
                                importraloi += item["SHDon"].ToString() + ",";
                        }
                    }
                    warningData.Importloi = !string.IsNullOrEmpty(importvaoloi) ? $"Đầu vào  : {importvaoloi}" : "";
                    warningData.Importloi += !string.IsNullOrEmpty(importraloi) ? $" Đầu ra  : {importraloi}" : "";
                    warningData.HoaDonThua = !string.IsNullOrEmpty(hdnhapduDauvao) ? $"Đv : {hdnhapduDauvao}" : "";
                    warningData.HoaDonThua += !string.IsNullOrEmpty(hdnhapduDaura) ? $"Đr : {hdnhapduDaura}" : "";

                    if (sumDkNo != sumDkCo)
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
                    warningData.Hangam = hangam;
                    warningDatas.Add(warningData);
                }
            }

            gridControl1.DataSource = warningDatas.OrderByDescending(m => m.Thang);
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