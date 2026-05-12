using Microsoft.Web.WebView2.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class TendoInvoice : Form
    {
        public TendoInvoice()
        {
            InitializeComponent();
        }
        string password, connectionString;
        string _content;
        string bearerToken;
        private static readonly HttpClient httpClient = new HttpClient();

        private void Phathanhhoadon()
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            IWebDriver driver = new ChromeDriver(service, options);
            try
            {
                driver.Navigate().GoToUrl("https://id-v2.tendoo.vn/vi/signin");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                driver.Quit();
            }
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
        private void Addinvoice()
        {
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var rows = dtInvoiceInfo.Rows[0];

            string username = rows["Username"]?.ToString();
            string password = rows["Password"]?.ToString();

            string query = "SELECT * FROM tbRegister";
            string pathluu = "";
            var kq = ExecuteQuery(query, null);

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            options.AddArgument("--disable-logging");
            options.AddArgument("--log-level=3");
            options.AddExcludedArgument("enable-automation");
            IWebDriver driver = new ChromeDriver(service, options);
            try
            {
                driver.Navigate().GoToUrl("https://id-v2.tendoo.vn/vi/signin");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                var txtUser = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("phone_login_form_phone_number"));
                        return el.Displayed ? el : null;
                    }
                    catch { return null; }
                });

                txtUser.Clear();
                txtUser.SendKeys(username); // 👉 username của bạn

                // ===== PASSWORD =====
                var txtPass = driver.FindElement(By.Id("phone_login_form_pwd"));
                txtPass.Clear();
                txtPass.SendKeys(password);

                IWebElement button = driver.FindElement(By.XPath("//button[.//span[text()='Đăng nhập']]"));
                button.Click();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Close();
                // driver.Quit();
            }
        }
        private async void TendoInvoice_Load(object sender, EventArgs e)
        {
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
            string userDataFolder = Path.Combine(
                Application.StartupPath,
                "TendooProfile"
            );

            var env = await CoreWebView2Environment.CreateAsync(
                null,
                userDataFolder
            );

            await webView21.EnsureCoreWebView2Async(env);

            webView21.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            webView21.Source = new Uri("https://seller-v2.tendoo.vn/");
        }

        private async void CoreWebView2_NavigationCompleted(
      object sender,
      CoreWebView2NavigationCompletedEventArgs e)
        {
            string url = webView21.Source.ToString();

            // Nếu đã ở POS thì lấy token và gọi API
            if (url.Contains("/sales/pos"))
            {
                await LayBearerToken();
                return;
            }

            await Task.Delay(3000);

            // Lấy thông tin từ database
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var rows = dtInvoiceInfo.Rows[0];
            string username = rows["Username"]?.ToString();
            string pass = rows["Password"]?.ToString();

            string js = $@"
(async function () {{

    if (localStorage.getItem('@access_token')) {{

        location.href =
        'https://seller-v2.tendoo.vn/sales/pos';

        return;
    }}

    function setReactInputValue(selector, value) {{

        const input = document.querySelector(selector);

        if (!input) return;

        const nativeInputValueSetter =
            Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                'value'
            ).set;

        nativeInputValueSetter.call(input, value);

        input.dispatchEvent(new Event('input', {{
            bubbles: true
        }}));

        input.dispatchEvent(new Event('change', {{
            bubbles: true
        }}));
    }}

    setReactInputValue(
        '#phone_login_form_phone_number',
        '{username}'
    );

    await new Promise(r => setTimeout(r, 800));

    setReactInputValue(
        '#phone_login_form_pwd',
        '{pass}'
    );

    await new Promise(r => setTimeout(r, 1000));

    const btn =
        document.querySelector('button[type=submit]');

    if (btn) {{

        btn.click();

        setTimeout(() => {{

            location.href =
            'https://seller-v2.tendoo.vn/sales/pos';

        }}, 1000);
    }}

}})();
";

            await webView21.CoreWebView2.ExecuteScriptAsync(js);
        }

        // Hàm lấy Bearer Token
        private async Task LayBearerToken()
        {
            string jsLayToken = @"
        (function() {
            var token = localStorage.getItem('@access_token');
            if (token) return token;
            return null;
        })();
    ";

            try
            {
                string token = await webView21.CoreWebView2.ExecuteScriptAsync(jsLayToken);
                token = token?.Trim('"');

                if (!string.IsNullOrEmpty(token) && token != "null")
                {
                    bearerToken = token;
                    _content = token;
                    MessageBox.Show($"Đã lấy được Bearer Token!", "Thành công");

                    // Lưu token
                    File.WriteAllText("token.txt", token);

                    // Đóng WebView2 vì không cần nữa
                    webView21.Visible = false;

                    // Gọi API
                    await GoiApiVoiToken();
                }
                else
                {
                    MessageBox.Show("Không lấy được token!", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // Hàm gọi API với Bearer Token
        private async Task GoiApiVoiToken()
        {
            try
            {
                // Cấu hình HttpClient
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                // Ví dụ 1: Lấy thông tin cửa hàng
                // await GetStoreInfo();

                // Ví dụ 2: Lấy danh sách hóa đơn
                await GetInvoices();

                // Ví dụ 3: Tạo hóa đơn mới
                // await CreateInvoice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gọi API: {ex.Message}", "Lỗi");
            }
        }

        // Lấy danh sách hóa đơn
        private async Task GetInvoices()
        {
            try
            {
                // Thay URL bằng API thực tế của Tendo
                string apiUrl = "https://apiv2.tendoo.vn/order/api/v5/get-list-order?business_id=9065ebd6-4c83-4c9c-a6d4-4937e3fda49b&page=2&page_size=10&sort=created_at%20desc&staff_creator_ids=&payment_status=&create_method=&payment_method=&shipping_method=&shipping_status=&schedule_time=&filter_date_type=created_date&date_from=2026-04-30T17%3A00%3A00.000Z&date_to=2026-05-09T16%3A59%3A59.999Z";

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Lấy danh sách hóa đơn thành công!\n{content}", "Thành công");
                    // Xử lý JSON ở đây
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}\n{content}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // Lấy thông tin cửa hàng
        private async Task GetStoreInfo()
        {
            try
            {
                string apiUrl = "https://seller-v2.tendoo.vn/api/store/info";

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Thông tin cửa hàng:\n{content}", "Thành công");
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // Tạo hóa đơn mới (ví dụ)
        private async Task CreateInvoice()
        {
            try
            {
                string apiUrl = "https://seller-v2.tendoo.vn/api/invoices/create";

                // Dữ liệu hóa đơn mẫu
                var invoiceData = new
                {
                    customer_name = "Khách hàng A",
                    customer_phone = "0987654321",
                    items = new[]
                    {
                        new { product_id = 1, quantity = 2, price = 100000 }
                    },
                    total = 200000
                };

                string jsonData = JsonSerializer.Serialize(invoiceData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Tạo hóa đơn thành công!\n{result}", "Thành công");
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}\n{result}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // Hàm POST dữ liệu
        private async Task PostData(string url, object data)
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Thành công: {result}", "Thông báo");
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}\n{result}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // Hàm GET dữ liệu (tổng quát)
        private async Task GetData(string url)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse JSON ở đây
                    var data = JsonSerializer.Deserialize<object>(content);
                    MessageBox.Show($"Dữ liệu: {content}", "Thành công");
                }
                else
                {
                    MessageBox.Show($"Lỗi: {response.StatusCode}", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
    }
}