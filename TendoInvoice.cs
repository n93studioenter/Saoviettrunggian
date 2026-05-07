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
using System.Reflection;
using System.Text;
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
            IWebDriver driver = new ChromeDriver(service, options);
            try
            {
                driver.Navigate().GoToUrl("https://id-v2.tendoo.vn/vi/signin");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
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
        private void TendoInvoice_Load(object sender, EventArgs e)
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
                Phathanhhoadon();
            }
            else
            {
                Addinvoice();
            }
        }
    }
}
