/*
Utilities for storing, viewing, and updating the settings of the plugin.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SecretsFinder;
using SecretsFinder.Utils;
using Kbg.NppPluginNET.PluginInfrastructure;
using System.Reflection;

namespace CsvQuery.PluginInfrastructure
{
    public class SettingsBase
    {
        private const int DEFAULT_WIDTH = 430;
        private const int DEFAULT_HEIGHT = 300;

        private static readonly string IniFilePath;

        /// <summary> Delegate for update events </summary>
        public delegate void RepositoryEventHandler(object sender, SettingsChangedEventArgs e);

        /// <summary> Raised before settings has been changed, allowing listeners to cancel the change </summary>
        public event RepositoryEventHandler ValidateChanges;

        /// <summary> Raised after a setting has been changed </summary>
        public event RepositoryEventHandler SettingsChanged;

        /// <summary> Overridable event logic </summary>
        protected virtual void OnSettingsChanged(object sender, SettingsChangedEventArgs e)
        {
            SettingsChanged?.Invoke(sender, e);
        }

        /// <summary> Overridable event logic </summary>
        protected virtual bool OnValidateChanges(object sender, SettingsChangedEventArgs e)
        {
            ValidateChanges?.Invoke(sender, e);
            return !e.Cancel;
        }

        static SettingsBase()
        {
            IniFilePath = Path.Combine(Main.PluginConfigDirectory, Main.PluginName + ".ini");
        }

        /// <summary>
        /// things to do whenever the settings are changed by using the dialog
        /// </summary>
        public virtual void OnSettingsChanged()
        {
            SaveToIniFile();
        }

        /// <summary>
        /// By default loads settings from the default N++ config folder
        /// </summary>
        public SettingsBase(bool loadFromFile = true)
        {
            // Set defaults
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() is DefaultValueAttribute def)
                {
                    propertyInfo.SetValue(this, def.Value, null);
                }
            }
            if (loadFromFile && !ReadFromIniFile())
                SaveToIniFile();
        }

        /// <summary>
        /// Reads all (existing) settings from an ini-file
        /// </summary>
        public bool ReadFromIniFile(string filename = null)
        {
            filename = filename ?? IniFilePath;
            if (!File.Exists(filename))
                return false;

            var loaded = GetType().GetProperties()
                .Select(x => ((CategoryAttribute)x.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault())?.Category ?? "General")
                .Distinct()
                .ToDictionary(section => section, section => GetKeys(filename, section));

            bool allConvertedCorrectly = true;
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var category = ((CategoryAttribute)propertyInfo.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault())?.Category ?? "General";
                var name = propertyInfo.Name;
                if (loaded.ContainsKey(category) && loaded[category].ContainsKey(name) && !string.IsNullOrEmpty(loaded[category][name]))
                {
                    var rawString = loaded[category][name];
                    var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                    bool convertedCorrectly = false;
                    if (converter.IsValid(rawString))
                    {
                        try
                        {
                            propertyInfo.SetValue(this, converter.ConvertFromInvariantString(rawString), null);
                            convertedCorrectly = true;
                        }
                        catch
                        {
                            // Conversion failed, use default
                        }
                    }
                    if (!convertedCorrectly)
                    {
                        allConvertedCorrectly = false;
                        SetPropertyInfoToDefault(propertyInfo);
                    }
                }
            }
            return allConvertedCorrectly;
        }

        private bool SetPropertyInfoToDefault(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() is DefaultValueAttribute def)
            {
                propertyInfo.SetValue(this, def.Value, null);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves all settings to an ini-file
        /// </summary>
        public void SaveToIniFile(string filename = null)
        {
            filename = filename ?? IniFilePath;
            Npp.CreateConfigSubDirectoryIfNotExists();

            using (var fp = new StreamWriter(filename, false, Encoding.UTF8))
            {
                fp.WriteLine("; {0} settings file", Main.PluginName);

                foreach (var section in GetType()
                    .GetProperties()
                    .GroupBy(x => ((CategoryAttribute)x.GetCustomAttributes(typeof(CategoryAttribute), false)
                                        .FirstOrDefault())?.Category ?? "General"))
                {
                    fp.WriteLine(Environment.NewLine + "[{0}]", section.Key);
                    foreach (var propertyInfo in section.OrderBy(x => x.Name))
                    {
                        var desc = (DescriptionAttribute)propertyInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                        string description = desc?.Description ?? propertyInfo.Name;
                        fp.WriteLine("; " + description.Replace(Environment.NewLine, Environment.NewLine + "; "));
                        var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                        fp.WriteLine("{0}={1}", propertyInfo.Name, converter.ConvertToInvariantString(propertyInfo.GetValue(this, null)));
                    }
                }
            }
        }

        private Dictionary<string, string> GetKeys(string iniFile, string category)
        {
            var buffer = new byte[8 * 1024];
            Win32.GetPrivateProfileSection(category, buffer, buffer.Length, iniFile);
            var tmp = Encoding.UTF8.GetString(buffer).Trim('\0').Split('\0');
            return tmp.Select(x => x.Split(new[] { '=' }, 2))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]);
        }

        /// <summary>
        /// Opens a window that edits all settings
        /// </summary>
        public void ShowDialog(bool debug = false)
        {
            var copy = (Settings)MemberwiseClone();

            var dialog = new Form
            {
                Name = "SettingsForm",
                Text = $"Settings - {Main.PluginName}",
                ClientSize = new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT),
                MinimumSize = new Size(250, 250),
                ShowIcon = false,
                AutoScaleMode = AutoScaleMode.Font,
                AutoScaleDimensions = new SizeF(6F, 13F),
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterParent,
                Controls =
                {
                    new Button
                    {
                        Name = "Cancel",
                        Text = "&Cancel",
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                        Size = new Size(75, 23),
                        Location = new Point(DEFAULT_WIDTH - 115, DEFAULT_HEIGHT - 36),
                        UseVisualStyleBackColor = true
                    },
                    new Button
                    {
                        Name = "Reset",
                        Text = "&Reset",
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                        Size = new Size(75, 23),
                        Location = new Point(DEFAULT_WIDTH - 212, DEFAULT_HEIGHT - 36),
                        UseVisualStyleBackColor = true
                    },
                    new Button
                    {
                        Name = "Ok",
                        Text = "&Ok",
                        Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                        Size = new Size(75, 23),
                        Location = new Point(DEFAULT_WIDTH - 310, DEFAULT_HEIGHT - 36),
                        UseVisualStyleBackColor = true
                    },
                    new PropertyGrid
                    {
                        Name = "Grid",
                        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                        Location = new Point(13, 13),
                        Size = new Size(DEFAULT_WIDTH - 13 - 13, DEFAULT_HEIGHT - 55),
                        AutoScaleMode = AutoScaleMode.Font,
                        AutoScaleDimensions = new SizeF(6F, 13F),
                        SelectedObject = copy
                    },
                }
            };

            dialog.Controls["Cancel"].Click += (a, b) => dialog.Close();
            dialog.Controls["Ok"].Click += (a, b) =>
            {
                var changesEventArgs = new SettingsChangedEventArgs((Settings)this, copy);
                if (!changesEventArgs.Changed.Any())
                {
                    dialog.Close();
                    return;
                }
                foreach (var propertyInfo in GetType().GetProperties())
                {
                    var oldValue = propertyInfo.GetValue(this, null);
                    var newValue = propertyInfo.GetValue(copy, null);
                    if (!oldValue.Equals(newValue))
                    {
                        try
                        {
                            propertyInfo.SetValue(this, newValue, null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Could not change setting {propertyInfo.Name} to value {newValue}.\r\n{ex.Message}",
                                "Invalid Setting",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                OnSettingsChanged();
                dialog.Close();
            };
            dialog.Controls["Reset"].Click += (a, b) =>
            {
                foreach (var propertyInfo in GetType().GetProperties())
                {
                    SetPropertyInfoToDefault(propertyInfo);
                }
                OnSettingsChanged();
                dialog.Close();
            };

            KeyEventHandler keyDownHandler = (a, b) =>
            {
                if (b.KeyCode == Keys.Escape)
                    dialog.Close();
            };
            dialog.KeyDown += keyDownHandler;
            foreach (Control ctrl in dialog.Controls)
                ctrl.KeyDown += keyDownHandler;

            dialog.ShowDialog();
        }

        /// <summary> Opens the config file directly in Notepad++ </summary>
        public void OpenFile()
        {
            if (!File.Exists(IniFilePath)) SaveToIniFile();
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DOOPEN, 0, IniFilePath);
        }
    }

    public class SettingsChangedEventArgs : CancelEventArgs
    {
        public HashSet<string> Changed { get; }
        public Settings OldSettings { get; }
        public Settings NewSettings { get; }

        public SettingsChangedEventArgs(Settings oldSettings, Settings newSettings)
        {
            OldSettings = oldSettings;
            NewSettings = newSettings;
            Changed = new HashSet<string>();
            foreach (var propertyInfo in typeof(Settings).GetProperties())
            {
                var oldValue = propertyInfo.GetValue(oldSettings, null);
                var newValue = propertyInfo.GetValue(newSettings, null);
                if (!oldValue.Equals(newValue))
                {
                    Trace.TraceInformation($"Setting {propertyInfo.Name} has changed");
                    Changed.Add(propertyInfo.Name);
                }
            }
        }
    }
}
