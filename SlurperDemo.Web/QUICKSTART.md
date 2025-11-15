# Quick Start - NPM Build System

## First Time Setup

1. **Install dependencies:**

   ```powershell
   npm install
   ```

2. **Build client assets:**

   ```powershell
   npm run build
   ```

3. **Run the application:**

   ```powershell
   dotnet run
   ```

## Daily Development

**Watch mode (recommended):**

```powershell
.\build.ps1 -Mode watch
```

Then in another terminal:

```powershell
dotnet run
```

## Build Commands

```powershell
# PowerShell build script (recommended)
.\build.ps1                    # Production build
.\build.ps1 -Mode development  # Dev build
.\build.ps1 -Mode watch        # Watch mode
.\build.ps1 -Clean             # Clean rebuild

# npm commands
npm run build      # Production
npm run build:dev  # Development
npm run watch      # Watch mode
npm run serve      # Dev server
npm run clean      # Clean output
```

## What Gets Built

**Source files** (`src/`) → **Output** (`wwwroot/dist/`)

- `src/js/vendor.js` → `dist/vendor.min.js` (jQuery, Bootstrap, validation)
- `src/js/site.js` → `dist/site.min.js` (your app code)
- `src/scss/site.scss` → `dist/site.min.css` (your styles)

## Troubleshooting

**Build fails?**

```powershell
.\build.ps1 -Clean -Install
```

**Changes not showing?**

- Hard refresh browser (Ctrl+Shift+R)
- Check build completed successfully
- Verify watch mode is running

**Need help?** See [BUILD.md](./BUILD.md) for complete documentation.

## MSBuild Integration

The npm build runs automatically when you build the .NET project:

```powershell
dotnet build    # Runs npm build automatically
```

Disable if needed:

```powershell
dotnet build /p:NpmBuildEnabled=false
```

---

**New to this project?** This build system bundles all client libraries (Bootstrap, jQuery, Font Awesome) and your custom JavaScript/CSS into optimized files for better performance.
