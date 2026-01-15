# SecretsFinder - Notepad++ Plugin

A Notepad++ plugin that detects secrets, API keys, tokens, and other sensitive information in your documents.

## Features

- **Scan Current Document** - Detect secrets in the active document (Ctrl+Alt+S)
- **Scan All Open Documents** - Check all open files for secrets
- **Scan Backup Folder** - Check the Notepad++ backup folder for exposed secrets
- **Visual Highlighting** - Found secrets are highlighted in the editor
- **Results Panel** - Dockable panel showing all detected secrets with navigation
- **Export Results** - Export findings to CSV or JSON
- **Configurable Patterns** - Enable/disable built-in patterns and add custom ones

## Detected Secrets

### Cloud Providers
- AWS Access Keys (AKIA...)
- AWS Secret Keys
- Google API Keys (AIza...)
- Azure Storage Connection Strings
- Azure SAS Tokens
- Firebase URLs
- Heroku API Keys

### Services
- GitHub Tokens (ghp_, gho_, ghu_, ghs_, ghr_)
- Stripe Keys (sk_live_, rk_live_)
- Slack Tokens and Webhooks
- Discord Webhooks
- Twilio API Keys
- SendGrid API Keys
- Mailchimp API Keys
- NPM Tokens
- NuGet API Keys

### Generic Patterns
- JWT Tokens
- Private Key Headers
- Connection String Passwords
- Bearer Tokens
- Basic Auth Credentials
- Generic API Key assignments

## Building

### Requirements
- Visual Studio 2022 or later
- .NET Framework 4.8 SDK

### Build Steps

1. Open `SecretsFinder.sln` in Visual Studio
2. Select configuration:
   - `Release|x64` for 64-bit Notepad++
   - `Release|x86` for 32-bit Notepad++
3. Build the solution (Ctrl+Shift+B)

The DLL will be output to:
- x64: `bin\Release-x64\SecretsFinder.dll`
- x86: `bin\Release\SecretsFinder.dll`

### Installation

1. Copy `SecretsFinder.dll` to:
   - 64-bit: `C:\Program Files\Notepad++\plugins\SecretsFinder\`
   - 32-bit: `C:\Program Files (x86)\Notepad++\plugins\SecretsFinder\`
2. Restart Notepad++
3. The plugin will appear under **Plugins → SecretsFinder**

## Usage

1. Open a document containing potential secrets
2. Press **Ctrl+Alt+S** or go to **Plugins → SecretsFinder → Scan Current Document**
3. Found secrets will be highlighted and listed in the Results Panel
4. Double-click any result to navigate to it
5. Right-click for options to copy or dismiss findings

## Settings

Access via **Plugins → SecretsFinder → Settings**

- **Scanning**: Auto-scan on file open, notifications
- **Patterns**: Enable/disable individual pattern types
- **Display**: Highlight color, toolbar icon visibility
- **Custom Patterns**: Add your own regex patterns

### Custom Pattern Format
```
PatternName|RegexPattern|Severity
```
Severity: Low, Medium, High, Critical

Example:
```
MyCompanyKey|MYCO_[A-Z0-9]{32}|High
```

## License

MIT License
