# Modern NPM Build System for Slurper Demo Web

## Overview

This project uses a modern npm-based build system with webpack to manage and bundle client-side assets. All JavaScript and CSS dependencies are managed through npm and bundled for optimal performance.

## Prerequisites

### Required Software

- **Node.js** v18.0.0 or higher
- **npm** v9.0.0 or higher
- **.NET 10.0 SDK**

### Verify Installation

```powershell
# Check Node.js version
node --version

# Check npm version
npm --version

# Check .NET version
dotnet --version
```

## Project Structure

```
SlurperDemo.Web/
├── package.json              # npm dependencies and scripts
├── webpack.config.js         # Webpack configuration
├── build.ps1                 # PowerShell build script
├── src/                      # Source files (not deployed)
│   ├── js/
│   │   ├── site.js          # Main application JavaScript
│   │   └── vendor.js        # Third-party libraries entry point
│   └── scss/
│       ├── site.scss        # Main styles
│       ├── _variables.scss  # SCSS variables
│       └── _mixins.scss     # SCSS mixins
├── wwwroot/
│   ├── dist/                # Build output (generated, not committed)
│   │   ├── vendor.min.js    # Bundled third-party JavaScript
│   │   ├── vendor.min.css   # Bundled third-party CSS
│   │   ├── site.min.js      # Bundled application JavaScript
│   │   └── site.min.css     # Bundled application CSS
│   ├── css/                 # Legacy CSS (can be removed)
│   ├── js/                  # Legacy JS (can be removed)
│   └── lib/                 # Old client libraries (not used)
└── Views/
    └── Shared/
        └── _Layout.cshtml   # References bundled assets
```

## Getting Started

### Initial Setup

1. **Install npm packages:**

```powershell
npm install
```

This will download all dependencies defined in `package.json` to the `node_modules/` folder.

2. **Build client assets:**

```powershell
# Production build (minified)
npm run build

# Development build (with source maps)
npm run build:dev
```

### Using the PowerShell Build Script

The `build.ps1` script provides a convenient way to build assets:

```powershell
# Production build
.\build.ps1

# Development build
.\build.ps1 -Mode development

# Clean and rebuild
.\build.ps1 -Clean

# Watch mode (auto-rebuild on file changes)
.\build.ps1 -Mode watch

# Force reinstall of packages
.\build.ps1 -Install
```

## Development Workflow

### Watch Mode

For active development, use watch mode to automatically rebuild when files change:

```powershell
npm run watch
```

or

```powershell
.\build.ps1 -Mode watch
```

This monitors changes in `src/` directory and rebuilds automatically.

### Development Server

Run a webpack dev server with hot module replacement:

```powershell
npm run serve
```

This starts a development server on `http://localhost:9000` with live reload.

### Running the .NET Application

```powershell
# Run the .NET application
dotnet run

# Or use Visual Studio
# Press F5 or click "Start Debugging"
```

The application will be available at `http://localhost:5000` (or the port specified in `launchSettings.json`).

## npm Scripts

Available scripts in `package.json`:

| Script | Command | Description |
|--------|---------|-------------|
| `build` | `npm run build` | Production build (minified, optimized) |
| `build:dev` | `npm run build:dev` | Development build (source maps included) |
| `watch` | `npm run watch` | Watch mode (auto-rebuild on changes) |
| `serve` | `npm run serve` | Start webpack dev server |
| `clean` | `npm run clean` | Remove build output |

## MSBuild Integration

The build system is integrated with MSBuild. When you build the .NET project:

1. **npm install** runs automatically if `node_modules/` doesn't exist
2. **npm build** runs automatically before compilation
3. Build artifacts are placed in `wwwroot/dist/`

### Control MSBuild Integration

You can disable npm build during MSBuild:

```powershell
dotnet build /p:NpmBuildEnabled=false
```

### Build Targets

The `.csproj` file includes these custom targets:

- **NpmInstall** - Installs packages before build if needed
- **NpmBuild** - Runs webpack build before compilation
- **NpmClean** - Cleans build output when project is cleaned

## Dependencies

### Production Dependencies

Installed in production builds:

- **bootstrap** ^5.3.3 - UI framework
- **jquery** ^3.7.1 - JavaScript library
- **jquery-validation** ^1.21.0 - Form validation
- **jquery-validation-unobtrusive** ^4.0.0 - ASP.NET validation
- **@fortawesome/fontawesome-free** ^6.7.2 - Icon library

### Development Dependencies

Used only during build:

- **webpack** ^5.97.1 - Module bundler
- **webpack-cli** ^6.0.1 - Webpack command line
- **webpack-dev-server** ^5.2.0 - Development server
- **babel-loader** ^9.2.1 - JavaScript transpiler
- **sass** ^1.83.0 - CSS preprocessor
- **sass-loader** ^16.0.3 - SASS loader for webpack
- **css-loader** ^7.1.2 - CSS loader
- **mini-css-extract-plugin** ^2.9.2 - CSS extraction
- **css-minimizer-webpack-plugin** ^7.0.0 - CSS minification
- **terser-webpack-plugin** ^5.3.11 - JavaScript minification

## Webpack Configuration

The `webpack.config.js` file is configured for:

### Entry Points

- **vendor.js** - Third-party libraries (Bootstrap, jQuery, Font Awesome)
- **site.js** - Application code and styles

### Output

- **Production** - Minified files with `.min.js` and `.min.css` extensions
- **Development** - Unminified with source maps

### Loaders

- **Babel** - Transpiles modern JavaScript to ES5
- **SASS** - Compiles SCSS to CSS
- **CSS** - Processes CSS files
- **Assets** - Handles fonts and images

### Optimization

- **Code Splitting** - Separates vendor and application code
- **Minification** - Reduces file sizes in production
- **Source Maps** - Debugging support in development

## Source Files

### JavaScript

**src/js/vendor.js**

- Imports all third-party libraries
- Makes jQuery globally available for ASP.NET validation
- Entry point for vendor bundle

**src/js/site.js**

- Main application JavaScript
- Contains all custom functionality:
  - Achievement tracking system
  - Data table enhancements
  - Form validation
  - Smooth scrolling
  - Toast notifications
  - Loading spinner utility

### SCSS

**src/scss/site.scss**

- Main stylesheet
- Imports variables and mixins
- Contains all application styles
- Includes animations and responsive design

**src/scss/_variables.scss**

- Color scheme
- Spacing values
- Typography settings
- Border styles
- Box shadows
- Z-index layers

**src/scss/_mixins.scss**

- Responsive breakpoints
- Flexbox utilities
- Text truncation
- Card shadows
- Reusable style patterns

## Build Output

After building, these files are generated in `wwwroot/dist/`:

### Production Build

```
wwwroot/dist/
├── vendor.min.js       # ~150KB - jQuery, Bootstrap, validation
├── vendor.min.js.map   # Source map for vendor.min.js
├── vendor.min.css      # ~200KB - Bootstrap, Font Awesome
├── vendor.min.css.map  # Source map for vendor.min.css
├── site.min.js         # ~15KB - Application code
├── site.min.js.map     # Source map for site.min.js
├── site.min.css        # ~5KB - Application styles
└── site.min.css.map    # Source map for site.min.css
```

### Development Build

Same files without `.min` in filename, larger sizes with readable code.

## Layout Integration

The `_Layout.cshtml` file references bundled assets:

```html
<!-- CSS -->
<link rel="stylesheet" href="~/dist/vendor.min.css" asp-append-version="true" />
<link rel="stylesheet" href="~/dist/site.min.css" asp-append-version="true" />

<!-- JavaScript -->
<script src="~/dist/vendor.min.js" asp-append-version="true"></script>
<script src="~/dist/site.min.js" asp-append-version="true"></script>
```

The `asp-append-version="true"` tag helper adds cache-busting query strings.

## Customization

### Adding New JavaScript

1. Add your code to `src/js/site.js`
2. Or create a new file and import it in `site.js`:

```javascript
import './modules/my-feature.js';
```

3. Rebuild:

```powershell
npm run build
```

### Adding New Styles

1. Add styles to `src/scss/site.scss`
2. Or create a new SCSS file and import it:

```scss
@import './components/my-component';
```

3. Rebuild:

```powershell
npm run build
```

### Adding npm Packages

1. Install the package:

```powershell
npm install package-name
```

2. Import in your JavaScript:

```javascript
import packageName from 'package-name';
```

3. Rebuild:

```powershell
npm run build
```

## Troubleshooting

### Build Fails

**Problem:** `npm install` fails

**Solution:**

```powershell
# Clear npm cache
npm cache clean --force

# Delete node_modules and reinstall
Remove-Item -Recurse -Force node_modules
npm install
```

**Problem:** Webpack build fails

**Solution:**

```powershell
# Check for syntax errors in src/ files
# Review webpack output for specific error
# Ensure all imports are correct
```

### Assets Not Loading

**Problem:** 404 errors for dist files

**Solution:**

```powershell
# Ensure build has run
npm run build

# Check that wwwroot/dist/ contains files
# Verify _Layout.cshtml has correct paths
```

**Problem:** Changes not appearing

**Solution:**

```powershell
# Clear browser cache (Ctrl+Shift+R)
# Hard refresh the page
# Check that files were rebuilt (check timestamps)
```

### MSBuild Issues

**Problem:** Build fails during npm step

**Solution:**

```powershell
# Build without npm integration
dotnet build /p:NpmBuildEnabled=false

# Then manually run npm build
npm run build

# Or use the build script
.\build.ps1
```

## Performance Optimization

### Production Checklist

- [x] Minification enabled (production mode)
- [x] Source maps generated separately
- [x] Vendor code split from application code
- [x] Tree shaking removes unused code
- [x] Console logs removed in production
- [x] CSS minimized
- [x] Asset optimization

### Cache Busting

ASP.NET Core's `asp-append-version` tag helper automatically adds version query strings:

```html
<script src="~/dist/site.min.js?v=abc123"></script>
```

This ensures browsers download new versions when files change.

## Migration from Old System

### Old System (lib folder)

```
wwwroot/lib/
├── bootstrap/
├── jquery/
├── jquery-validation/
└── jquery-validation-unobtrusive/
```

### New System (npm + webpack)

All dependencies managed through npm, bundled into optimized files.

### Benefits

1. **Dependency Management** - npm handles versions and updates
2. **Bundle Optimization** - Smaller file sizes, faster loading
3. **Modern JavaScript** - Use ES6+ features, transpiled for compatibility
4. **SCSS Support** - Advanced CSS with variables and mixins
5. **Development Tools** - Hot reload, source maps, dev server
6. **Tree Shaking** - Remove unused code automatically
7. **Code Splitting** - Separate vendor and app code

### Removing Old Files

After verifying the new system works:

```powershell
# Remove old client libraries (optional - already excluded from build)
Remove-Item -Recurse wwwroot/lib

# Remove old CSS (now in src/scss/)
Remove-Item wwwroot/css/site.css

# Remove old JS (now in src/js/)
Remove-Item wwwroot/js/site.js
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Build and Test

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
        cache: 'npm'
        cache-dependency-path: SlurperDemo.Web/package-lock.json
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Install npm dependencies
      working-directory: SlurperDemo.Web
      run: npm ci
    
    - name: Build client assets
      working-directory: SlurperDemo.Web
      run: npm run build
    
    - name: Build .NET application
      run: dotnet build --configuration Release
    
    - name: Run tests
      run: dotnet test --configuration Release
```

## Best Practices

### Development

1. **Use watch mode** during active development
2. **Commit source files** (`src/`, `package.json`, `webpack.config.js`)
3. **Don't commit** `node_modules/` or `wwwroot/dist/`
4. **Test production build** before deploying
5. **Keep dependencies updated** regularly

### Production

1. **Always use production build** (`npm run build`)
2. **Verify bundle sizes** are reasonable
3. **Test in target browsers** after building
4. **Monitor bundle performance** in production
5. **Use CDN** for additional performance (future enhancement)

## Future Enhancements

Potential improvements to consider:

- [ ] TypeScript support
- [ ] CSS/JS code splitting by route
- [ ] Service Worker for offline support
- [ ] Critical CSS inlining
- [ ] Image optimization pipeline
- [ ] Bundle analysis and reporting
- [ ] PostCSS with autoprefixer
- [ ] ESLint for code quality
- [ ] Prettier for code formatting
- [ ] Jest for JavaScript testing

## Resources

- [Webpack Documentation](https://webpack.js.org/)
- [Bootstrap 5 Documentation](https://getbootstrap.com/)
- [jQuery Documentation](https://jquery.com/)
- [SASS Documentation](https://sass-lang.com/)
- [npm Documentation](https://docs.npmjs.com/)

## Support

For issues or questions:

1. Check this documentation
2. Review webpack output for errors
3. Check browser console for runtime errors
4. Review the [DAWPM instructions](./README.md) for context

## Summary

This modern build system provides:

✅ **Optimized Performance** - Minified, bundled assets  
✅ **Modern Development** - ES6+, SCSS, hot reload  
✅ **Dependency Management** - npm package ecosystem  
✅ **Build Automation** - Integrated with MSBuild  
✅ **Developer Experience** - Source maps, watch mode, dev server  
✅ **Production Ready** - Optimized for deployment  

The build system seamlessly integrates with the existing .NET application while providing modern client-side development capabilities.
