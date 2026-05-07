using System;
using System.Windows.Forms;
using kido_teacher_app.Services;

namespace kido_teacher_app.Forms
{
    public partial class Form_CacheDiagnostics : Form
    {
        public Form_CacheDiagnostics()
        {
            this.Text = "Cache Diagnostics";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Title
            var lblTitle = new Label
            {
                Text = "Cache Database Diagnostic",
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(20, 20)
            };
            this.Controls.Add(lblTitle);

            // Report text box
            var txtReport = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new System.Drawing.Font("Consolas", 10),
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(750, 450),
                Name = "txtReport"
            };
            this.Controls.Add(txtReport);

            // Buttons
            var btnScan = new Button
            {
                Text = "Scan Database",
                Location = new System.Drawing.Point(20, 520),
                Size = new System.Drawing.Size(120, 32),
                Name = "btnScan"
            };
            btnScan.Click += (s, e) => RunScan(txtReport);
            this.Controls.Add(btnScan);

            var btnFix = new Button
            {
                Text = "Backup & Reset",
                Location = new System.Drawing.Point(150, 520),
                Size = new System.Drawing.Size(120, 32),
                Name = "btnFix",
                Enabled = false
            };
            btnFix.Click += (s, e) => FixDatabase(txtReport, btnFix);
            this.Controls.Add(btnFix);

            var btnClose = new Button
            {
                Text = "Close",
                Location = new System.Drawing.Point(650, 520),
                Size = new System.Drawing.Size(120, 32),
                Name = "btnClose"
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            // Status label
            var lblStatus = new Label
            {
                Text = "Ready",
                AutoSize = true,
                Location = new System.Drawing.Point(280, 530),
                ForeColor = System.Drawing.Color.Gray,
                Name = "lblStatus"
            };
            this.Controls.Add(lblStatus);
        }

        private void RunScan(TextBox txtReport)
        {
            try
            {
                var lblStatus = this.Controls["lblStatus"] as Label;
                if (lblStatus != null)
                    lblStatus.Text = "Scanning...";

                var report = CacheDiagnosticService.ScanDatabase();
                var reportText = CacheDiagnosticService.GetReportText(report);

                txtReport.Text = reportText;

                if (lblStatus != null)
                {
                    if (report.CorruptedEntries.Count > 0)
                    {
                        lblStatus.Text = $"⚠ Found {report.CorruptedEntries.Count} corrupted entries";
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        
                        var btnFix = this.Controls["btnFix"] as Button;
                        if (btnFix != null)
                            btnFix.Enabled = true;
                    }
                    else
                    {
                        lblStatus.Text = "✓ Database is healthy";
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning database:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FixDatabase(TextBox txtReport, Button btnFix)
        {
            var result = MessageBox.Show(
                "This will backup the current database and reset it.\n\nContinue?",
                "Confirm Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                var lblStatus = this.Controls["lblStatus"] as Label;
                if (lblStatus != null)
                    lblStatus.Text = "Resetting...";

                if (CacheDiagnosticService.BackupAndResetDatabase())
                {
                    txtReport.AppendText("\n\n✓ Database backed up and reset successfully\n");
                    txtReport.AppendText("App will reinitialize cache on next start\n");

                    MessageBox.Show(
                        "Database reset successfully!\n\nPlease restart the application.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    btnFix.Enabled = false;
                    if (lblStatus != null)
                    {
                        lblStatus.Text = "✓ Reset completed";
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                    }
                }
                else
                {
                    throw new Exception("Failed to backup/reset database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting database:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
