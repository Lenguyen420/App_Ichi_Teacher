using kido_teacher_app.Forms.Main.Page.BaiThi.Lop1;
using kido_teacher_app.Model;
using kido_teacher_app.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi
{
    public partial class UC_BaiThi_Main : UserControl
    {
        //private Button selectedTab;
        //private readonly List<Button> classTabs = new();

        private ComboBox cbClass;
        private Button btnSearch;
        private TextBox txtFilter;
        private Panel panelFilter;

        private List<ClassDto> allClasses = new();

        public UC_BaiThi_Main()
        {
            InitializeComponent();
            _ = LoadClassesAsync();

            btnSearch.Click += BtnSearch_Click;
            txtFilter.TextChanged += TxtFilter_TextChanged;
        }

        private async Task LoadClassesAsync()
        {
            allClasses = await ClassService.GetAllAsync();

            cbClass.DataSource = allClasses;
            cbClass.DisplayMember = "name";
            cbClass.ValueMember = "id";
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if (cbClass.SelectedItem is not ClassDto cls) return;

            panelContent.Controls.Clear();

            var uc = new UC_Lop1(panelContent, cls)
            {
                Dock = DockStyle.Fill
            };

            panelContent.Controls.Add(uc);
        }


        private void TxtFilter_TextChanged(object sender, EventArgs e)
        {
            string key = txtFilter.Text.Trim().ToLower();

            var filtered = string.IsNullOrEmpty(key)
                ? allClasses
                : allClasses.FindAll(c => c.name.ToLower().Contains(key));

            cbClass.DataSource = null;
            cbClass.DataSource = filtered;
            cbClass.DisplayMember = "name";
            cbClass.ValueMember = "id";
        }


        // ================= LOAD TAB LỚP =================
        //private async Task LoadClassTabsAsync()
        //{
        //    panelTabs.Controls.Clear();
        //    classTabs.Clear();
        //    selectedTab = null;

        //    var classes = await ClassService.GetAllAsync();
        //    int x = 10;

        //    foreach (var cls in classes)
        //    {
        //        var btn = new Button
        //        {
        //            Text = cls.name,
        //            Width = 140,
        //            Height = 35,
        //            Location = new Point(x, 7),
        //            FlatStyle = FlatStyle.Flat,
        //            BackColor = Color.White,
        //            Cursor = Cursors.Hand,
        //            Tag = cls
        //        };

        //        btn.FlatAppearance.BorderColor = Color.Gray;
        //        btn.Click += (s, e) => SelectClassTab(btn);

        //        panelTabs.Controls.Add(btn);
        //        classTabs.Add(btn);
        //        x += btn.Width + 8;

        //        if (selectedTab == null)
        //        {
        //            SelectClassTab(btn);
        //        }
        //    }
        //}

        //private void SelectClassTab(Button btn)
        //{
        //    if (selectedTab != null)
        //        selectedTab.BackColor = Color.White;

        //    selectedTab = btn;
        //    selectedTab.BackColor = Color.FromArgb(150, 220, 80);

        //    var cls = btn.Tag as ClassDto;
        //    if (cls == null) return;

        //    panelContent.Controls.Clear();

        //    var uc = new UC_Lop1(panelContent, cls)
        //    {
        //        Dock = DockStyle.Fill
        //    };

        //    panelContent.Controls.Add(uc);
        //}
    }
}
