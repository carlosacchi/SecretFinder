using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SecretsFinder.Utils;

namespace SecretsFinder.Forms
{
    public partial class AboutForm : Form
    {
        private Label TitleLabel;
        private Label VersionLabel;
        private Label DescriptionLabel;
        private Label CreditsLabel;
        private Label DisclaimerLabel;
        private LinkLabel GitHubLinkLabel;
        private Button CloseButton;

        public AboutForm()
        {
            InitializeComponent();
            FormStyle.ApplyStyle(this, Main.settings.use_npp_styling);
        }

        private void InitializeComponent()
        {
            this.TitleLabel = new Label();
            this.VersionLabel = new Label();
            this.DescriptionLabel = new Label();
            this.CreditsLabel = new Label();
            this.DisclaimerLabel = new Label();
            this.GitHubLinkLabel = new LinkLabel();
            this.CloseButton = new Button();

            this.SuspendLayout();

            // TitleLabel
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.TitleLabel.Location = new Point(20, 20);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Text = "SecretsFinder";

            // VersionLabel
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new Point(22, 50);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Text = "Version 1.0.12";
            this.VersionLabel.ForeColor = Color.Gray;

            // DescriptionLabel
            this.DescriptionLabel.Location = new Point(22, 80);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new Size(300, 80);
            this.DescriptionLabel.Text = "A Notepad++ plugin for detecting secrets, API keys, tokens, and other sensitive information in your documents.\n\n" +
                "Helps prevent accidental exposure of credentials in backups, shared folders, and version control.";

            // CreditsLabel
            this.CreditsLabel.AutoSize = true;
            this.CreditsLabel.Location = new Point(22, 170);
            this.CreditsLabel.Name = "CreditsLabel";
            this.CreditsLabel.Text = "Credits: Carlos Sacchi";

            // DisclaimerLabel
            this.DisclaimerLabel.Location = new Point(22, 190);
            this.DisclaimerLabel.Name = "DisclaimerLabel";
            this.DisclaimerLabel.Size = new Size(300, 40);
            this.DisclaimerLabel.Text = "Disclaimer: automated scans help but cannot guarantee 100% detection. Combine with manual review.";
            this.DisclaimerLabel.ForeColor = Color.Gray;

            // GitHubLinkLabel
            this.GitHubLinkLabel.AutoSize = true;
            this.GitHubLinkLabel.Location = new Point(22, 235);
            this.GitHubLinkLabel.Name = "GitHubLinkLabel";
            this.GitHubLinkLabel.Text = "GitHub Repository";
            this.GitHubLinkLabel.LinkClicked += GitHubLinkLabel_LinkClicked;

            // CloseButton
            this.CloseButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.CloseButton.Location = new Point(250, 265);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new Size(75, 25);
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += CloseButton_Click;

            // AboutForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(340, 300);
            this.Controls.Add(this.TitleLabel);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.DescriptionLabel);
            this.Controls.Add(this.CreditsLabel);
            this.Controls.Add(this.DisclaimerLabel);
            this.Controls.Add(this.GitHubLinkLabel);
            this.Controls.Add(this.CloseButton);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "About SecretsFinder";

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void GitHubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var ps = new ProcessStartInfo(Main.PluginRepository)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open URL:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}
