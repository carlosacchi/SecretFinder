# SecretsFinder - Notepad++ Plugin

A Notepad++ plugin that detects secrets, API keys, tokens, and other sensitive information in your documents.

[![Build and Release](https://github.com/carlosacchi/SecretFinder/actions/workflows/build-release.yml/badge.svg)](https://github.com/carlosacchi/SecretFinder/actions/workflows/build-release.yml)

---

## Download

**[Download Latest Release](https://github.com/carlosacchi/SecretFinder/releases/latest)**

- `SecretsFinder_x64.zip` - For 64-bit Notepad++ (most common)
- `SecretsFinder_x86.zip` - For 32-bit Notepad++

---

## Quick Start

### Step 1: Download

Go to [Releases](https://github.com/carlosacchi/SecretFinder/releases/latest) and download:
- **64-bit Notepad++**: `SecretsFinder_x64.zip`
- **32-bit Notepad++**: `SecretsFinder_x86.zip`

### Step 2: Install

1. Extract the ZIP file
2. Copy `SecretsFinder.dll` to your Notepad++ plugins folder:
   - **64-bit**: `C:\Program Files\Notepad++\plugins\SecretsFinder\`
   - **32-bit**: `C:\Program Files (x86)\Notepad++\plugins\SecretsFinder\`
3. **Restart Notepad++**

<div align="center">
  <img src="Resources/demo-images/SecretFinderInstall.png" alt="Installation" width="600">
  <p><em>Copy SecretsFinder.dll to your Notepad++ plugins folder</em></p>
</div>

### Step 3: Access the Menu

After restarting Notepad++, find **SecretsFinder** in the Plugins menu:

<div align="center">
  <img src="Resources/demo-images/SecretFinderMenu.png" alt="Menu" width="400">
  <p><em>Access SecretsFinder from the Plugins menu</em></p>
</div>

### Step 4: Scan for Secrets

1. Open any file in Notepad++
2. Press **`Ctrl+Alt+S`** or use **Plugins → SecretsFinder → Scan Current Document**
3. Found secrets are **highlighted in red** directly in your editor
4. The **Results Panel** opens automatically showing all findings

<div align="center">
  <img src="Resources/demo-images/SecretFinderResults.png" alt="Results Panel Overview" width="700">
  <p><em>Secrets highlighted in red with detailed results panel</em></p>
</div>

### Step 5: Review Results

The Results Panel shows comprehensive information for each detected secret:

<div align="center">
  <img src="Resources/demo-images/SecretFinderResults_0.png" alt="Result Details" width="700">
  <p><em>Each result shows file location, line number, secret type, and severity level</em></p>
</div>

**Double-click any result** to navigate directly to that secret in the editor:

<div align="center">
  <img src="Resources/demo-images/SecretFinderResults_1.png" alt="Navigation" width="700">
  <p><em>Click results to jump to the exact location in your files</em></p>
</div>

### Step 6: Scan Multiple Files

Use **Scan All Open Documents** to check all open tabs at once:

<div align="center">
  <img src="Resources/demo-images/SecretFinderResults_2.png" alt="Multiple Results" width="700">
  <p><em>Scan all open documents and see aggregated results across files</em></p>
</div>

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
| Azure Client Secret | `8Q~xxxxxxxxxxxxxxxxxxxxxxxxxxxx` |
| Azure GUID | `12345678-1234-1234-1234-123456789abc` |
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
| High Entropy String | `Random-looking 20+ char strings` (disabled by default) |

### Cryptocurrency Wallets
| Secret Type | Example Pattern |
|-------------|-----------------|
| Bitcoin Private Key | `5Kb8kLf9zgWQn...` (WIF format) |
| Bitcoin Address | `1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa` or `bc1q...` |
| Ethereum Private Key | `0x1234567890abcdef...` (64 hex chars) |
| Ethereum Address | `0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb` |
| Crypto Seed Phrase | `seed = "word1 word2 word3..."` (12-24 words) |
| Wallet Password | `wallet_password = "..."` |
| Litecoin Address | `LTC address starting with L, M, or 3` |
| Dogecoin Address | `DOGE address starting with D` |
| Ripple Secret Key | `sXXXXXXXXXXXXXXXXXXXXXXXXXXX` |
| Monero Address | `4XXXXXXXXXXXXXXX...` (95 chars) |

---

## Settings

Access via **Plugins → SecretsFinder → Settings**

### Scanning
- `auto_scan_on_open` - Automatically scan when files are opened
- `show_auto_scan_notification` - Show popup when secrets found
- `max_file_size_kb` - Skip files larger than this (0 = unlimited)
- `heuristic_filter_enabled` - Reduce false positives for simple passwords by combining entropy check and common-word filtering (English-focused)

### Patterns
Enable/disable individual pattern types:
- Cloud: AWS, Google, Azure (Storage, SAS, Client Secrets, GUIDs), Firebase, Heroku
- Services: GitHub, Stripe, Slack, Discord, Twilio, SendGrid, Mailchimp, NPM, NuGet
- Generic: JWT, Private Keys, Connection Strings, Bearer/Basic Auth, High Entropy Strings
- Crypto: Bitcoin (Private Keys, Addresses), Ethereum (Private Keys, Addresses), Litecoin, Dogecoin, Ripple, Monero, Seed Phrases, Wallet Passwords

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
# Test secrets - these are fake examples for testing
aws_key = AKIAIOSFODNN7EXAMPLE
github_token = ghp_1234567890abcdefghijklmnopqrstuvwxyz
stripe_key = sk_live_abcdefghijklmnopqrstuvwx
azure_secret = 8Q~abcdefghijklmnopqrstuvwxyz1234567890
azure_client_id = 12345678-abcd-1234-abcd-123456789abc
jwt = eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U
password = SuperSecretPassword123
api_key = "my_secret_api_key_1234567890123456"

# Cryptocurrency examples
bitcoin_address = 1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa
ethereum_address = 0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb
bitcoin_private_key = 5KYZdUEo39z3FPrtuX2QbbwGnNP5zTd7yyr2SC1j299sBCnWjss
ethereum_private_key = 0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80
wallet_password = MyWalletPassword123!
seed_phrase = abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon about
```

Press `Ctrl+Alt+S` - all secrets should be highlighted!

---

## Building from Source

### Requirements
- Visual Studio 2022 or later
- .NET Framework 4.8 SDK

### Build Steps

1. Clone the repository
2. Open `SecretsFinder.sln` in Visual Studio
3. Select configuration:
   - `Release | x64` for 64-bit
   - `Release | x86` for 32-bit
4. Build: `Ctrl+Shift+B`

Output:
- x64: `bin\Release-x64\SecretsFinder.dll`
- x86: `bin\Release\SecretsFinder.dll`

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
└── .github/workflows/         # CI/CD automation
```

---

## Troubleshooting

> Disclaimer: SecretsFinder helps surface likely secrets, but no automated scanner can guarantee 100% detection or zero false positives. Always combine automated scans with manual review and your organization’s security processes.

### Plugin doesn't appear in menu
- Make sure DLL is in correct folder: `plugins\SecretsFinder\SecretsFinder.dll`
- Check Notepad++ architecture matches plugin (x64 vs x86)
- Restart Notepad++ completely

### Secrets not detected
- Check if the pattern is enabled in Settings
- Verify the secret matches expected format
- Try the test file above

---

## License

MIT License
