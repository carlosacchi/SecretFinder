using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;
using SecretsFinder.Core;
using SecretsFinder.Forms;
using SecretsFinder.Utils;
using static Kbg.NppPluginNET.PluginInfrastructure.Win32;

namespace SecretsFinder
{
    class Main
    {
        #region Fields
        internal const string PluginName = "SecretsFinder";
        public static readonly string PluginConfigDirectory = Path.Combine(Npp.notepad.GetConfigDirectory(), PluginName);
        public const string PluginRepository = "https://github.com/carlosacchi/SecretFinder";

        public static Settings settings = new Settings();
        private static IndicatorManager indicatorManager = new IndicatorManager();
        private static List<SecretMatch> currentMatches = new List<SecretMatch>();

        // Forms
        public static ResultsPanel resultsPanel = null;
        private static Icon dockingFormIcon = null;
        private static IntPtr dockingFormIconHandle = IntPtr.Zero;

        // Menu item IDs
        static internal int IdScanCurrent = 0;
        static internal int IdScanAllOpen = 1;
        static internal int IdScanBackup = 2;
        static internal int IdResultsPanel = 4;
        static internal int IdSettings = 6;
        static internal int IdAbout = 7;
        #endregion

        #region Startup/Cleanup
        static internal void CommandMenuInit()
        {
            // Menu items
            PluginBase.SetCommand(0, "Scan &Current Document", ScanCurrentDocument,
                new ShortcutKey(true, true, false, Keys.S)); // Ctrl+Alt+S
            IdScanCurrent = 0;

            PluginBase.SetCommand(1, "Scan &All Open Documents", ScanAllOpenDocuments);
            IdScanAllOpen = 1;

            PluginBase.SetCommand(2, "Scan &Backup Folder", ScanBackupFolder);
            IdScanBackup = 2;

            PluginBase.SetCommand(3, "---", null); // Separator

            PluginBase.SetCommand(4, "&Results Panel", ToggleResultsPanel);
            IdResultsPanel = 4;

            PluginBase.SetCommand(5, "---", null); // Separator

            PluginBase.SetCommand(6, "S&ettings", OpenSettings);
            IdSettings = 6;

            PluginBase.SetCommand(7, "&About", ShowAboutForm);
            IdAbout = 7;

            PluginBase.SetCommand(8, "Debug: Show &Open Files", ShowOpenFilesDebug);
            // This debug command shows detailed information about file enumeration

            // Initialize indicator
            indicatorManager.Initialize();
            indicatorManager.SetupIndicatorStyle(settings.GetHighlightColor());
        }

        static internal void SetToolBarIcons()
        {
            string iconsToUseChars = settings.toolbar_icons.ToLower();

            if (iconsToUseChars.Contains('s'))
            {
                // Create a simple scan icon programmatically
                using (Bitmap bmp = CreateScanIcon(false))
                using (Icon icon = CreateScanIconAsIcon(false))
                using (Icon iconDarkMode = CreateScanIconAsIcon(true))
                {
                    toolbarIcons tbIcons = new toolbarIcons();
                    tbIcons.hToolbarBmp = bmp.GetHbitmap();
                    tbIcons.hToolbarIcon = icon.Handle;
                    tbIcons.hToolbarIconDarkMode = iconDarkMode.Handle;

                    IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
                    Marshal.StructureToPtr(tbIcons, pTbIcons, false);

                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON_FORDARKMODE,
                        PluginBase._funcItems.Items[IdScanCurrent]._cmdID, pTbIcons);

                    Marshal.FreeHGlobal(pTbIcons);

                    // Clean up GDI handle to prevent leak
                    Win32.DeleteObject(tbIcons.hToolbarBmp);
                }
            }
        }

        private static Bitmap CreateScanIcon(bool darkMode)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                Color keyColor = darkMode ? Color.FromArgb(200, 200, 200) : Color.FromArgb(80, 80, 80);
                Color alertColor = Color.FromArgb(255, 100, 100);

                using (Pen keyPen = new Pen(keyColor, 1.5f))
                using (SolidBrush alertBrush = new SolidBrush(alertColor))
                {
                    // Draw key head (circle)
                    g.DrawEllipse(keyPen, 2, 3, 6, 6);

                    // Draw key shaft
                    g.DrawLine(keyPen, 8, 6, 13, 6);

                    // Draw key teeth
                    g.DrawLine(keyPen, 10, 6, 10, 9);
                    g.DrawLine(keyPen, 12, 6, 12, 8);

                    // Draw alert dot
                    g.FillEllipse(alertBrush, 11, 1, 4, 4);
                }
            }
            return bmp;
        }

        private static Icon CreateScanIconAsIcon(bool darkMode)
        {
            using (Bitmap bmp = CreateScanIcon(darkMode))
            {
                return Icon.FromHandle(bmp.GetHicon());
            }
        }

        public static void OnNotification(ScNotification notification)
        {
            uint code = notification.Header.Code;

            switch (code)
            {
                case (uint)NppMsg.NPPN_BUFFERACTIVATED:
                    Npp.editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());

                    // Auto-scan if enabled
                    if (settings.auto_scan_on_open)
                    {
                        ScanCurrentDocumentSilent();
                    }
                    else
                    {
                        // Highlight existing matches for current file
                        string currentFile = Npp.notepad.GetCurrentFilePath();
                        indicatorManager.HighlightMatchesForCurrentFile(currentMatches, currentFile);
                    }
                    break;

                case (uint)NppMsg.NPPN_WORDSTYLESUPDATED:
                    RestyleEverything();
                    break;

                case (uint)NppMsg.NPPN_READY:
                    // Plugin fully loaded
                    break;
            }
        }

        static internal void PluginCleanUp()
        {
            if (resultsPanel != null && !resultsPanel.IsDisposed)
            {
                resultsPanel.Close();
                resultsPanel.Dispose();
            }
        }
        #endregion

        #region Menu Functions

        public static void ScanCurrentDocument()
        {
            // Use long to prevent integer overflow on large files
            long length = Npp.editor.GetLength();
            if (length > int.MaxValue - 1)
            {
                MessageBox.Show("Document is too large to scan (>2GB).",
                    "SecretsFinder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string text = Npp.editor.GetText((int)length + 1);
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("No text to scan in current document.",
                    "SecretsFinder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string filePath = Npp.notepad.GetCurrentFilePath();
            var scanner = new SecretScanner(settings.GetEnabledPatterns());
            var matches = scanner.ScanText(text, filePath);

            // Update current matches (remove old matches for this file, add new ones)
            currentMatches.RemoveAll(m => string.Equals(m.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            currentMatches.AddRange(matches);

            // Highlight matches
            indicatorManager.SetupIndicatorStyle(settings.GetHighlightColor());
            indicatorManager.HighlightMatches(matches);

            // Always update results panel to clear stale results
            EnsureResultsPanelVisible();
            resultsPanel.UpdateResults(currentMatches);

            // Show/update results panel
            if (matches.Count > 0)
            {
                MessageBox.Show(
                    $"Found {matches.Count} potential secret(s) in the current document.\n\n" +
                    "Double-click a result in the panel to navigate to it.",
                    "SecretsFinder - Scan Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(
                    "No secrets detected in the current document.",
                    "SecretsFinder - Scan Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private static void ScanCurrentDocumentSilent()
        {
            try
            {
                long length = Npp.editor.GetLength();
                if (length > int.MaxValue - 1)
                    return; // Silently skip files that are too large

                string text = Npp.editor.GetText((int)length + 1);
                if (string.IsNullOrEmpty(text))
                    return;

                string filePath = Npp.notepad.GetCurrentFilePath();
                var scanner = new SecretScanner(settings.GetEnabledPatterns());
                var matches = scanner.ScanText(text, filePath);

                // Always update current matches to clear stale results
                currentMatches.RemoveAll(m => string.Equals(m.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
                currentMatches.AddRange(matches);

                indicatorManager.SetupIndicatorStyle(settings.GetHighlightColor());
                indicatorManager.HighlightMatches(matches);

                // Always update results panel if visible to clear stale results
                if (resultsPanel != null && resultsPanel.Visible)
                {
                    resultsPanel.UpdateResults(currentMatches);
                }

                if (matches.Count > 0)
                {

                    if (settings.show_auto_scan_notification)
                    {
                        MessageBox.Show(
                            $"SecretsFinder detected {matches.Count} potential secret(s) in this file!",
                            "SecretsFinder - Auto-Scan Alert",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }
            catch
            {
                // Silent fail for auto-scan
            }
        }

        public static void ScanAllOpenDocuments()
        {
            var allMatches = new List<SecretMatch>();
            var scanner = new SecretScanner(settings.GetEnabledPatterns());
            string[] openFiles = Npp.notepad.GetOpenFileNames();
            int filesScanned = 0;

            // Remember current file to restore focus later
            string originalFile = Npp.notepad.GetCurrentFilePath();

            foreach (string filePath in openFiles)
            {
                try
                {
                    // Activate each open buffer so we read the in-memory text (unsaved edits included)
                    Npp.notepad.ActivateFile(filePath);
                    Npp.editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());

                    long length = Npp.editor.GetLength();
                    if (length > int.MaxValue - 1)
                        continue; // Skip files that are too large

                    string content = Npp.editor.GetText((int)length + 1);
                    if (string.IsNullOrEmpty(content))
                        continue;

                    var matches = scanner.ScanText(content, filePath);
                    allMatches.AddRange(matches);
                    filesScanned++;
                }
                catch
                {
                    // Skip files that can't be read
                }
            }

            // Restore original buffer focus
            if (!string.IsNullOrEmpty(originalFile) && File.Exists(originalFile))
            {
                try
                {
                    Npp.notepad.ActivateFile(originalFile);
                    Npp.editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
                }
                catch { }
            }

            currentMatches = allMatches;

            // Highlight matches in current document
            string currentFile = Npp.notepad.GetCurrentFilePath();
            indicatorManager.SetupIndicatorStyle(settings.GetHighlightColor());
            indicatorManager.HighlightMatchesForCurrentFile(allMatches, currentFile);

            // Always update results panel to clear stale results
            EnsureResultsPanelVisible();
            resultsPanel.UpdateResults(allMatches);

            if (allMatches.Count > 0)
            {
                int filesWithSecrets = allMatches.Select(m => m.FilePath).Distinct().Count();
                MessageBox.Show(
                    $"Found {allMatches.Count} potential secret(s) across {filesWithSecrets} file(s)\n" +
                    $"(scanned {filesScanned} open documents).\n\n" +
                    "Double-click a result in the panel to navigate to it.",
                    "SecretsFinder - Scan Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(
                    $"No secrets detected in {filesScanned} open document(s).",
                    "SecretsFinder - Scan Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        public static void ScanBackupFolder()
        {
            string backupPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Notepad++", "backup");

            if (!Directory.Exists(backupPath))
            {
                MessageBox.Show(
                    $"Backup folder not found at:\n{backupPath}\n\n" +
                    "This folder is created by Notepad++ when you have unsaved files.",
                    "SecretsFinder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var scanner = new SecretScanner(settings.GetEnabledPatterns());
            var allMatches = scanner.ScanDirectory(backupPath);

            // Always update results panel to clear stale results
            EnsureResultsPanelVisible();
            resultsPanel.UpdateResults(allMatches);

            if (allMatches.Count > 0)
            {
                int filesWithSecrets = allMatches.Select(m => m.FilePath).Distinct().Count();
                MessageBox.Show(
                    $"Found {allMatches.Count} potential secret(s) in {filesWithSecrets} backup file(s)!\n\n" +
                    "This is a security risk - consider cleaning your backup folder:\n" +
                    $"{backupPath}",
                    "SecretsFinder - Backup Scan Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                var files = Directory.GetFiles(backupPath, "*", SearchOption.AllDirectories);
                MessageBox.Show(
                    $"No secrets detected in {files.Length} backup file(s).\n\n" +
                    $"Backup folder: {backupPath}",
                    "SecretsFinder - Backup Scan Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        public static void ToggleResultsPanel()
        {
            if (resultsPanel != null && resultsPanel.Visible)
            {
                Npp.notepad.HideDockingForm(resultsPanel);
            }
            else
            {
                EnsureResultsPanelVisible();
            }
        }

        private static void EnsureResultsPanelVisible()
        {
            if (resultsPanel == null || resultsPanel.IsDisposed)
            {
                resultsPanel = new ResultsPanel();
                DisplayResultsPanel(resultsPanel);
            }
            else
            {
                Npp.notepad.ShowDockingForm(resultsPanel);
            }
        }

        private static void DisplayResultsPanel(ResultsPanel form)
        {
            // Clean up previous icon handle if it exists
            if (dockingFormIconHandle != IntPtr.Zero)
            {
                Win32.DestroyIcon(dockingFormIconHandle);
                dockingFormIconHandle = IntPtr.Zero;
            }

            using (Bitmap newBmp = CreateScanIcon(false))
            {
                dockingFormIconHandle = newBmp.GetHicon();
                dockingFormIcon = Icon.FromHandle(dockingFormIconHandle);
            }

            NppTbData nppTbData = new NppTbData();
            nppTbData.hClient = form.Handle;
            nppTbData.pszName = "SecretsFinder Results";
            nppTbData.dlgID = IdResultsPanel;
            nppTbData.uMask = NppTbMsg.DWS_DF_CONT_BOTTOM | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
            nppTbData.hIconTab = (uint)dockingFormIcon.Handle;
            nppTbData.pszModuleName = PluginName;

            IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
            Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);

            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);
            Npp.notepad.ShowDockingForm(form);
        }

        public static void OpenSettings()
        {
            settings.ShowDialog();
        }

        public static void ShowAboutForm()
        {
            using (var aboutForm = new AboutForm())
            {
                aboutForm.ShowDialog();
            }
        }

        public static void ShowOpenFilesDebug()
        {
            // Call GetOpenFileNames with debug info enabled
            Npp.notepad.GetOpenFileNames(showDebugInfo: true);
        }

        public static void RestyleEverything()
        {
            if (resultsPanel != null && !resultsPanel.IsDisposed)
                FormStyle.ApplyStyle(resultsPanel, settings.use_npp_styling);
        }

        public static void OnSettingsChanged()
        {
            indicatorManager.SetupIndicatorStyle(settings.GetHighlightColor());

            string currentFile = Npp.notepad.GetCurrentFilePath();
            indicatorManager.HighlightMatchesForCurrentFile(currentMatches, currentFile);
        }

        public static void NavigateToMatch(SecretMatch match)
        {
            if (match == null)
                return;

            // Open file if not current
            string currentFile = Npp.notepad.GetCurrentFilePath();
            if (!string.Equals(currentFile, match.FilePath, StringComparison.OrdinalIgnoreCase))
            {
                bool fileOpened = false;

                // Strategy 1: Try to activate the file if it's already open (handles unsaved buffers)
                // This works for files like "new 1", "BASIC TOOLS" that don't have file paths yet
                if (!string.IsNullOrEmpty(match.FilePath))
                {
                    fileOpened = Npp.notepad.ActivateFile(match.FilePath);
                }

                // Strategy 2: If activation failed, try to open from disk (handles saved files)
                if (!fileOpened && File.Exists(match.FilePath))
                {
                    fileOpened = Npp.notepad.OpenFile(match.FilePath);
                }

                // If both strategies failed, show error
                if (!fileOpened)
                {
                    string fileName = Path.GetFileName(match.FilePath);
                    MessageBox.Show(
                        $"Cannot navigate to file:\n\n{fileName}\n\n" +
                        "The file may have been closed, deleted, or was an unsaved buffer that no longer exists.\n\n" +
                        "Try scanning again to refresh the results.",
                        "SecretsFinder - Navigation Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Reset editor after file change
                Npp.editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            }

            // Go to position and select
            Npp.editor.GotoPos(match.StartPosition);
            Npp.editor.SetSel(match.StartPosition, match.EndPosition);

            // Ensure line is visible
            int line = Npp.editor.LineFromPosition(match.StartPosition);
            Npp.editor.EnsureVisible(line);
        }

        public static void ClearHighlights()
        {
            indicatorManager.ClearAllHighlights();
        }

        public static void RefreshHighlights()
        {
            string currentFile = Npp.notepad.GetCurrentFilePath();
            indicatorManager.HighlightMatchesForCurrentFile(currentMatches, currentFile);
        }
        #endregion
    }
}
