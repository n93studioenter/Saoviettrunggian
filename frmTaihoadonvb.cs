using DevExpress.XtraEditors;
using DevExpress.XtraMap.Native;
using DevExpress.XtraReports.Design;
using DevExpress.XtraWaitForm;
using Newtonsoft.Json;
using SaovietTax.Database;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class frmTaihoadonvb : DevExpress.XtraEditors.XtraForm
    {
        public frmTaihoadonvb()
        {
            InitializeComponent();
        }
       
        public string GetInvoiceUrl(int invoiceType, string nbmst, string khhdon, string shdon, string Khmshdon)
        {
            string url;

            if (invoiceType == 4 || invoiceType == 6 || invoiceType == 8)
            {
                url = $"https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-xml?nbmst={nbmst}&khhdon={khhdon}&shdon={shdon}&khmshdon={Khmshdon}";
            }
            else if (invoiceType == 5 || invoiceType == 10)
            {
                url = $"https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-xml?nbmst={nbmst}&khhdon={khhdon}&shdon={shdon}&khmshdon={Khmshdon}";
            }
            else
            {
                throw new ArgumentException("Loại hóa đơn không hợp lệ.");
            }

            return url;
        }
        bool needlogin=true;

        string connectionString;
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
        string mytokken = "";
        private void Getttoken()
        {
            string querykh = @" SELECT *  FROM tbRegister"; // Sử dụng ? thay cho @mst trong OleDb

            var tbRegister = ExecuteQuery(querykh, new OleDbParameter("?", ""));
            string gettimeTokken = tbRegister.AsEnumerable().FirstOrDefault()["TimeTokken"].ToString();
            if (!string.IsNullOrEmpty(gettimeTokken))
            {
                var timpsan = DateTime.Now - DateTime.Parse(gettimeTokken);
                if (timpsan.TotalMinutes <= 10)
                {
                    needlogin = false;
                    mytokken = tbRegister.AsEnumerable().FirstOrDefault().Field<string>("tokken");
                }
            }
            if (needlogin)
            {

                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    string url = "https://hoadondientu.gdt.gov.vn:30000/captcha";
                    try
                    {
                        response = client.GetAsync(url).Result;
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show(ex.Message);
                        return;
                    }

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    MyJson myJson = JsonConvert.DeserializeObject<MyJson>(responseBody);
                    //string filePath = "output.svg";
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + "output.svg"; // Đảm bảo tệp ở cùng thư mục với chương trình
                                                                                            //Lưu chuỗi SVG vào tệp
                    File.WriteAllText(filePath, myJson.Content);
                    Thread.Sleep(2000);
                    SvgCaptchaSolver solver = new SvgCaptchaSolver();
                    string result = solver.SolveCaptcha(filePath);

                    url = "https://hoadondientu.gdt.gov.vn:30000/security-taxpayer/authenticate";
                    var payload = new
                    {
                        username = user,
                        password = password,
                        cvalue = result,
                        ckey = myJson.Key
                    };
                    try
                    {
                        string json = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = client.PostAsync(url, content).Result;
                        response.EnsureSuccessStatusCode();
                        Thread.Sleep(1000);
                        responseBody = response.Content.ReadAsStringAsync().Result;
                        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                        mytokken = tokenResponse.token;
                        string query = @"UPDATE tbRegister SET TimeTokken=?, tokken=? ";

                        var parameters = new OleDbParameter[]
                 {
               new OleDbParameter("?", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                 new OleDbParameter("?",mytokken)
                 };
                        int rowsAffected = ExecuteQueryResult(query, parameters);
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(200);
                        Getttoken();
                    }


                }
            }
        }
        string dbPath = "";
        public int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
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

                    int rowsAffected = command.ExecuteNonQuery(); // Thực thi câu lệnh
                    return rowsAffected;
                }
            }

            return -1;
        }
        private void LoadData()
        {
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
 
        }
        string mstcongty = "";
        string savedPath = "";
        string user = "";
        string password = "";
        string namtc = "";
        private async void frmTaihoadonvb_Load(object sender, EventArgs e)
        {
            this.Hide();
            LoadData();
         
            string query = "SELECT * FROM License";

            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            var kq = ExecuteQuery(query, null);
            mstcongty = kq.Rows[0]["MaSoThue"].ToString();
            namtc= kq.Rows[0]["NamTC"].ToString();
            query = "SELECT * FROM tbRegister";
            // Tạo mảng tham số với giá trị cho câu lệnh SQL

            kq = ExecuteQuery(query, null);
            savedPath = kq.Rows[0]["Hoadonpath"].ToString();
            user = kq.Rows[0]["Username"].ToString();
            password = kq.Rows[0]["Password"].ToString();

            Getttoken();

            string qr = "SELECT * FROM HoaDon";
            DataTable tbHoadon = ExecuteQuery(qr, null);
            string filePath = Path.Combine(savedPath, "hdlink.txt");

            if (File.Exists(filePath))
            {
                try
                {
                    string content = File.ReadAllText(filePath).Trim();
                    string mst = "";
                    var getsplit = content.Split('_');
                    string sokh = "1";
                    string sohd = Helpers.RemoveLeadingZeros(getsplit[2]);
                    string khhd = getsplit[3];
                    if (getsplit[1] == "8")
                    {
                        mst = mstcongty;
                    }
                    else
                    {
                        mst = getsplit[0];
                        var findmauhd= tbHoadon.AsEnumerable().Where(m=>m.Field<string>("KyHieu") == khhd && Helpers.RemoveLeadingZeros(m.Field<string>("SoHD")) ==sohd).FirstOrDefault();
                        if(findmauhd!=null)
                        {
                            double tt = findmauhd.Field<double>("ThanhTien");
                            if (tt == 0)
                            {
                                sokh = "2";
                            }
                        }

                    } 

                    //
                    string pathravao = getsplit[1] != "8" ? "HDVao" : "HDRa";
                    string fn = $"{mst}_{sohd}_{khhd}.zip";
                    int tuthang = int.Parse(getsplit[4]);
                    string yearpath = $"HD{namtc}";
                    string path = Path.Combine(savedPath, yearpath, pathravao, tuthang.ToString(), fn);
                    string url1 = GetInvoiceUrl(4, mst, khhd, sohd, sokh);
                    string url2 = GetInvoiceUrl(5, mst, khhd, sohd, sokh);
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mytokken);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                        // Danh sách các URL cần thực hiện
                        var urls = new string[] { url1, url2 }; // Thay thế url1 và url2 bằng các URL thực tế

                        foreach (var url in urls)
                        {
                            try
                            {
                                string rootPath = Path.GetDirectoryName(path);
                                string getnamefile = Path.GetFileNameWithoutExtension(path);
                                string directoryPath = Path.Combine(rootPath, "Giainen_" + getnamefile);
                                string targetFilePath = Path.Combine(rootPath, getnamefile + ".html");
                                if (File.Exists(targetFilePath))
                                {
                                    this.Close();
                                }
                                HttpResponseMessage response = await client.GetAsync(url);
                                response.EnsureSuccessStatusCode(); // Ném ngoại lệ nếu không thành công

                                // Đọc nội dung phản hồi dưới dạng byte
                                var fileBytes = await response.Content.ReadAsByteArrayAsync();

                                // Lưu file ZIP bằng FileStream
                                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                                {
                                    await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                                }

                                Console.WriteLine($"File ZIP đã được lưu tại: {path}");

                                try
                                { 

                                    ZipFile.ExtractToDirectory(path, directoryPath);
                                    var files = Directory.GetFiles(directoryPath, "invoice.html", SearchOption.AllDirectories);
                                  

                                    if (files.Length > 0)
                                    {
                                        File.Move(files.FirstOrDefault(), targetFilePath);
                                        File.Delete(path);
                                        Directory.Delete(directoryPath, true);
                                        Console.WriteLine($"File đã được xử lý từ URL: {url}");
                                        this.Close();
                                        break;

                                    }
                                    else
                                    {
                                        Console.WriteLine("Không tìm thấy file invoice.html để xử lý.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Lỗi khi giải nén hoặc xử lý file: {ex.Message}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Đã xảy ra lỗi với URL {url}: {ex.Message}");

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi khi đọc file: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("File status.txt không tồn tại tại: " + filePath);
            }

            this.Close();
        }

        private void frmTaihoadonvb_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }
    }
}