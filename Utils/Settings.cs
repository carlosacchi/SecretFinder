using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using CsvQuery.PluginInfrastructure;
using SecretsFinder.Core;

namespace SecretsFinder.Utils
{
    public class Settings : SettingsBase
    {
        public override void OnSettingsChanged()
        {
            base.OnSettingsChanged();
            Main.RestyleEverything();
            Main.OnSettingsChanged();
        }

        #region SCANNING
        [Description("Automatically scan documents when they are opened"),
            Category("Scanning"), DefaultValue(false)]
        public bool auto_scan_on_open { get; set; }

        [Description("Show notification when secrets are found during auto-scan"),
            Category("Scanning"), DefaultValue(true)]
        public bool show_auto_scan_notification { get; set; }

        [Description("Scan files larger than this size (in KB) - set to 0 for unlimited"),
            Category("Scanning"), DefaultValue(5000)]
        public int max_file_size_kb { get; set; }
        #endregion

        #region PATTERNS_CLOUD
        [Description("Enable AWS Access Key detection (AKIA...)"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_aws_access_key { get; set; }

        [Description("Enable AWS Secret Key detection"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_aws_secret_key { get; set; }

        [Description("Enable Google API Key detection (AIza...)"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_google_api_key { get; set; }

        [Description("Enable Azure Storage Connection String detection"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_azure_storage { get; set; }

        [Description("Enable Azure SAS Token detection"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_azure_sas_token { get; set; }

        [Description("Enable Firebase URL detection"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_firebase_url { get; set; }

        [Description("Enable Heroku API Key detection"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_heroku_api_key { get; set; }

        [Description("Enable Azure Client Secret detection (Entra ID app passwords)"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_azure_client_secret { get; set; }

        [Description("Enable Azure GUID detection (Client ID, Tenant ID, Secret ID)"),
            Category("Cloud Patterns"), DefaultValue(true)]
        public bool pattern_azure_client_id { get; set; }
        #endregion

        #region PATTERNS_SERVICES
        [Description("Enable GitHub Token detection (ghp_, gho_, etc.)"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_github_token { get; set; }

        [Description("Enable GitHub Classic Token detection"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_github_token_classic { get; set; }

        [Description("Enable Stripe Live Key detection (sk_live_)"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_stripe_live_key { get; set; }

        [Description("Enable Stripe Restricted Key detection (rk_live_)"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_stripe_restricted_key { get; set; }

        [Description("Enable Slack Token detection (xox...)"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_slack_token { get; set; }

        [Description("Enable Slack Webhook URL detection"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_slack_webhook { get; set; }

        [Description("Enable Discord Webhook URL detection"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_discord_webhook { get; set; }

        [Description("Enable Twilio API Key detection"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_twilio_api_key { get; set; }

        [Description("Enable SendGrid API Key detection"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_sendgrid_api_key { get; set; }

        [Description("Enable Mailchimp API Key detection"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_mailchimp_api_key { get; set; }

        [Description("Enable NPM Token detection"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_npm_token { get; set; }

        [Description("Enable NuGet API Key detection"),
            Category("Service Patterns"), DefaultValue(true)]
        public bool pattern_nuget_api_key { get; set; }
        #endregion

        #region PATTERNS_GENERIC
        [Description("Enable JWT Token detection (eyJ...)"),
            Category("Generic Patterns"), DefaultValue(true)]
        public bool pattern_jwt_token { get; set; }

        [Description("Enable Private Key Header detection (-----BEGIN PRIVATE KEY-----)"),
            Category("Generic Patterns"), DefaultValue(true)]
        public bool pattern_private_key_header { get; set; }

        [Description("Enable Connection String Password detection"),
            Category("Generic Patterns"), DefaultValue(true)]
        public bool pattern_connection_string_password { get; set; }

        [Description("Enable Generic Secret/API Key detection"),
            Category("Generic Patterns"), DefaultValue(true)]
        public bool pattern_generic_secret { get; set; }

        [Description("Enable Bearer Token detection"),
            Category("Generic Patterns"), DefaultValue(true)]
        public bool pattern_bearer_token { get; set; }

        [Description("Enable Basic Auth detection"),
            Category("Generic Patterns"), DefaultValue(true)]
        public bool pattern_basic_auth { get; set; }

        [Description("Enable High Entropy String detection (potential secrets, may have false positives)"),
            Category("Generic Patterns"), DefaultValue(false)]
        public bool pattern_high_entropy_string { get; set; }
        #endregion

        #region CUSTOM_PATTERNS
        [Description("Custom regex patterns (one per line, format: Name|Regex|Severity)\n" +
                    "Severity values: Low, Medium, High, Critical\n" +
                    "Example: MyApiKey|my_api_[a-z0-9]{32}|High"),
            Category("Custom Patterns"), DefaultValue("")]
        public string custom_patterns { get; set; }
        #endregion

        #region DISPLAY
        [Description("Highlight color for detected secrets (HTML format, e.g., #FF6B6B)"),
            Category("Display"), DefaultValue("#FF6B6B")]
        public string highlight_color { get; set; }

        [Description("Use the same colors as the editor window for this plugin's forms?"),
            Category("Display"), DefaultValue(true)]
        public bool use_npp_styling { get; set; }

        [Description("Show toolbar icon for quick scanning ('s' = show, empty = hide)"),
            Category("Display"), DefaultValue("s")]
        public string toolbar_icons { get; set; }
        #endregion

        public List<SecretPattern> GetEnabledPatterns()
        {
            var patterns = new List<SecretPattern>();

            foreach (var builtIn in SecretPattern.BuiltInPatterns)
            {
                var propName = $"pattern_{builtIn.Id}";
                var prop = GetType().GetProperty(propName);

                var patternCopy = new SecretPattern
                {
                    Id = builtIn.Id,
                    Name = builtIn.Name,
                    Pattern = builtIn.Pattern,
                    Severity = builtIn.Severity,
                    Description = builtIn.Description,
                    CompiledRegex = builtIn.CompiledRegex,
                    IsEnabled = prop != null ? (bool)prop.GetValue(this) : true
                };

                patterns.Add(patternCopy);
            }

            // Parse custom patterns
            if (!string.IsNullOrWhiteSpace(custom_patterns))
            {
                foreach (var line in custom_patterns.Split('\n'))
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine))
                        continue;

                    var parts = trimmedLine.Split('|');
                    if (parts.Length >= 2)
                    {
                        var severity = SecretSeverity.Medium;
                        if (parts.Length >= 3)
                        {
                            switch (parts[2].Trim().ToLower())
                            {
                                case "low": severity = SecretSeverity.Low; break;
                                case "high": severity = SecretSeverity.High; break;
                                case "critical": severity = SecretSeverity.Critical; break;
                            }
                        }

                        try
                        {
                            patterns.Add(new SecretPattern(
                                $"custom_{patterns.Count}",
                                parts[0].Trim(),
                                parts[1].Trim(),
                                severity,
                                "Custom pattern"
                            ));
                        }
                        catch
                        {
                            // Skip invalid patterns
                        }
                    }
                }
            }

            return patterns;
        }

        public Color GetHighlightColor()
        {
            try
            {
                return ColorTranslator.FromHtml(highlight_color);
            }
            catch
            {
                return Color.FromArgb(255, 107, 107); // Default red
            }
        }
    }
}
