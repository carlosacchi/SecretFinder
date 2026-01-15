using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SecretsFinder.Core;

namespace SecretsFinder.Forms
{
    public partial class ResultsPanel : Form
    {
        private List<SecretMatch> _matches = new List<SecretMatch>();
        private ListView ResultsListView;
        private ColumnHeader FileColumn;
        private ColumnHeader LineColumn;
        private ColumnHeader TypeColumn;
        private ColumnHeader ValueColumn;
        private ColumnHeader SeverityColumn;
        private Label CountLabel;
        private Button ClearAllButton;
        private Button ExportButton;
        private ContextMenuStrip ResultsContextMenu;
        private ToolStripMenuItem CopyValueMenuItem;
        private ToolStripMenuItem CopyObscuredMenuItem;
        private ToolStripMenuItem DismissMenuItem;

        public ResultsPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.ResultsListView = new ListView();
            this.FileColumn = new ColumnHeader();
            this.LineColumn = new ColumnHeader();
            this.TypeColumn = new ColumnHeader();
            this.ValueColumn = new ColumnHeader();
            this.SeverityColumn = new ColumnHeader();
            this.CountLabel = new Label();
            this.ClearAllButton = new Button();
            this.ExportButton = new Button();
            this.ResultsContextMenu = new ContextMenuStrip();
            this.CopyValueMenuItem = new ToolStripMenuItem();
            this.CopyObscuredMenuItem = new ToolStripMenuItem();
            this.DismissMenuItem = new ToolStripMenuItem();

            this.SuspendLayout();

            // ResultsListView
            this.ResultsListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                                          AnchorStyles.Left | AnchorStyles.Right;
            this.ResultsListView.Columns.AddRange(new ColumnHeader[] {
                this.FileColumn, this.LineColumn, this.TypeColumn,
                this.ValueColumn, this.SeverityColumn
            });
            this.ResultsListView.FullRowSelect = true;
            this.ResultsListView.GridLines = true;
            this.ResultsListView.Location = new Point(5, 5);
            this.ResultsListView.Name = "ResultsListView";
            this.ResultsListView.Size = new Size(590, 180);
            this.ResultsListView.TabIndex = 0;
            this.ResultsListView.UseCompatibleStateImageBehavior = false;
            this.ResultsListView.View = View.Details;
            this.ResultsListView.DoubleClick += ResultsListView_DoubleClick;
            this.ResultsListView.MouseUp += ResultsListView_MouseUp;

            // Column Headers
            this.FileColumn.Text = "File";
            this.FileColumn.Width = 260;
            this.LineColumn.Text = "Line";
            this.LineColumn.Width = 50;
            this.TypeColumn.Text = "Type";
            this.TypeColumn.Width = 130;
            this.ValueColumn.Text = "Value (Obscured)";
            this.ValueColumn.Width = 180;
            this.SeverityColumn.Text = "Severity";
            this.SeverityColumn.Width = 70;

            // CountLabel
            this.CountLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.CountLabel.AutoSize = true;
            this.CountLabel.Location = new Point(5, 192);
            this.CountLabel.Name = "CountLabel";
            this.CountLabel.Text = "Found: 0 secret(s)";

            // ClearAllButton
            this.ClearAllButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.ClearAllButton.Location = new Point(430, 188);
            this.ClearAllButton.Name = "ClearAllButton";
            this.ClearAllButton.Size = new Size(75, 23);
            this.ClearAllButton.Text = "Clear All";
            this.ClearAllButton.UseVisualStyleBackColor = true;
            this.ClearAllButton.Click += ClearAllButton_Click;

            // ExportButton
            this.ExportButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.ExportButton.Location = new Point(510, 188);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new Size(75, 23);
            this.ExportButton.Text = "Export...";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += ExportButton_Click;

            // ResultsContextMenu
            this.ResultsContextMenu.Items.AddRange(new ToolStripItem[] {
                this.CopyValueMenuItem,
                this.CopyObscuredMenuItem,
                new ToolStripSeparator(),
                this.DismissMenuItem
            });

            // CopyValueMenuItem
            this.CopyValueMenuItem.Text = "Copy Full Value (Caution!)";
            this.CopyValueMenuItem.Click += CopyValueMenuItem_Click;

            // CopyObscuredMenuItem
            this.CopyObscuredMenuItem.Text = "Copy Obscured Value";
            this.CopyObscuredMenuItem.Click += CopyObscuredMenuItem_Click;

            // DismissMenuItem
            this.DismissMenuItem.Text = "Dismiss";
            this.DismissMenuItem.Click += DismissMenuItem_Click;

            // ResultsPanel
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(600, 215);
            this.Controls.Add(this.ResultsListView);
            this.Controls.Add(this.CountLabel);
            this.Controls.Add(this.ClearAllButton);
            this.Controls.Add(this.ExportButton);
            this.Name = "ResultsPanel";
            this.Text = "SecretsFinder Results";

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public void UpdateResults(List<SecretMatch> matches)
        {
            _matches = matches ?? new List<SecretMatch>();
            RefreshListView();
        }

        private void RefreshListView()
        {
            ResultsListView.Items.Clear();

            var visibleMatches = _matches.Where(m => !m.IsDismissed).ToList();

            foreach (var match in visibleMatches)
            {
                var item = new ListViewItem(new[]
                {
                    match.DisplayPath,
                    match.LineNumber.ToString(),
                    match.Pattern?.Name ?? "Unknown",
                    match.ObscuredValue,
                    match.Pattern?.Severity.ToString() ?? "Unknown"
                });
                item.Tag = match;
                item.ToolTipText = match.DisplayPath;

                // Adjust severity display upward and recolor: Low->Medium (yellow), Medium->High (orange), High->Critical (red), Critical->Critical (purple)
                if (match.Pattern != null)
                {
                    var severity = match.Pattern.Severity;
                    switch (severity)
                    {
                        case SecretSeverity.Low:
                            item.SubItems[4].Text = SecretSeverity.Medium.ToString();
                            item.BackColor = Color.FromArgb(255, 255, 200); // yellow
                            break;
                        case SecretSeverity.Medium:
                            item.SubItems[4].Text = SecretSeverity.High.ToString();
                            item.BackColor = Color.FromArgb(255, 200, 120); // orange
                            break;
                        case SecretSeverity.High:
                            item.SubItems[4].Text = SecretSeverity.Critical.ToString();
                            item.BackColor = Color.FromArgb(255, 160, 160); // red-ish
                            break;
                        case SecretSeverity.Critical:
                            item.BackColor = Color.FromArgb(200, 160, 255); // purple
                            break;
                    }
                }

                ResultsListView.Items.Add(item);
            }

            // Update count label
            int totalCount = visibleMatches.Count;
            int fileCount = visibleMatches.Select(m => m.FilePath).Distinct().Count();
            CountLabel.Text = $"Found: {totalCount} secret(s) in {fileCount} file(s)";
        }

        private void ResultsListView_DoubleClick(object sender, EventArgs e)
        {
            if (ResultsListView.SelectedItems.Count > 0)
            {
                var match = ResultsListView.SelectedItems[0].Tag as SecretMatch;
                if (match != null)
                {
                    Main.NavigateToMatch(match);
                }
            }
        }

        private void ResultsListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && ResultsListView.SelectedItems.Count > 0)
            {
                ResultsContextMenu.Show(ResultsListView, e.Location);
            }
        }

        private void CopyValueMenuItem_Click(object sender, EventArgs e)
        {
            if (ResultsListView.SelectedItems.Count > 0)
            {
                var match = ResultsListView.SelectedItems[0].Tag as SecretMatch;
                if (match != null)
                {
                    var result = MessageBox.Show(
                        "Are you sure you want to copy the full secret value to clipboard?\n\n" +
                        "This could be a security risk if your clipboard is monitored.",
                        "Security Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            Clipboard.SetText(match.MatchedValue);
                        }
                        catch
                        {
                            MessageBox.Show("Failed to copy to clipboard.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void CopyObscuredMenuItem_Click(object sender, EventArgs e)
        {
            if (ResultsListView.SelectedItems.Count > 0)
            {
                var match = ResultsListView.SelectedItems[0].Tag as SecretMatch;
                if (match != null)
                {
                    try
                    {
                        Clipboard.SetText(match.ObscuredValue);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to copy to clipboard.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DismissMenuItem_Click(object sender, EventArgs e)
        {
            if (ResultsListView.SelectedItems.Count > 0)
            {
                var match = ResultsListView.SelectedItems[0].Tag as SecretMatch;
                if (match != null)
                {
                    match.IsDismissed = true;
                    RefreshListView();
                    Main.RefreshHighlights();
                }
            }
        }

        private void ClearAllButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Clear all results? This will also remove highlights from documents.",
                "Confirm Clear",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _matches.Clear();
                RefreshListView();
                Main.ClearHighlights();
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (_matches.Count == 0 || _matches.All(m => m.IsDismissed))
            {
                MessageBox.Show("No results to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV Files|*.csv|JSON Files|*.json|All Files|*.*";
                dialog.FileName = $"secrets_report_{DateTime.Now:yyyyMMdd_HHmmss}";
                dialog.Title = "Export SecretsFinder Results";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExportResults(dialog.FileName);
                        MessageBox.Show($"Results exported to:\n{dialog.FileName}",
                            "Export Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed:\n{ex.Message}",
                            "Export Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportResults(string filePath)
        {
            var visibleMatches = _matches.Where(m => !m.IsDismissed).ToList();
            var sb = new StringBuilder();

            if (filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                // JSON export
                sb.AppendLine("[");
                for (int i = 0; i < visibleMatches.Count; i++)
                {
                    var m = visibleMatches[i];
                    sb.Append("  {");
                    sb.Append($"\"file\":\"{EscapeJson(m.FilePath)}\",");
                    sb.Append($"\"line\":{m.LineNumber},");
                    sb.Append($"\"type\":\"{EscapeJson(m.Pattern?.Name ?? "Unknown")}\",");
                    sb.Append($"\"severity\":\"{m.Pattern?.Severity.ToString() ?? "Unknown"}\",");
                    sb.Append($"\"valueObscured\":\"{EscapeJson(m.ObscuredValue)}\"");
                    sb.Append("}");
                    sb.AppendLine(i < visibleMatches.Count - 1 ? "," : "");
                }
                sb.AppendLine("]");
            }
            else
            {
                // CSV export
                sb.AppendLine("File,Line,Type,Severity,Value (Obscured)");
                foreach (var m in visibleMatches)
                {
                    sb.AppendLine($"\"{EscapeCsv(m.FilePath)}\",{m.LineNumber},\"{EscapeCsv(m.Pattern?.Name ?? "Unknown")}\",{m.Pattern?.Severity.ToString() ?? "Unknown"},\"{EscapeCsv(m.ObscuredValue)}\"");
                }
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private string EscapeCsv(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\"", "\"\"");
        }
    }
}
