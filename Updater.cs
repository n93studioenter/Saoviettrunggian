using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace SaovietTax
{
    public partial class Updater : DevExpress.XtraEditors.XtraForm
    {
        public Updater()
        {
            InitializeComponent();
        }

        private async void Updater_Load(object sender, EventArgs e)
        {
            // Hiển thị trạng thái loading lên giao diện 
            this.Refresh();

            try
            {
                // Chạy quá trình cập nhật trong background
                string duongDanVietStar = await Task.Run(() => ThucHienCapNhat());

                // Kiểm tra kết quả
                if (!string.IsNullOrEmpty(duongDanVietStar))
                {
                    // Thông báo thành công
                    //XtraMessageBox.Show(
                    //    "Cập nhật thành công!",
                    //    "Thông báo",
                    //    MessageBoxButtons.OK,
                    //    MessageBoxIcon.Information
                    //);

                    // Chạy ứng dụng VietStar mới
                    ProcessStartInfo thongTinMo = new ProcessStartInfo
                    {
                        FileName = duongDanVietStar,
                        UseShellExecute = true
                    };
                    Process.Start(thongTinMo);
                }
                else
                {
                    XtraMessageBox.Show(
                        "Không tìm thấy file cập nhật!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception loi)
            {
                XtraMessageBox.Show(
                    $"Lỗi cập nhật: {loi.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                // Đóng form updater
                this.Close();
            }
        }

        private string ThucHienCapNhat()
        {
            // ================================================================
            // BƯỚC 1: Lấy version từ file trên server
            // ================================================================
            string version = "";
            string duongDanVersionServer = @"\\192.168.1.90\Ke toan 2025 New\1 Copi vao dung 1\Tools\version.txt";

            if (File.Exists(duongDanVersionServer))
            {
                version = File.ReadAllText(duongDanVersionServer).Trim();
                progressPanel1.Caption = $"Đang tải phiên bản {version}, vui lòng chờ";
                System.Diagnostics.Debug.WriteLine("Version: " + version);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Không tìm thấy file version.txt trên server");
            }

            // ================================================================
            // BƯỚC 2: Lấy đường dẫn thư mục hiện tại của ứng dụng
            // ================================================================
            string duongDanThuMucExe = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string duongDanThuMucProject = Directory.GetParent(duongDanThuMucExe).Parent.FullName;

            // ================================================================
            // BƯỚC 3: Ghi version vào thư mục Hoadon
            // ================================================================
            string duongDanThuMucHoaDon = Path.Combine(duongDanThuMucProject, "Hoadon");
            string duongDanVersionHoaDon = Path.Combine(duongDanThuMucHoaDon, "version.txt");

            if (File.Exists(duongDanVersionHoaDon))
            {
                File.WriteAllText(duongDanVersionHoaDon, version);
                System.Diagnostics.Debug.WriteLine("Đã ghi version vào thư mục Hoadon");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Không tìm thấy file version.txt trong thư mục Hoadon");
            }

            // ================================================================
            // BƯỚC 4: Copy các file cần thiết từ server về máy
            // ================================================================
            string duongDanRoot = @"\\192.168.1.90\Ke toan 2025 New\1 Copi vao dung 1";
            string duongDanThuMucDich = duongDanThuMucExe;

            string[] danhSachFileCanCopy =
            {
                "SaovietTax.exe.Config",
                "SaovietTax.application",
                "SaovietTax.exe.manifest",
                "SaovietTax.exe",
                "SaovietTax.pdb"
            };

            try
            {
                // Tìm tất cả thư mục Debug nằm trong thư mục Tools
                var danhSachThuMucDebug = Directory.GetDirectories(
                    duongDanRoot,
                    "Debug",
                    SearchOption.AllDirectories
                ).Where(duongDan => duongDan.EndsWith(
                    Path.Combine("Tools", "Debug"),
                    StringComparison.OrdinalIgnoreCase
                ));

                // Tạo thư mục đích nếu chưa tồn tại
                Directory.CreateDirectory(duongDanThuMucDich);

                // Duyệt qua từng thư mục Debug tìm được
                foreach (var duongDanThuMucDebug in danhSachThuMucDebug)
                {
                    System.Diagnostics.Debug.WriteLine("Tìm thấy thư mục Debug: " + duongDanThuMucDebug);

                    // Copy từng file
                    foreach (var tenFile in danhSachFileCanCopy)
                    {
                        string duongDanFileNguon = Path.Combine(duongDanThuMucDebug, tenFile);

                        if (File.Exists(duongDanFileNguon))
                        {
                            string duongDanFileDich = Path.Combine(duongDanThuMucDich, tenFile);
                            File.Copy(duongDanFileNguon, duongDanFileDich, true);
                            System.Diagnostics.Debug.WriteLine("Đã copy: " + tenFile);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Không tìm thấy file: " + tenFile);
                        }
                    }

                    // Chỉ copy ở thư mục Debug đầu tiên tìm được
                    break;
                }

                System.Diagnostics.Debug.WriteLine("Hoàn thành copy các file chính.");
            }
            catch (Exception loi)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi copy file: " + loi.Message);
            }

            // ================================================================
            // BƯỚC 5: Copy file VietStar.exe mới nhất
            // ================================================================
            string duongDanVietStarDaCopy = "";

            try
            {
                // Tìm file VietStar.exe mới nhất
                var fileVietStarMoiNhat = new DirectoryInfo(duongDanRoot)
                    .GetFiles("*.exe", SearchOption.AllDirectories)
                    .Where(file => file.Name.IndexOf("VietStar", StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderByDescending(file => file.LastWriteTime)
                    .FirstOrDefault();

                if (fileVietStarMoiNhat != null)
                {
                    // Copy file VietStar vào thư mục project
                    string duongDanFileDich = Path.Combine(duongDanThuMucProject, fileVietStarMoiNhat.Name);
                    File.Copy(fileVietStarMoiNhat.FullName, duongDanFileDich, true);
                    duongDanVietStarDaCopy = duongDanFileDich;

                    System.Diagnostics.Debug.WriteLine("Đã copy VietStar: " + duongDanFileDich);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Không tìm thấy file VietStar.exe");
                }
            }
            catch (Exception loi)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi copy VietStar: " + loi.Message);
            }

            // Trả về đường dẫn của file VietStar đã copy
            return duongDanVietStarDaCopy;
        }
    }
}