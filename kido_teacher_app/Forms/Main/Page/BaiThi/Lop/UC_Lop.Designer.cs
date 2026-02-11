using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    partial class UC_Lop
    {
        private FlowLayoutPanel flowContent;

        private void InitializeComponent()
        {
            flowContent = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // flowContent
            // 
            flowContent.AutoScroll = true;
            flowContent.BackColor = Color.White;
            flowContent.Dock = DockStyle.Fill;
            flowContent.Location = new Point(0, 0);
            flowContent.Name = "flowContent";
            flowContent.Padding = new Padding(20);
            flowContent.Size = new Size(1390, 717);
            flowContent.TabIndex = 0;
            // 
            // UC_Lop1
            // 
            Controls.Add(flowContent);
            Name = "UC_Lop1";
            Size = new Size(1390, 717);
            ResumeLayout(false);
        }
    }
}
