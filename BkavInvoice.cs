using DevExpress.XtraEditors;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaovietTax
{
    public partial class BkavInvoice : Form
    {
        public BkavInvoice()
        {
            InitializeComponent();
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
        public class Product
        {
            public string Name { get; set; }
            public string Unit { get; set; }
            public double Qty { get; set; }
            public double Price { get; set; }
        }
        private async void Addinvoice()
        {
            string qrq = "SELECT * FROM tbInvoiceInfo";
            var dtInvoiceInfo = ExecuteQuery(qrq, null);
            var rows = dtInvoiceInfo.Rows[0];

            string username = rows["Username"]?.ToString();
            string password = rows["Password"]?.ToString();

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


            var getsplit= _content.Split('_');
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
            DateTime date = DateTime.ParseExact(kq2.Rows[0]["NgayCT"].ToString(), "dd/MM/yy hh:mm:ss tt", CultureInfo.InvariantCulture);
            string formattedDate = date.ToString("dd/MM/yyyy");

            string sql = "SELECT * FROM ChungTu WHERE MaCT = ?";
            var param = new OleDbParameter[]
            {
                new OleDbParameter("?", kq2.Rows[0]["MaCT"])
            };
            List<Product> products = new List<Product>();
            //Lấy data chung tu
            var kq3 = ExecuteQuery(sql, param);
            var sqlkh = "SELECT * FROM KhachHang WHERE MaSo = ?";
            parameterss = new OleDbParameter[]
          {
    new OleDbParameter("?",  kq2.Rows[0]["MaKhachHang"]),
          };
            var dtKhachhang = ExecuteQuery(sqlkh, parameterss);
            foreach (DataRow row in kq3.Rows)
            {
                if (row["MaVattu"].ToString().Trim() == "0")
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
                Product product = new Product
                {
                    Name = Helpers.ConvertVniToUnicode(kqHangHoa.Rows[0]["TenVattu"].ToString()),
                    Unit = Helpers.ConvertVniToUnicode(kqHangHoa.Rows[0]["DonVi"].ToString()),
                    Qty = soluong,
                    Price = soluong == 0 ? 0 : Math.Round(sops / soluong)
                };
                products.Add(product);
            }
             
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();

            // 🔥 Ẩn trình duyệt (không hiện UI nhưng vẫn chạy thật)
            //options.AddArgument("--window-position=-32000,-32000");
           // options.AddArgument("--window-size=1920,1080");

            IWebDriver driver = new ChromeDriver(service, options);

            try
            {
                driver.Navigate().GoToUrl("https://van.ehoadon.vn/");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

                // ===== USERNAME =====
                var txtUser = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("txtUserName"));
                        return el.Displayed ? el : null;
                    }
                    catch { return null; }
                });

                txtUser.Clear();
                txtUser.SendKeys(username); // 👉 username của bạn

                // ===== PASSWORD =====
                var txtPass = driver.FindElement(By.Id("txtPassword"));
                txtPass.Clear();
                txtPass.SendKeys(password);

                // ===== CLICK LOGIN =====
                var btnLogin = driver.FindElement(By.Id("btnLogin"));
                btnLogin.Click();

                // ===== CHỜ LOGIN THÀNH CÔNG =====
                wait.Until(d => d.Url.Contains("QLHD"));

                Console.WriteLine("✅ Login thành công");

                wait.Until(d => d.Url.Contains("QLHD"));

                Console.WriteLine("✅ Đã vào QLHD");

                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

                var btnAddNew = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.Id("body_btnAddNew"));
                        return (el.Displayed && el.Enabled) ? el : null;
                    }
                    catch { return null; }
                });

                btnAddNew.Click();
                // Tìm tất cả popup
                // Tìm popup (có 3 popup, lấy popup đang hiển thị)
                var popups = driver.FindElements(By.ClassName("pop"));
                var dialog = popups.FirstOrDefault(p => p.Displayed);

                // Switch vào iframe
                var iframe = dialog.FindElement(By.Id("framedialogInvoiceNewEdit"));
                driver.SwitchTo().Frame(iframe);
                 
                // Tìm input và gán giá trị
                var input = wait.Until(d => d.FindElement(By.Id("txtBuyerSearch")));
                input.Clear();
                Thread.Sleep(800);
                input.SendKeys(dtKhachhang.Rows[0]["MST"].ToString());
                input.Clear();
                Thread.Sleep(800);
                input.SendKeys(dtKhachhang.Rows[0]["MST"].ToString());
                input.Clear();
                input.SendKeys(dtKhachhang.Rows[0]["MST"].ToString());
                Thread.Sleep(800);
                // Tìm input và nhập ngày mới
              
                // Chờ gợi ý xuất hiện
                var suggestion = wait.Until(d => d.FindElement(By.CssSelector("#eac-container-txtBuyerSearch .eac-item")));

                // Click bằng JavaScript
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", suggestion);
                var txtInvoiceDate = wait.Until(d => d.FindElement(By.Id("txtInvoiceDate")));
                txtInvoiceDate.Clear();
                txtInvoiceDate.SendKeys(formattedDate);

                // 1. Đã ở trong dialog chính (iframe framedialogInvoiceNewEdit)
                // Tìm và click nút "Thêm chi tiết"
                var btnAddDetail = wait.Until(d => d.FindElement(By.Id("MasterPlaceHolderBlank_btnAddDetails")));
                btnAddDetail.Click();

                // 2. Chờ popup chi tiết xuất hiện
                wait.Until(d => d.FindElement(By.CssSelector("div.ui-dialog[aria-describedby='InvoiceDetailsNewEdit']")).Displayed);

                // 3. Switch vào iframe của popup chi tiết
                var detailIframe = wait.Until(d => d.FindElement(By.Id("frameInvoiceDetailsNewEdit")));
                driver.SwitchTo().Frame(detailIframe);

                // Danh sách sản phẩm cần thêm
                

                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];

                    // Nhập tên hàng hóa (có thể có autocomplete)
                    var txtItemName = wait.Until(d => d.FindElement(By.Id("txtItemName")));
                    txtItemName.Clear();
                    txtItemName.SendKeys(product.Name);

                    // Chờ autocomplete nếu có (tùy chọn)
                    Thread.Sleep(500);
                    // Nếu có gợi ý thì chọn
                    var suggestions = driver.FindElements(By.CssSelector("#eac-container-txtItemName .eac-item"));
                    if (suggestions.Count > 0)
                    {
                        suggestions[0].Click();
                    }

                    // Nhập ĐVT
                    var txtUnitName = driver.FindElement(By.Id("txtUnitName"));
                    txtUnitName.Clear();
                    txtUnitName.SendKeys(product.Unit);

                    // Nhập số lượng
                    var txtQty = driver.FindElement(By.Id("txtQty"));
                    txtQty.Clear();
                    txtQty.SendKeys(product.Qty.ToString());

                    // Nhập đơn giá
                    var txtPrice = driver.FindElement(By.Id("txtPrice"));
                    txtPrice.Clear();
                    txtPrice.SendKeys(product.Price.ToString());

                    var ddlTaxRate = driver.FindElement(By.Id("ddlTaxRate"));
                    ddlTaxRate.SendKeys($"{kq2.Rows[0]["TyLe"].ToString()}%"); // Hoặc dùng SelectElement
                    // Chờ thành tiền tự tính (hoặc tab để kích hoạt)
                    txtPrice.SendKeys(OpenQA.Selenium.Keys.Tab);
                    Thread.Sleep(200);

                    // Lưu sản phẩm
                    if (i < products.Count - 1)
                    {
                        // Nếu còn sản phẩm tiếp theo: dùng "Ghi lại & Nhập tiếp"
                        var btnAddAndContinue = driver.FindElement(By.Id("btnAddAndContinue"));
                        btnAddAndContinue.Click();

                        // Chờ form reset (các field trống)
                        wait.Until(d => driver.FindElement(By.Id("txtItemName")).GetAttribute("value") == "");
                    }
                    else
                    {
                        // Sản phẩm cuối cùng: dùng "Ghi lại"
                        var btnAdd = driver.FindElement(By.Id("btnAdd"));
                        btnAdd.Click(); 
                    }

                    Console.WriteLine($"Đã thêm sản phẩm {i + 1}/{products.Count}: {product.Name}");
                }

                driver.SwitchTo().DefaultContent();

                // Bây giờ mới tìm iframe
                var mainIframe = driver.FindElement(By.Id("framedialogInvoiceNewEdit"));
                driver.SwitchTo().Frame(mainIframe);

                // 2. Click nút Ghi lại
                var saveBtn = driver.FindElement(By.Id("btnSave"));
                saveBtn.Click();

                Console.WriteLine("Đã click Ghi lại - Lưu hóa đơn"); 
                // 2. Chờ về trang danh sách 
                // 3. Chờ bảng load xong
                Thread.Sleep(1000); // Hoặc wait cụ thể

                driver.SwitchTo().DefaultContent();

                // 2. Chờ bảng danh sách load lại
                wait.Until(d => d.FindElement(By.Id("gg-table-1")).Displayed);

                // 3. Chờ thêm 1 chút để dữ liệu refresh
                Thread.Sleep(1000);

                // 4. Lấy dòng đầu tiên
                var firstRow = driver.FindElement(By.CssSelector("#gg-table-1 tbody tr:first-child"));
                // 5. Lấy GUID từ checkbox
                var checkbox = firstRow.FindElement(By.CssSelector(".CheckOne input"));
                string invoiceGuid = checkbox.GetAttribute("data-invoiceguid");

                Console.WriteLine($"Hóa đơn vừa tạo có GUID: {invoiceGuid}");


              
                driver.Quit();  
                this.Close();   

            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi: " + ex.Message);
            }
        }
        string password, connectionString;
        string _content;
        private async void BkavInvoice_Load(object sender, EventArgs e)
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
            //Đọc file txt
            string filePath = Path.Combine(rootDirectory, "Hoadon", "invoice.txt");
            _content = File.ReadAllText(filePath);
            if (_content.Contains("PH_"))
            {
                MessageBox.Show("Phát hành hoá đon thành công!");
            }
            else
            {
                Addinvoice();
            }

        }
    }
}
