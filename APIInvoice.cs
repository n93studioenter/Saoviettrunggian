using Microsoft.Playwright;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

                    var json = @"{
                ""username"": ""3502412669"",
                ""password"": ""Tdt@12345678"",
                ""rememberMe"": false,
                ""captcha"": """"
            }";

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);

                    // *** CÁCH LẤY COOKIE ***
                    // Lấy tất cả cookies từ response
                    var cookies = handler.CookieContainer.GetCookies(new Uri(url));

                    foreach (System.Net.Cookie cookie in cookies)
                    {
                        MessageBox.Show($"Cookie: {cookie.Name} = {cookie.Value}");

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

                    MessageBox.Show(response.StatusCode.ToString());
                    MessageBox.Show(result);
                }
            }
        }
        private async  void APIInvoice_Load(object sender, EventArgs e)
        {
            //await Login();
            await LoginWithSelenium();   // Gọi hàm login mới
        }
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
                   // allCookies[cookie.Name] = cookie.Value;
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
        private Dictionary<string, string> allCookies = new Dictionary<string, string>();   // Lưu tất cả cookie
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
                var searchUrl = "https://vinvoice.viettel.vn/api/cluster5/services/einvoiceapplication/api/customer/search?page=0&size=10&customerCode.contains=&email.contains=&idNo.contains=&name.contains=&taxCode.contains=35024953&phoneNumber.contains=&unitName.contains=&sort=createdDate%2Cdesc";

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
            if (allCookies.Count == 0)
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
                    string cookieHeader = string.Join("; ", allCookies.Select(c => $"{c.Key}={c.Value}"));

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
    }
}
