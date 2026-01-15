# SecretsFinder - Notepad++ Plugin

A Notepad++ plugin that detects secrets, API keys, tokens, and other sensitive information in your documents.

---

## Quick Start

### Step 1: Build the Plugin

1. Open `SecretsFinder.sln` in **Visual Studio 2022** (or later)
2. Select build configuration:
   - **64-bit Notepad++** (most common): `Release | x64`
   - **32-bit Notepad++**: `Release | x86`
3. Build: `Ctrl+Shift+B`

### Step 2: Install the Plugin

The build automatically copies the DLL to Notepad++. If it doesn't work:

**Manual installation:**
1. Find the built DLL:
   - 64-bit: `bin\Release-x64\SecretsFinder.dll`
   - 32-bit: `bin\Release\SecretsFinder.dll`

2. Copy to Notepad++ plugins folder:
   - 64-bit: `C:\Program Files\Notepad++\plugins\SecretsFinder\SecretsFinder.dll`
   - 32-bit: `C:\Program Files (x86)\Notepad++\plugins\SecretsFinder\SecretsFinder.dll`

3. **Restart Notepad++**

### Step 3: Use the Plugin

1. Open any file in Notepad++
2. Press `Ctrl+Alt+S` to scan for secrets
3. Found secrets are **highlighted in red** in the editor
4. Open **Results Panel** to see all findings

---

## Features

| Feature | Description |
|---------|-------------|
| **Scan Current Document** | Detect secrets in active file (`Ctrl+Alt+S`) |
| **Scan All Open Documents** | Check all open tabs for secrets |
| **Scan Backup Folder** | Check Notepad++ backup folder for exposed secrets |
| **Visual Highlighting** | Secrets are highlighted in the editor |
| **Results Panel** | Dockable panel with all findings |
| **Export Results** | Save findings to CSV or JSON |
| **Configurable Patterns** | Enable/disable patterns, add custom ones |

---

## Menu

**Plugins → SecretsFinder**

| Menu Item | Shortcut | Description |
|-----------|----------|-------------|
| Scan Current Document | `Ctrl+Alt+S` | Scan the active document |
| Scan All Open Documents | - | Scan all open tabs |
| Scan Backup Folder | - | Scan `%APPDATA%\Notepad++\backup` |
| Results Panel | - | Toggle results panel visibility |
| Settings | - | Configure patterns and display |
| About | - | Plugin information |

---

## Detected Secrets

### Cloud Providers
| Secret Type | Example Pattern |
|-------------|-----------------|
| AWS Access Key | `AKIAIOSFODNN7EXAMPLE` |
| AWS Secret Key | `aws_secret_key = ...` |
| Google API Key | `AIzaSyC...` |
| Azure Storage | `DefaultEndpointsProtocol=https;AccountName=...` |
| Azure SAS Token | `?sig=...` |
| Firebase URL | `https://xxx.firebaseio.com` |
| Heroku API Key | `heroku...UUID` |

### Service Tokens
| Secret Type | Example Pattern |
|-------------|-----------------|
| GitHub Token | `ghp_xxxxxxxxxxxx...` |
| Stripe Live Key | `sk_live_...` |
| Stripe Restricted Key | `rk_live_...` |
| Slack Token | `xoxb-...` |
| Slack Webhook | `https://hooks.slack.com/services/...` |
| Discord Webhook | `https://discord.com/api/webhooks/...` |
| Twilio API Key | `SK...` (32 hex chars) |
| SendGrid API Key | `SG....` |
| Mailchimp API Key | `....-us1` |
| NPM Token | `npm_...` |
| NuGet API Key | `oy2...` |

### Generic Patterns
| Secret Type | Example Pattern |
|-------------|-----------------|
| JWT Token | `eyJhbGciOiJIUzI1NiIs...` |
| Private Key | `-----BEGIN PRIVATE KEY-----` |
| Connection String | `password=...` or `pwd=...` |
| Bearer Token | `Bearer eyJ...` |
| Basic Auth | `Basic dXNlcjpwYXNz...` |
| Generic API Key | `api_key = "..."` |

---

## Settings

Access via **Plugins → SecretsFinder → Settings**

### Scanning
- `auto_scan_on_open` - Automatically scan when files are opened
- `show_auto_scan_notification` - Show popup when secrets found
- `max_file_size_kb` - Skip files larger than this (0 = unlimited)

### Patterns
Enable/disable individual pattern types:
- Cloud: AWS, Google, Azure, Firebase, Heroku
- Services: GitHub, Stripe, Slack, Discord, Twilio, SendGrid, etc.
- Generic: JWT, Private Keys, Connection Strings, Bearer/Basic Auth

### Display
- `highlight_color` - Color for highlighting secrets (default: `#FF6B6B`)
- `use_npp_styling` - Match Notepad++ theme colors
- `toolbar_icons` - Show toolbar icon (`s` = show, empty = hide)

### Custom Patterns

Add your own patterns in settings:

```
PatternName|RegexPattern|Severity
```

**Severity levels:** `Low`, `Medium`, `High`, `Critical`

**Example:**
```
MyCompanyKey|MYCO_[A-Z0-9]{32}|High
InternalToken|INT_[a-f0-9]{64}|Critical
```

---

## Test File

Create a test file to verify the plugin works:

```
# Test secrets - these are fake examples
aws_key = AKIAIOSFODNN7EXAMPLE
github_token = ghp_abcdefghijklmnopqrstuvwxyz0123456789
stripe_key = sk_live_abcdefghijklmnopqrstuvwx
jwt = eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U
password = SuperSecretPassword123
api_key = "my_secret_api_key_1234567890123456"
```

Press `Ctrl+Alt+S` - all secrets should be highlighted!

---

## Project Structure

```
SecretFinder/
├── SecretsFinder.sln          # Visual Studio solution
├── SecretsFinder.csproj       # Project file
├── Main.cs                    # Plugin entry point
├── Core/
│   ├── SecretPattern.cs       # Pattern definitions (25+ built-in)
│   ├── SecretMatch.cs         # Match result with obscuring
│   ├── SecretScanner.cs       # Scanning engine
│   └── IndicatorManager.cs    # Scintilla highlighting
├── Forms/
│   ├── ResultsPanel.cs        # Dockable results panel
│   └── AboutForm.cs           # About dialog
├── Utils/
│   ├── Settings.cs            # Plugin settings
│   └── Npp.cs                 # Notepad++ helpers
├── PluginInfrastructure/      # NPP plugin API
├── Properties/
│   └── AssemblyInfo.cs
└── Dependencies/              # DllExport
```

---

## Requirements

- **Visual Studio 2022** or later
- **.NET Framework 4.8** SDK
- **Notepad++** 8.0 or later (64-bit recommended)

---

## Troubleshooting

### Plugin doesn't appear in menu
- Make sure DLL is in correct folder: `plugins\SecretsFinder\SecretsFinder.dll`
- Check Notepad++ architecture matches plugin (x64 vs x86)
- Restart Notepad++ completely

### Build errors
- Ensure .NET Framework 4.8 SDK is installed
- Restore NuGet packages: Right-click solution → Restore NuGet Packages

### Secrets not detected
- Check if the pattern is enabled in Settings
- Verify the secret matches expected format
- Try the test file above

---

## License

MIT License
