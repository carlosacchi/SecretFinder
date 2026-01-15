using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SecretsFinder.Core
{
    public enum SecretSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class SecretPattern
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Pattern { get; set; }
        public Regex CompiledRegex { get; set; }
        public bool IsEnabled { get; set; }
        public SecretSeverity Severity { get; set; }
        public string Description { get; set; }

        public SecretPattern()
        {
            IsEnabled = true;
        }

        public SecretPattern(string id, string name, string pattern, SecretSeverity severity, string description)
        {
            Id = id;
            Name = name;
            Pattern = pattern;
            Severity = severity;
            Description = description;
            IsEnabled = true;
            CompiledRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);
        }

        public static readonly List<SecretPattern> BuiltInPatterns = new List<SecretPattern>
        {
            new SecretPattern(
                "aws_access_key",
                "AWS Access Key",
                @"AKIA[0-9A-Z]{16,128}",
                SecretSeverity.High,
                "AWS Access Key ID (16-128 chars)"
            ),
            new SecretPattern(
                "aws_secret_key",
                "AWS Secret Key",
                @"(?i)aws[_\-]?secret[_\-]?(?:access[_\-]?)?key[\s]*[=:][\s]*['""]?([A-Za-z0-9/+=]{40})['""]?",
                SecretSeverity.Critical,
                "AWS Secret Access Key"
            ),
            new SecretPattern(
                "github_token",
                "GitHub Token",
                @"gh[pousr]_[0-9a-zA-Z]{36,}",
                SecretSeverity.High,
                "GitHub Personal Access Token"
            ),
            new SecretPattern(
                "github_token_classic",
                "GitHub Token (Classic)",
                @"ghp_[0-9a-zA-Z]{36,}",
                SecretSeverity.High,
                "GitHub Classic Personal Access Token"
            ),
            new SecretPattern(
                "stripe_live_key",
                "Stripe Live Key",
                @"sk_live_[0-9a-zA-Z]{24,}",
                SecretSeverity.Critical,
                "Stripe Live Secret Key"
            ),
            new SecretPattern(
                "stripe_restricted_key",
                "Stripe Restricted Key",
                @"rk_live_[0-9a-zA-Z]{24,}",
                SecretSeverity.Critical,
                "Stripe Restricted API Key"
            ),
            new SecretPattern(
                "google_api_key",
                "Google API Key",
                @"AIza[0-9A-Za-z\-_]{35}",
                SecretSeverity.High,
                "Google API Key"
            ),
            new SecretPattern(
                "jwt_token",
                "JWT Token",
                @"eyJ[a-zA-Z0-9_-]*\.eyJ[a-zA-Z0-9_-]*\.[a-zA-Z0-9_-]*",
                SecretSeverity.Medium,
                "JSON Web Token"
            ),
            new SecretPattern(
                "private_key_header",
                "Private Key",
                @"-----BEGIN\s+(RSA\s+|EC\s+|DSA\s+|OPENSSH\s+)?PRIVATE\s+KEY-----",
                SecretSeverity.Critical,
                "Private Key Header"
            ),
            new SecretPattern(
                "azure_storage",
                "Azure Storage Connection",
                @"DefaultEndpointsProtocol=https;AccountName=[^;]+;AccountKey=[A-Za-z0-9+/=]+",
                SecretSeverity.Critical,
                "Azure Storage Connection String"
            ),
            new SecretPattern(
                "azure_sas_token",
                "Azure SAS Token",
                @"[?&]sig=[A-Za-z0-9%]+",
                SecretSeverity.High,
                "Azure Shared Access Signature"
            ),
            new SecretPattern(
                "slack_token",
                "Slack Token",
                @"xox[baprs]-[0-9]{10,13}-[0-9]{10,13}[a-zA-Z0-9-]*",
                SecretSeverity.High,
                "Slack API Token"
            ),
            new SecretPattern(
                "slack_webhook",
                "Slack Webhook",
                @"https://hooks\.slack\.com/services/T[a-zA-Z0-9_]+/B[a-zA-Z0-9_]+/[a-zA-Z0-9_]+",
                SecretSeverity.High,
                "Slack Incoming Webhook URL"
            ),
            new SecretPattern(
                "discord_webhook",
                "Discord Webhook",
                @"https://discord(?:app)?\.com/api/webhooks/[0-9]+/[A-Za-z0-9_-]+",
                SecretSeverity.High,
                "Discord Webhook URL"
            ),
            new SecretPattern(
                "twilio_api_key",
                "Twilio API Key",
                @"SK[0-9a-fA-F]{32}",
                SecretSeverity.High,
                "Twilio API Key"
            ),
            new SecretPattern(
                "sendgrid_api_key",
                "SendGrid API Key",
                @"SG\.[A-Za-z0-9_-]{22}\.[A-Za-z0-9_-]{43}",
                SecretSeverity.High,
                "SendGrid API Key"
            ),
            new SecretPattern(
                "mailchimp_api_key",
                "Mailchimp API Key",
                @"[0-9a-f]{32}-us[0-9]{1,2}",
                SecretSeverity.High,
                "Mailchimp API Key"
            ),
            new SecretPattern(
                "connection_string_password",
                "Connection String Password",
                @"(?i)(password|pwd)[\s]*[=:][\s]*['""]?[^\s;'""]{8,}['""]?",
                SecretSeverity.Medium,
                "Password in connection string"
            ),
            new SecretPattern(
                "generic_secret",
                "Generic Secret",
                @"(?i)(secret|api[_\-]?key|apikey|auth[_\-]?token|access[_\-]?token)[\s]*[=:][\s]*['""]?[a-zA-Z0-9_\-]{16,}['""]?",
                SecretSeverity.Medium,
                "Generic secret or API key assignment"
            ),
            new SecretPattern(
                "bearer_token",
                "Bearer Token",
                @"(?i)bearer\s+[a-zA-Z0-9_\-\.]+",
                SecretSeverity.Medium,
                "Bearer authentication token"
            ),
            new SecretPattern(
                "basic_auth",
                "Basic Auth",
                @"(?i)basic\s+[A-Za-z0-9+/=]{20,}",
                SecretSeverity.Medium,
                "Basic authentication credentials"
            ),
            new SecretPattern(
                "probable_password",
                "Probable Password (mixed case)",
                @"(?=.{6,32}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])[A-Za-z0-9!@#$%^&*()_+\-=/\\.,]{6,32}",
                SecretSeverity.Low,
                "Likely password: mixed case + digit, 6-32 chars"
            ),
            new SecretPattern(
                "npm_token",
                "NPM Token",
                @"npm_[A-Za-z0-9]{36}",
                SecretSeverity.High,
                "NPM Access Token"
            ),
            new SecretPattern(
                "nuget_api_key",
                "NuGet API Key",
                @"oy2[a-z0-9]{43}",
                SecretSeverity.High,
                "NuGet API Key"
            ),
            new SecretPattern(
                "heroku_api_key",
                "Heroku API Key",
                @"[h|H][e|E][r|R][o|O][k|K][u|U].{0,30}[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}",
                SecretSeverity.High,
                "Heroku API Key"
            ),
            new SecretPattern(
                "firebase_url",
                "Firebase URL",
                @"https://[a-z0-9-]+\.firebaseio\.com",
                SecretSeverity.Medium,
                "Firebase Database URL"
            ),
            new SecretPattern(
                "azure_client_secret",
                "Azure Client Secret",
                @"[a-zA-Z0-9]{1,3}~[a-zA-Z0-9_.-]{30,}",
                SecretSeverity.High,
                "Azure/Entra ID Client Secret (app password)"
            ),
            new SecretPattern(
                "azure_client_id",
                "Azure Client/Secret ID",
                @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
                SecretSeverity.Medium,
                "Azure GUID (Client ID, Secret ID, Tenant ID)"
            ),
            new SecretPattern(
                "simple_password",
                "Simple Password",
                @"(?i)(password|passwd|pwd|pass|secret)[\s]*[=:][\s]*['\""]?([a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':\\|,.<>\/?]{6,})['\""]?",
                SecretSeverity.Low,
                "Simple password (6+ chars)"
            ),
            new SecretPattern(
                "high_entropy_string",
                "High Entropy String",
                @"[a-zA-Z0-9+/~_\-\.]{12,}",
                SecretSeverity.Low,
                "High entropy string (12+ chars)"
            )
        };
    }
}
