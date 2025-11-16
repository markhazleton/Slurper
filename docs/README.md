# WebSpark.Slurper GitHub Pages Site

This directory contains the static website for the WebSpark.Slurper NuGet package, designed to be served via GitHub Pages.

## 🌐 Live Site

Once deployed, the site will be available at: `https://markhazleton.github.io/Slurper/`

## 📁 Structure

```
docs/
├── index.html       # Main landing page
├── styles.css       # All styling and animations
├── script.js        # Interactive functionality
├── icon.png         # Slurper logo
└── README.md        # This file
```

## 🚀 Features

### World-Class Design Elements

- **Modern, Responsive Layout**: Fully responsive design that works on all devices
- **Dark Theme**: Professional dark theme with gradient accents
- **Smooth Animations**: Subtle animations and transitions for enhanced UX
- **Syntax Highlighting**: Code examples with Prism.js highlighting
- **Interactive Tabs**: Browse different code examples easily
- **Copy-to-Clipboard**: One-click code copying functionality
- **Smooth Scrolling**: Enhanced navigation experience
- **SEO Optimized**: Meta tags and semantic HTML for search engines
- **Performance Optimized**: Fast loading with minimal dependencies

### Content Sections

1. **Hero Section**: Eye-catching introduction with live code example
2. **Features Grid**: 9 key features with icons and descriptions
3. **Quick Start Guide**: 3-step installation and usage guide
4. **Code Examples**: Interactive tabs showing XML, JSON, CSV, HTML, and DI examples
5. **Version History**: Timeline of releases with detailed change logs
6. **Framework Support**: Visual grid of supported frameworks
7. **Documentation Links**: Quick access to resources
8. **Call-to-Action**: Prominent NuGet installation buttons
9. **Footer**: Comprehensive links and information

## 🛠️ Setup for GitHub Pages

### Option 1: Using GitHub Settings (Recommended)

1. Push the `docs` folder to your repository
2. Go to your repository on GitHub
3. Navigate to Settings → Pages
4. Under "Source", select:
   - Branch: `main` (or your default branch)
   - Folder: `/docs`
5. Click "Save"
6. GitHub will automatically deploy your site

### Option 2: Using GitHub Actions

Create `.github/workflows/deploy-pages.yml`:

```yaml
name: Deploy GitHub Pages

on:
  push:
    branches: [ main ]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

jobs:
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v3
      
      - name: Setup Pages
        uses: actions/configure-pages@v3
      
      - name: Upload artifact
        uses: actions/upload-pages-artifact@v2
        with:
          path: './docs'
      
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v2
```

## 🔧 Local Development

To test the site locally:

### Using Python (Simple)

```bash
cd docs
python -m http.server 8000
```

Then open `http://localhost:8000` in your browser.

### Using Node.js (With live-server)

```bash
npm install -g live-server
cd docs
live-server
```

### Using .NET (For ASP.NET developers)

```bash
cd docs
dotnet tool install --global dotnet-serve
dotnet serve -p 8000
```

## 📝 Customization

### Updating Content

- **Version History**: Edit the `#versions` section in `index.html`
- **Features**: Modify the feature cards in the `#features` section
- **Code Examples**: Update code samples in the tabs section
- **Links**: Update GitHub, NuGet, and documentation links throughout

### Styling

- Colors and themes are defined in CSS variables at the top of `styles.css`
- Modify the `:root` section to change the color scheme
- All spacing uses consistent CSS variables for easy adjustment

### Adding New Sections

1. Add the HTML section in `index.html`
2. Add corresponding styles in `styles.css`
3. Update navigation links in the navbar
4. Add any required JavaScript in `script.js`

## 📦 Dependencies

The site uses CDN-hosted libraries:

- **Fonts**: Google Fonts (Inter, JetBrains Mono)
- **Icons**: Font Awesome 6.5.1
- **Syntax Highlighting**: Prism.js 1.29.0
- **Clipboard**: Clipboard.js 2.0.11

These are loaded from CDNs and require an internet connection to display properly.

## 🎨 Design Principles

- **Simplicity**: Clean, uncluttered design
- **Readability**: High contrast, readable typography
- **Performance**: Minimal JavaScript, optimized CSS
- **Accessibility**: Semantic HTML, keyboard navigation
- **Mobile-First**: Responsive design that works on all screen sizes

## 📊 Analytics (Optional)

To add Google Analytics, insert the tracking code in the `<head>` section of `index.html`:

```html
<!-- Google Analytics -->
<script async src="https://www.googletagmanager.com/gtag/js?id=GA_MEASUREMENT_ID"></script>
<script>
  window.dataLayer = window.dataLayer || [];
  function gtag(){dataLayer.push(arguments);}
  gtag('js', new Date());
  gtag('config', 'GA_MEASUREMENT_ID');
</script>
```

## 🔍 SEO Optimization

The site includes:

- Meta descriptions and keywords
- Open Graph tags for social sharing
- Semantic HTML5 structure
- Proper heading hierarchy
- Alt text for images
- Fast loading times

## 🐛 Troubleshooting

### Site not updating
- Clear your browser cache
- Wait a few minutes for GitHub Pages to rebuild
- Check the GitHub Actions tab for deployment status

### Styles not loading
- Verify all file paths are relative
- Check browser console for errors
- Ensure CDN resources are accessible

### Code highlighting not working
- Verify Prism.js is loading from CDN
- Check that code blocks use proper `language-*` classes
- Ensure JavaScript is enabled in the browser

## 📄 License

This site and the WebSpark.Slurper package are licensed under the MIT License.

## 🤝 Contributing

To improve this site:

1. Fork the repository
2. Make your changes in the `docs` folder
3. Test locally
4. Submit a pull request

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/MarkHazleton/Slurper/issues)
- **Discussions**: [GitHub Discussions](https://github.com/MarkHazleton/Slurper/discussions)
- **NuGet**: [Package Page](https://www.nuget.org/packages/WebSpark.Slurper)

---

Built with ❤️ for the .NET community
