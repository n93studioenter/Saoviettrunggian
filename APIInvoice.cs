using DevExpress.XtraEditors;
using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SaovietTax.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tensorflow;
using Cookie = System.Net.Cookie;
namespace SaovietTax
{
    public partial class APIInvoice : Form
    {
        public APIInvoice()
        {
            InitializeComponent();
        }
        public class LoginResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public long iat { get; set; }
            public string invoice_cluster { get; set; }
            public int type { get; set; }
            public string jti { get; set; }
        }
        public class UseCookie
        {
             public string __cf_bm { get; set; }
            public string JSESSIONID { get; set; }
            public string access_token { get; set; }
            public string session_token { get; set; }
        }
        public static LoginResponse loginResponse { get; set; } = new LoginResponse();
        public static UseCookie useCookie { get; set; } = new UseCookie();
        private async Task Login()
        {
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var row = dtInvoiceInfo.Rows[0];

            string username = row["Username"]?.ToString();
            string password = row["Password"]?.ToString();

            var url = "https://vinvoice.viettel.vn/api/auth/login";

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                // Tùy chọn: tự động xử lý cookie
                handler.UseCookies = true;
                handler.CookieContainer = new CookieContainer();

                using (HttpClient client = new HttpClient(handler))
                {
                    // giống Postman
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                                    var json = $@"{{
                        ""username"": ""{username}"",
                        ""password"": ""{password}"",
                        ""rememberMe"": false,
                        ""captcha"": """"
                    }}";

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);

                    // *** CÁCH LẤY COOKIE ***
                    // Lấy tất cả cookies từ response
                    var cookies = handler.CookieContainer.GetCookies(new Uri(url));

                    foreach (System.Net.Cookie cookie in cookies)
                    {
                        //MessageBox.Show($"Cookie: {cookie.Name} = {cookie.Value}");

                        // Nếu bạn muốn lấy riêng cookie __cf_bm
                        if (cookie.Name == "__cf_bm")
                        {
                            string cf_bm_value = cookie.Value;
                            useCookie.__cf_bm = cf_bm_value;
                        }
                        if (cookie.Name == "JSESSIONID")
                        {
                            string JSESSIONID_value = cookie.Value;
                            useCookie.JSESSIONID = JSESSIONID_value;
                        }
                        if (cookie.Name == "access_token")
                        {
                            string access_token_value = cookie.Value;
                            useCookie.access_token = access_token_value;
                        }
                        if (cookie.Name == "session_token")
                        {
                            string session_token_value = cookie.Value;
                            useCookie.session_token = session_token_value;
                        }
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);
                    if (loginResponse != null)
                    {
                        if (_content == "1")
                        {
                            btnGetTemplate.PerformClick();
                        }
                        else
                        {
                            simpleButton3.PerformClick();
                        }
                    }
                }
            }
        }
        private async  void APIInvoice_Load(object sender, EventArgs e)
        {
          

            using (HttpClient client = new HttpClient())
            {
                string url = $"https://mst.vn/api/company/{"3502495312"}";
                var res = await client.GetStringAsync(url); 
            }

            //await LoginWithSelenium();   // Gọi hàm login mới
            string dbPath = "";
            string password = "1@35^7*9)1";
            string appPath = Assembly.GetExecutingAssembly().Location;

            // Lấy thư mục chứa ứng dụng
            string directoryPath = Path.GetDirectoryName(appPath);

            // Xóa phần \bin\Debug để lấy đường dẫn gốc
            string rootDirectory = Path.GetFullPath(Path.Combine(directoryPath, @"..\.."));

            // Tạo đường dẫn đến file dpPath.txt trong thư mục hoadon
            string filePaths = Path.Combine(rootDirectory, "Hoadon", "dpPath.txt");
            string pathThumuc = Path.Combine(rootDirectory);
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
            connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};";
            //Đọc file txt
            string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
            _content = File.ReadAllText(filePath);
            await Login();
        }
        string _content;
        private IWebDriver driver;
        private async Task LoginWithSelenium()
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");

                // options.AddArgument("--headless");   // Bỏ comment nếu muốn chạy ngầm (dễ bị block hơn)

                driver = new ChromeDriver(options);

                lblStatus.Text = "Đang mở trang đăng nhập...";
                driver.Navigate().GoToUrl("https://vinvoice.viettel.vn/account/login");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(25));

                lblStatus.Text = "Đang tìm form đăng nhập...";

                // === SỬA SELECTOR Ở ĐÂY ===
                IWebElement txtUsername;
                IWebElement txtPassword;
                IWebElement btnLogin;

                try
                {
                    // Cách 1: Tìm theo placeholder "Tên đăng nhập"
                    txtUsername = wait.Until(d => d.FindElement(By.CssSelector("input[placeholder*='Nhập tên đăng nhập của bạn *']")));
                }
                catch
                {
                    // Cách 2: Tìm input đầu tiên kiểu text/email
                    txtUsername = driver.FindElement(By.CssSelector("input[type='text'], input[type='email'], input[name='username']"));
                }

                try
                {
                    // Tìm input password
                    txtPassword = driver.FindElement(By.CssSelector("input[type='password']"));
                }
                catch
                {
                    txtPassword = driver.FindElement(By.CssSelector("input[name='password']"));
                }

                // Điền thông tin
                txtUsername.Clear();
                txtUsername.SendKeys("3502412669");   // ← THAY BẰNG USERNAME THẬT

                txtPassword.Clear();
                txtPassword.SendKeys("Tdt@12345678");        // ← THAY BẰNG PASSWORD THẬT

                lblStatus.Text = "Đang click Đăng nhập...";

                // === Selector nút Đăng nhập (đã sửa) ===
                try
                {
                    btnLogin = driver.FindElement(By.CssSelector("button[type='submit']"));
                }
                catch
                {
                    try
                    {
                        btnLogin = driver.FindElement(By.XPath("//button[contains(., 'Đăng nhập')]"));
                    }
                    catch
                    {
                        btnLogin = driver.FindElement(By.CssSelector("button.btn, button.btn-primary, button.login-button"));
                    }
                }

                btnLogin.Click();

                // Chờ sau khi login thành công
                wait.Until(d =>
                    d.Url.Contains("vinvoice.viettel.vn") &&
                    !d.Url.ToLower().Contains("login") &&
                    !d.Url.ToLower().Contains("forgot")
                );

                await Task.Delay(4000); // Chờ JavaScript set thêm cookie (_ga, showNoti...)

                lblStatus.Text = "Đăng nhập thành công! Đang lấy cookies...";

                var allCookies = driver.Manage().Cookies.AllCookies;

                richTextBox1.Clear();
                richTextBox1.AppendText($"✅ Đăng nhập thành công!\n");
                richTextBox1.AppendText($"Tìm thấy {allCookies.Count} cookies:\n\n");

                foreach (var cookie in allCookies.OrderBy(c => c.Name))
                {
                    lstallCookies.Add(cookie.Name, cookie.Value);   // Lưu vào dictionary để dùng sau
                    richTextBox1.AppendText($"Name : {cookie.Name}\n");
                    richTextBox1.AppendText($"Value: {cookie.Value}\n");
                    richTextBox1.AppendText($"Domain: {cookie.Domain}\n");
                    richTextBox1.AppendText(new string('-', 60) + "\n");
                }
                //allCookies.Clear();

                var seleniumCookies = driver.Manage().Cookies.AllCookies;

                //foreach (var cookie in seleniumCookies)
                //{
                //    allCookies[cookie.Name] = cookie.Value;

                //    // In ra để bạn xem
                //    richTextBox1.AppendText($"Cookie lưu: {cookie.Name} = {cookie.Value.Substring(0, Math.Min(50, cookie.Value.Length))}...\n");
                //}
                string cookieString = string.Join("; ", allCookies.Select(c => $"{c.Name}={c.Value}"));
                richTextBox1.AppendText("\n=== COOKIE STRING ĐẦY ĐỦ ===\n" + cookieString);

                MessageBox.Show($"Thành công! Lấy được {allCookies.Count} cookies.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message + "\n\nHãy chụp màn hình lỗi và gửi lại.", "Lỗi Selector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
        }
        private Dictionary<string, string> lstallCookies = new Dictionary<string, string>();   // Lưu tất cả cookie
        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            var baseUrl = "https://vinvoice.viettel.vn";
            var cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            })
            using (var client = new HttpClient(handler))
            {
                // 1. Thêm cookies đã có vào CookieContainer
                var uri = new Uri(baseUrl);

                // Thêm __cf_bm
                cookieContainer.Add(uri, new Cookie("__cf_bm", useCookie.__cf_bm));

                // Thêm JSESSIONID
                cookieContainer.Add(uri, new Cookie("JSESSIONID", useCookie.JSESSIONID));

                // 2. Thêm headers (access_token và session_token)
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {useCookie.access_token}");
                client.DefaultRequestHeaders.Add("X-Session-Token", useCookie.session_token);

                // 3. Gọi API search (cookies sẽ tự động được gửi từ CookieContainer)
                var searchUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/product/search?page=0&size=10&productCode.contains=&productName.contains=&unitName.contains=&sort=createdDate%2Cdesc";

                var response = await client.GetAsync(searchUrl);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Thành công!\n{result}");
                }
                else
                {
                    MessageBox.Show($"Lỗi {response.StatusCode}:\n{result}");
                }

                // Debug: Kiểm tra cookies đã được gửi
                Console.WriteLine("Cookies in container:");
                var cookies = cookieContainer.GetCookies(uri);
                foreach (Cookie cookie in cookies)
                {
                    Console.WriteLine($"{cookie.Name} = {cookie.Value}");
                }
            }
        }

        private async Task<string> CallApiWithAllCookies(string fullUrl)
        {
            if (lstallCookies.Count == 0)
            {
                MessageBox.Show("Chưa có cookie. Vui lòng đăng nhập lại.");
                return "No cookie";
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);   // hoặc Post nếu cần

                    // Tạo Cookie string từ tất cả cookie đã lưu
                    string cookieHeader = string.Join("; ", lstallCookies.Select(c => $"{c.Key}={c.Value}"));
                    cookieHeader = "ga=GA1.1.2010400788.1776829182; _ga_XPBRQ19161=GS2.1.s1776829181$o1$g0$t1776829181$j60$l0$h0; access_token=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX25hbWUiOiIzNTAyNDEyNjY5Iiwic2NvcGUiOlsib3BlbmlkIl0sImV4cCI6MTc3NjgzMDM4MSwidHlwZSI6MSwiaWF0IjoxNzc2ODI5MTgxLCJpbnZvaWNlX2NsdXN0ZXIiOiJjbHVzdGVyNSIsImF1dGhvcml0aWVzIjpbIlJPTEVfVVNFUiJdLCJqdGkiOiJjM2M5Njc5MC1lNjE1LTQ3ZmMtYTkwNS0wMDU2ZmEyMzRhMTQiLCJjbGllbnRfaWQiOiJ3ZWJfYXBwIn0.BMP67lEVbkTFINyxIaTaibmqs1ilhDNYzLYlho8CI3dfe2l50O5HkHKlB8qi35lPVS8Hj9wuwv_91rK1jREwyzpx2JVrik5LiSDXHFHySHDzL6zFx_zXf-0D-XQW6hXQeYMR24AcYrBg47pR747UGVXwHT1faq7xiuBsJXJ2BZ3neDB06NQrrhmPtbLr7zEbkwo2ZNY74xu8B1BpfdTfkvOaB9hut2vgGb_Q5UH98cpFELQGJdxiMX7eH3zLo40g2_-Q1DPhJq61cYnIj5F3YVmtGUYXkv131Wb8-ToxIdwq91vpirtceHOvFgmGjNVKu-fT9G55T5k3e5u-ubVjQA; session_token=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VyX25hbWUiOiIzNTAyNDEyNjY5Iiwic2NvcGUiOlsib3BlbmlkIl0sImF0aSI6ImMzYzk2NzkwLWU2MTUtNDdmYy1hOTA1LTAwNTZmYTIzNGExNCIsImV4cCI6MTc3NzQzMzk4MSwidHlwZSI6MSwiaWF0IjoxNzc2ODI5MTgxLCJpbnZvaWNlX2NsdXN0ZXIiOiJjbHVzdGVyNSIsImF1dGhvcml0aWVzIjpbIlJPTEVfVVNFUiJdLCJqdGkiOiJiOWExNDMzZC0wMDk5LTQ3ZTMtOGU1ZS01NGNkZTBhMTg5NzIiLCJjbGllbnRfaWQiOiJ3ZWJfYXBwIn0.dYSXxe0NKhWMyiRE-d0Z2MmN5YhtshDwsi5lgbYWNAQNS0CWtsFTYz1bZcQQhe0sgIern06FQWekqvQha5KEzWyox2F7ffqeZNd9QlRQuy3IpA7QkMVYywkXpjy1tCCZNfxaI-KAQXWIQdRlGWi5AkpeyTjSKcwiK1_-ZpMq10b-g33HL_5B6uYVb4nkmgzjWpvOVDKN5Zb-0BoJFvx6_QvfyDZfSVoMuEDkk4iqddsNj-dO0O0aJlqHOinR61AUuMjeprX3v0nYdRVwY95xaC1P4DxjGgSMiINjgNXU9MuATMsKZpnXrmLIdj3WxvtTkIjla3CauGioEddPx7l_eg; JSESSIONID=Zct53cLKDDgd6aXzhcWkntT8Y1d8hy5bZ2QyuQCB; showPopup=0; __cf_bm=PSkGdoZqa2b032gCkx.MPb4YmwzDlqwFh3asQAjjRBA-1776829218.7304292-1.0.1.1-pQY9qYScyeJcIWneYLQsxKfyAMD_1rBMr.U0BVn5xr.4l67lRTL87IfAP7Hm8tdXP5Oiogql0EHj1iz53a9qb0u0Xpqt5xn7TFCtB5PmvA6NYOafYopCpt5mh.RLevab";
                    request.Headers.Add("Cookie", cookieHeader);

                    // Header bổ sung quan trọng
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    request.Headers.Add("Accept", "application/json, text/plain, */*");
                    request.Headers.Add("Referer", "https://vinvoice.viettel.vn/");

                    var response = await client.SendAsync(request);
                    string result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return result;
                    }
                    else
                    {
                        richTextBox1.AppendText($"\nLỗi {(int)response.StatusCode}: {result}\n");
                        return $"Error {(int)response.StatusCode}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gọi API: " + ex.Message);
                return "Exception: " + ex.Message;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            string url= "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/product/search?page=0&size=10&productCode.contains=&productName.contains=&unitName.contains=&sort=createdDate%2Cdesc";
            var apiResult = CallApiWithAllCookies(url).GetAwaiter().GetResult();
        }
        string password, connectionString;
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
        private async void simpleButton3_Click(object sender, EventArgs e)
        {
          

            string query = "SELECT * FROM tbRegister";
            string pathluu = "";    
            var kq = ExecuteQuery(query, null); 
            try
            {
                if (kq.Rows.Count > 0)
                {
                    pathluu = kq.Rows[0]["Hoadonpath"].ToString();
                    pathluu = Directory.GetParent(pathluu).FullName;
                    pathluu = Path.Combine(pathluu, $"HoaDon/HdNhap");
                    // ✅ kiểm tra tồn tại
                    if (!Directory.Exists(pathluu))
                    {
                        Directory.CreateDirectory(pathluu);
                    }
                    //Lấy nam taichinh
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }


            var getsplit = _content.Split('_');

            // Có thể bạn nên JOIN theo ID, không phải MaSo
            var qrTimct = @"
    SELECT ChungTu.*,HOADON.*
    FROM ChungTu 
    INNER JOIN HOADON ON HOADON.MaSo = ChungTu.MaSo
    WHERE ChungTu.SoHieu = ? AND KyHieu = ? AND NgayCT=? ";

            var parameterss = new OleDbParameter[]
            {
    new OleDbParameter("?", getsplit[0]),
    new OleDbParameter("?", getsplit[1]),
    new OleDbParameter("?", getsplit[2])
            };

            var kq2 = ExecuteQuery(qrTimct, parameterss);
            //Lấy danh sách chứng từ từ MaCT
            var sql= "SELECT * FROM KhachHang WHERE MaSo = ?";
            parameterss = new OleDbParameter[]
          {
    new OleDbParameter("?",  kq2.Rows[0]["MaKhachHang"]),
          };
            var dtKhachhang = ExecuteQuery(sql, parameterss);

            sql = "SELECT * FROM ChungTu WHERE MaCT = ?";
            var param = new OleDbParameter[]
            {
                new OleDbParameter("?", kq2.Rows[0]["MaCT"])
            };
            //Lấy data khách hàng Từ MaKhachHang 
            var kq3 = ExecuteQuery(sql, param);
            //add datatable hang hoa
            DataTable dtHangHoa = new DataTable();
            dtHangHoa.Columns.Add("ItemCode", typeof(string));
            dtHangHoa.Columns.Add("ItemName", typeof(string));
            dtHangHoa.Columns.Add("UnitName", typeof(string));
            dtHangHoa.Columns.Add("UnitPrice", typeof(decimal));
            dtHangHoa.Columns.Add("Quantity", typeof(decimal));
            dtHangHoa.Columns.Add("Amount", typeof(decimal));
            foreach (DataRow row in kq3.Rows)
            {
                try
                {
                    if( row["MaVattu"].ToString().Trim() == "0")
                    {
                        continue; // Bỏ qua nếu MaVattu trống
                    }
                    string sqlHangHoa = "SELECT * FROM Vattu WHERE MaSo = ?";
                    var paramHangHoa = new OleDbParameter[]
                    {
                    new OleDbParameter("?", row["MaVattu"])
                    };
                    var kqHangHoa = ExecuteQuery(sqlHangHoa, paramHangHoa);
                    double sops = row["SoPS"].ToString() == "" ? 0 : Convert.ToDouble(row["SoPS"]);
                    double soluong = row["SoPS2Co"].ToString() == "" ? 0 : Convert.ToDouble(row["SoPS2Co"]);
                    double dongia = Math.Round(sops / soluong);
                    string tenhh = kqHangHoa.Rows[0]["TenVattu"].ToString();
                    string donvitinh = kqHangHoa.Rows[0]["DonVi"].ToString();
                    dtHangHoa.Rows.Add("", tenhh, donvitinh, dongia, soluong, sops);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lấy thông tin hàng hóa: " + ex.Message);
                }
            }
            var itemInfo = dtHangHoa.AsEnumerable()
    .Select((row, index) => new
    {
        lineNumber = index + 1,
        itemCode = "",
        itemName = Helpers.ConvertVniToUnicode(row["ItemName"].ToString()),
        unitName = Helpers.ConvertVniToUnicode(row["UnitName"].ToString()),
        unitPrice = Convert.ToDouble(row["UnitPrice"]),
        quantity = Convert.ToDouble(row["Quantity"]),
        itemTotalAmountWithoutVat = Convert.ToDouble(row["Amount"]),
        selection = 1
    }).ToArray(); 
            var baseUrl = "https://vinvoice.viettel.vn";
            var cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            })
            using (var client = new HttpClient(handler))
            {
                // 1. Thêm cookies
                var uri = new Uri(baseUrl);
                cookieContainer.Add(uri, new Cookie("__cf_bm", useCookie.__cf_bm));
                cookieContainer.Add(uri, new Cookie("JSESSIONID", useCookie.JSESSIONID));

                // 2. Thêm headers
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {useCookie.access_token}");
                client.DefaultRequestHeaders.Add("X-Session-Token", useCookie.session_token);
                client.DefaultRequestHeaders.Add("Referer", "https://vinvoice.viettel.vn/invoice-management/invoice-draft");
                client.DefaultRequestHeaders.Add("Origin", "https://vinvoice.viettel.vn");

                // 3. Tạo JSON data (toàn bộ payload)
                dynamic dataJson = new ExpandoObject();

                //Kiểm tra xem update hay tạo mới
                var sqlcheck = @"select * from HoaDon  WHERE MaSo =?";
                var paramcheck = new OleDbParameter[]
                { 
        new OleDbParameter("?", kq2.Rows[0]["HOADON.MaSo"].ToString()),

                };
                var checkupdate = ExecuteQuery(sqlcheck, paramcheck);
                if (!string.IsNullOrEmpty(checkupdate.Rows[0]["IdNhap"].ToString()))
                {
                    dataJson.id= checkupdate.Rows[0]["IdNhap"].ToString();
                    dataJson.transactionUuid = null;
                }
                dataJson.invoiceType = "1";
                dataJson.templateCode = "1/001";
                dataJson.invoiceSeri = "C22TDT";
                dataJson.buyerTaxCode = dtKhachhang.Rows[0]["MST"].ToString();
                dataJson.buyerName = Helpers.ConvertVniToUnicode(kq2.Rows[0]["Nguoimuahang"].ToString());
                dataJson.buyerAddress = Helpers.ConvertVniToUnicode(dtKhachhang.Rows[0]["DiaChi"].ToString());
                dataJson.totalAmountWithoutVAT = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() != "14038").Sum(m => Convert.ToDouble(m["SoPS"]));
                dataJson.totalVATAmount = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() == "14038").Sum(m => Convert.ToDouble(m["SoPS"]));
                dataJson.discountAmount = 0;
                dataJson.totalAmountWithVAT = kq3.AsEnumerable().Sum(m => Convert.ToDouble(m["SoPS"]));
                dataJson.totalAmountAfterDiscount = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() != "14038").Sum(m => Convert.ToDouble(m["SoPS"]));
                dataJson.totalServiceChargeAmount = 0;
                dataJson.totalExciseTaxAmount = 0;
                dataJson.currencyCode = "VND";
                dataJson.buyerViewStatus = 0;
                dataJson.invoiceTemplateId = int.Parse(getsplit[3].ToString());
                dataJson.paymentMethod = 3;
                dataJson.buyerUnitName = Helpers.ConvertVniToUnicode(dtKhachhang.Rows[0]["Ten"].ToString());
                dataJson.paymentMethodName = "TM/CK";
                dataJson.domain = null;
                dataJson.autoCreatePdfInstance = 0;
                dataJson.invoiceTypeId = 5;
                dataJson.listProduct = new
                {
                    itemInfo = itemInfo,
                    invoiceTaxBreakdowns = new[]
                    {
                        new
                        {
                            vatPercentage = kq2.Rows[0]["TyLe"].ToString(),
                            vatTaxAmount = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() == "14038").Sum(m => Convert.ToDouble(m["SoPS"])),
                            vatTaxableAmount = kq3.AsEnumerable().Where(m => m["MaTKTCCo"].ToString() != "14038").Sum(m => Convert.ToDouble(m["SoPS"]))
                        }
                    }
                 };
                dataJson.listInfoUpdate = new object[] { };
                dataJson.exchangeRate = 1;
                dataJson.listElectricityWater = new object[] { };
                dataJson.totalAmountWithTaxInWords = "Bảy mươi tám nghìn bảy trăm sáu mươi đồng";
                dataJson.hdbtscInfo = null;
                dataJson.fileSpecification = null;
                dataJson.listFuelInfo = new object[] { };
                dataJson.source = "WEB";
                 

                // 4. Tạo MultipartFormDataContent với field "data"
                var formData = new MultipartFormDataContent();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(dataJson);
                formData.Add(new StringContent(json, System.Text.Encoding.UTF8, "application/json"), "data");

                // 5. Gọi API
                var saveUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice/draft/save";
                var response = await client.PostAsync(saveUrl, formData);
                var result = await response.Content.ReadAsStringAsync();
               

                if (response.IsSuccessStatusCode)
                {
                    var jObj = JObject.Parse(result);
                    long invoiceId = jObj["data"]["id"].Value<long>();
                    var updateQr = @"UPDATE HoaDon  SET IdNhap = ? WHERE MaSo =?";
                    var updateParameters = new OleDbParameter[]
                    {
        new OleDbParameter("?", invoiceId.ToString()), // Cập nhật giá trị TiLe
        new OleDbParameter("?", kq2.Rows[0]["HOADON.MaSo"].ToString()),

                    };
                    var updateRowsAffected = ExecuteQueryResult(updateQr, updateParameters);
                   // MessageBox.Show($"Tạo hóa đơn thành công!\n{result}");

                    var url = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/invoice/gen-pdf-invoice?isDraft=0";

                    var json2 = JsonConvert.SerializeObject(dataJson);

                    var content = new StringContent(json2, Encoding.UTF8, "application/json");

                    // ⚠️ QUAN TRỌNG: thêm cookie/token giống browser
                    //client.DefaultRequestHeaders.Add("Cookie", "YOUR_COOKIE_HERE");

                    var response2 = await client.PostAsync(url, content);

                    var pdfBytes = await response2.Content.ReadAsByteArrayAsync();
                    string path = Path.Combine(pathluu, $"{invoiceId.ToString()}.pdf");
                    File.WriteAllBytes(path, pdfBytes);
                    // mở file
                    Process.Start(new ProcessStartInfo(path)
                    {
                        UseShellExecute = true
                    });
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show($"Lỗi {response.StatusCode}:\n{result}");
                }
            }
        }

        private async void btnGetTemplate_Click(object sender, EventArgs e)
        {
            var baseUrl = "https://vinvoice.viettel.vn";
            var cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            })
            using (var client = new HttpClient(handler))
            {
                // 1. Thêm cookies đã có vào CookieContainer
                var uri = new Uri(baseUrl);

                // Thêm __cf_bm
                cookieContainer.Add(uri, new Cookie("__cf_bm", useCookie.__cf_bm));

                // Thêm JSESSIONID
                cookieContainer.Add(uri, new Cookie("JSESSIONID", useCookie.JSESSIONID));

                // 2. Thêm headers (access_token và session_token)
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {useCookie.access_token}");
                client.DefaultRequestHeaders.Add("X-Session-Token", useCookie.session_token);

                // 3. Gọi API search (cookies sẽ tự động được gửi từ CookieContainer)
                var searchUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/management-order-template/search-template-order?page=0&size=10&invoiceTypeId.equals=&invoiceName.contains=&barcodeType.equals=&status.equals=&sort=id%2Cdesc&sort=createdDate";

                var response = await client.GetAsync(searchUrl);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var list = JObject.Parse(result)["data"]["content"]
                    .Select(x => new
                    {
                        Id = (long)x["id"],
                        TemplateCode = (string)x["templateCode"],
                        InvoiceName = (string)x["invoiceName"]
                    })
                    .ToList();
                    foreach(var item in list)
                    {
                        string sqlCheck = "SELECT * FROM tbInvoiceTemplate WHERE Id = @Id";
                        var paramHangHoa = new OleDbParameter[]
                  {
                    new OleDbParameter("?", item.Id.ToString())
                  };
                        var kqHangHoa = ExecuteQuery(sqlCheck, paramHangHoa);
                        if (kqHangHoa.Rows.Count==0)
                        {
                            string sqlInsert = "INSERT INTO tbInvoiceTemplate (ID, Code, Name) VALUES (?, ?, ?)";
                            var updateParameters = new OleDbParameter[]
                   {
        new OleDbParameter("?", item.Id.ToString()), // Cập nhật giá trị TiLe
        new OleDbParameter("?", item.TemplateCode),
        new OleDbParameter("?", item.InvoiceName)   

                   };
                            var updateRowsAffected = ExecuteQueryResult(sqlInsert, updateParameters);
                        }
                        // richTextBox1.AppendText($"ID: {item.Id}, TemplateCode: {item.TemplateCode}, InvoiceName: {item.InvoiceName}\n");
                    }
                    MessageBox.Show($"Thành công!\n{result}");
                }
                else
                {
                    MessageBox.Show($"Lỗi {response.StatusCode}:\n{result}");
                }

                 
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {

        }

        public int ExecuteQueryResult(string query, params OleDbParameter[] parameters)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Kết nối đến cơ sở dữ liệu thành công!");

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // Thêm tham số
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    // Thực thi INSERT, UPDATE, DELETE
                    command.ExecuteNonQuery();
                }

                // Lấy ID vừa thêm bằng @@IDENTITY
                using (OleDbCommand idCommand = new OleDbCommand("SELECT @@IDENTITY", connection))
                {
                    object result = idCommand.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

    }
}
