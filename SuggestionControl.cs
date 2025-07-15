using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SaovietTax.frmMain;

namespace SaovietTax
{
    public partial class SuggestionControl : UserControl
    {
        public event EventHandler<string> ItemSelected;

        public List<HeThongTK> Suggestions
        {
            set
            {
                listBox1.Items.Clear();
                listView1.Items.Clear();
                foreach (var item in value)
                {
                    listBox1.Items.Add(item.SoHieu + "-" + Helpers.ConvertVniToUnicode(item.Ten)); // Hiển thị tên
                    listView1.Items.Add(new ListViewItem(new[] { item.SoHieu, Helpers.ConvertVniToUnicode(item.Ten) }));
                }
            }
        }

        public SuggestionControl()
        {
            InitializeComponent();
            listBox1.DoubleClick += ListBox1_DoubleClick;

            listView1.View = View.Details;
            listView1.Columns.Add("Số hiệu", 100);
            listView1.Columns.Add("Tên", 350);


            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.OwnerDraw = true;
        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                // Lấy SoHieu từ mục đã chọn
                string selectedItem = listBox1.SelectedItem.ToString();
                string soHieu = selectedItem.Split('-')[0].Trim(); // Tách SoHieu ra

                ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
                Hide();
            }
        }

        public void UpdateSuggestions(List<HeThongTK> newSuggestions)
        {
            Suggestions = newSuggestions;

            // Hiển thị UserControl nếu có gợi ý
            if (listBox1.Items.Count > 0)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView1.SelectedItems[0];

                // Lấy dữ liệu từ hàng đã chọn
                string soHieu = selectedItem.SubItems[0].Text;
                string tenVatTu = selectedItem.SubItems[1].Text; 
                ItemSelected?.Invoke(this, soHieu); // Gửi SoHieu
                Hide();
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

            // Vẽ văn bản
            e.DrawText(TextFormatFlags.Left);
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {

            e.DrawDefault = true; // Vẽ mặc định để hiển thị văn bản
        }
    }

}