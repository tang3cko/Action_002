# Web Publish Tools

## Purpose

Unity editor extension that validates and applies WebGL build settings for web publishing platforms. Supports a profile-based workflow: pick the target platform (e.g. `unityroom`) from a dropdown, and each setting card shows whether your project matches the platform's requirements with a one-click apply button.

---

## Features

- Profile selector for switching platform requirements (extensible to itch.io and others)
- Per-setting cards with current value, expected value, and individual apply buttons
- Resolution preset dropdown (common landscape / portrait / square sizes)
- No "Apply All" — each setting is opt-in, so you can intentionally diverge from the preset
- UI Toolkit (UXML / USS) based, BEM-style design tokens

---

## Supported Profiles

| Profile | Status |
|---------|--------|
| unityroom | Supported |
| itch.io   | Planned |

---

## Installation

### Unity Package Manager (Git URL)

1. Open `Window > Package Manager`
2. Click the `+` button and select `Add package from git URL...`
3. Enter the following URL:

```
https://github.com/tang3cko/WebPublishTools.git?path=Packages/com.tang3cko.web-publish-tools
```

### Edit manifest.json directly

Add the following to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.tang3cko.web-publish-tools": "https://github.com/tang3cko/WebPublishTools.git?path=Packages/com.tang3cko.web-publish-tools"
  }
}
```

---

## Usage

Open the window from `Window > Web Publish Tools`.

1. Select a profile from the dropdown in the top-right of the toolbar
2. Each card displays the current value, the expected value for the active profile, and a status icon
3. Click the inline apply button on any card to bring that setting in line with the profile

Cards with no expected value for the active profile are shown as informational only (no apply button).

| Card | Description |
|------|-------------|
| Build Target | Switch to WebGL platform |
| Compression Format | Set Player Settings WebGL compression (Gzip / Brotli / Disabled) |
| Decompression Fallback | Toggle the decompression fallback option |
| Development Build | Toggle EditorUserBuildSettings development flag |
| Data Caching | Toggle WebGL data caching |
| Exception Support | Set WebGL exception support level |
| Managed Stripping Level | Set the managed stripping level for WebGL |
| Resolution | Apply a preset canvas size from a dropdown |

---

## Requirements

- Unity 6.0 (6000.0) or later

---

## License

MIT
