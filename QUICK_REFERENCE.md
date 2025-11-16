# 🎯 Quick Reference Card - WebSpark.Slurper GitHub Pages

## 🚀 Deployment Commands

```bash
# Quick deploy
git add .
git commit -m "Update GitHub Pages site"
git push

# The site auto-deploys via GitHub Actions!
```

## 📝 Common Updates

### Update Version Number
**File**: `docs/index.html`
**Find**: `Version 3.3.0`
**Replace**: `Version X.Y.Z`
**Also update**: Timeline section with new version

### Add New Feature
**File**: `docs/index.html`
**Section**: `<div class="features-grid">`
**Template**:
```html
<div class="feature-card">
    <div class="feature-icon">
        <i class="fas fa-icon-name"></i>
    </div>
    <h3>Feature Name</h3>
    <p>Feature description here.</p>
</div>
```

### Add Code Example
**File**: `docs/index.html`
**Section**: `<div class="tabs">`
**Steps**:
1. Add button in `.tab-buttons`
2. Add pane in `.tab-content`

### Change Colors
**File**: `docs/styles.css`
**Section**: `:root { ... }`
**Variables**:
```css
--primary-color: #6366f1;
--secondary-color: #8b5cf6;
--accent-color: #ec4899;
```

## 🔧 Local Testing

```bash
# Python method (easiest)
cd docs
python -m http.server 8000
# Visit: http://localhost:8000

# PowerShell method
cd docs
Start-Process "index.html"
```

## 📊 Check Deployment Status

1. Go to: `https://github.com/MarkHazleton/Slurper/actions`
2. Latest workflow shows deployment status
3. Green ✅ = Success
4. Red ❌ = Failed (check logs)

## 🔗 Important URLs

| Resource | URL |
|----------|-----|
| **Live Site** | https://markhazleton.github.io/Slurper/ |
| **Repository** | https://github.com/MarkHazleton/Slurper |
| **NuGet** | https://www.nuget.org/packages/WebSpark.Slurper |
| **Actions** | https://github.com/MarkHazleton/Slurper/actions |
| **Settings** | https://github.com/MarkHazleton/Slurper/settings/pages |

## 🎨 Icon Classes (Font Awesome)

```html
<!-- Common icons used in the site -->
<i class="fas fa-download"></i>     <!-- Download -->
<i class="fas fa-rocket"></i>       <!-- Get Started -->
<i class="fas fa-code"></i>         <!-- Code -->
<i class="fas fa-book"></i>         <!-- Documentation -->
<i class="fab fa-github"></i>       <!-- GitHub -->
<i class="fas fa-cube"></i>         <!-- NuGet -->
<i class="fas fa-check-circle"></i> <!-- Success -->
<i class="fas fa-times-circle"></i> <!-- Error -->
```

## 📱 Responsive Breakpoints

```css
/* Mobile */
@media (max-width: 480px) { }

/* Tablet */
@media (max-width: 768px) { }

/* Desktop */
@media (max-width: 968px) { }
```

## 🐛 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| **Site not updating** | Clear cache (Ctrl+Shift+R), wait 2 mins |
| **Styles broken** | Check CDN links, browser console |
| **404 errors** | Verify file paths are relative |
| **Deployment failed** | Check Actions tab for error logs |
| **Mobile issues** | Test responsive breakpoints |

## 📋 File Structure

```
docs/
├── index.html      ← Main page
├── styles.css      ← All styles
├── script.js       ← Interactive features
├── icon.png        ← Logo
├── 404.html        ← Error page
└── README.md       ← Documentation
```

## 🎯 CDN Resources Used

```html
<!-- Fonts -->
Google Fonts: Inter, JetBrains Mono

<!-- Icons -->
Font Awesome 6.5.1

<!-- Syntax Highlighting -->
Prism.js 1.29.0

<!-- Clipboard -->
Clipboard.js 2.0.11
```

## 💡 Quick Tips

1. **Always test locally** before pushing
2. **Clear cache** when viewing updates
3. **Check mobile view** in DevTools (F12)
4. **Verify all links** after updates
5. **Monitor Actions** for deployment success
6. **Use relative paths** for all assets
7. **Keep backups** before major changes
8. **Document changes** in commit messages

## 🔑 Key Files to Know

| File | Purpose |
|------|---------|
| `docs/index.html` | Main content and structure |
| `docs/styles.css` | All styling and animations |
| `docs/script.js` | Interactive features |
| `.github/workflows/deploy-pages.yml` | Auto-deployment |
| `GITHUB_PAGES_SETUP.md` | Full setup guide |

## 📞 Get Help

- **Setup Issues**: Read `GITHUB_PAGES_SETUP.md`
- **Customization**: Check `docs/README.md`
- **Bugs**: Open GitHub issue
- **Questions**: Start a discussion

## ✅ Pre-Push Checklist

- [ ] Tested locally
- [ ] Links verified
- [ ] Mobile responsive checked
- [ ] No console errors
- [ ] Version numbers updated
- [ ] Commit message clear
- [ ] Ready to push!

---

**Keep this card handy for quick reference!** 📌

*Last Updated: November 2025*
