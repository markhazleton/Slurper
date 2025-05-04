# Managing NuGet Packages: Best Practices

This document outlines best practices for managing NuGet packages, including versioning, continuous integration/continuous deployment (CI/CD) with GitHub Actions, and publishing to NuGet.org.

## Table of Contents

- [Setting Up GitHub Actions for NuGet Publishing](#setting-up-github-actions-for-nuget-publishing)
- [Version Management with Project Files](#version-management-with-project-files)
- [Secure API Key Management](#secure-api-key-management)
- [Release Process](#release-process)
- [Common Issues and Solutions](#common-issues-and-solutions)
- [Best Practices Checklist](#best-practices-checklist)

## Setting Up GitHub Actions for NuGet Publishing

GitHub Actions provides a powerful platform for automating your NuGet package build, test, and publishing process.

### Workflow File Structure

Place your GitHub workflow files in the `.github/workflows` directory at the **root level** of your repository:

```
Repository Root
├── .github
│   └── workflows
│       └── slurper-ci-cd.yml
├── src
└── ...
```

### Sample Workflow

Our current workflow for building, testing, and publishing a NuGet package:

```yaml
name: WebSpark.Slurper CI/CD

on:
  push:
    branches: [main]
    tags: ["v*"]
  pull_request:
    branches: [main]
  workflow_dispatch:
    inputs:
      forceVersion:
        description: "Force version (leave empty to use version from csproj)"
        required: false
        type: string
      publishPackages:
        description: "Publish packages to NuGet.org and GitHub Packages"
        type: boolean
        default: false
        required: false

# Add permissions block for better security
permissions:
  contents: read
  packages: write  # Needed for GitHub Packages

jobs:
  build-and-test:
    name: Build and Test
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Get all history for proper versioning

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "9.0.x"
          cache: 'nuget'  # Add caching for better performance

      - name: Extract version from csproj
        id: extract_version
        run: |
          VERSION=$(grep -oP '<Version>\K[^<]+' WebSpark.Slurper/WebSpark.Slurper.csproj)
          echo "VERSION=$VERSION" >> $GITHUB_ENV
          echo "Extracted version: $VERSION"

      # ... more steps ...
```

## Version Management with Project Files

We manage our package versioning directly in the project file, which is simpler and more straightforward than using external versioning tools.

### Setting Version in .csproj

The version is defined in the .csproj file with the `<Version>` tag:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Version>3.1.4</Version>
    <!-- Other properties -->
  </PropertyGroup>
  <!-- Rest of the project file -->
</Project>
```

### Semantic Versioning Principles

We follow semantic versioning (SemVer) principles:

- **Patch version** (3.1.0 → 3.1.1): For backward-compatible bug fixes
- **Minor version** (3.1.0 → 3.2.0): For backward-compatible new features
- **Major version** (3.1.0 → 4.0.0): For breaking changes

### Manual Version Updates

To update the version:

1. Edit the `<Version>` element in the .csproj file
2. Commit and push the change
3. The CI/CD pipeline will use this version for building and packaging

### Git Tags for Version Control

After a successful release, create a git tag to mark the version:

```
git tag v3.1.4
git push --tags
```

This will trigger the release workflow in GitHub Actions.

## Secure API Key Management

### Creating a NuGet API Key

1. Log in to [NuGet.org](https://www.nuget.org/)
2. Go to your account settings
3. Click on "API Keys"
4. Create a new API key with appropriate permissions
5. Copy the API key for later use

### Adding the API Key to GitHub Secrets

1. Go to your GitHub repository
2. Navigate to Settings > Secrets and variables > Actions
3. Click "New repository secret"
4. Name: `NUGET_API_KEY`
5. Value: Paste your NuGet API key
6. Click "Add secret"

### Security Best Practices

- Never store API keys in code or commit them to your repository
- Rotate your API keys periodically
- Use the principle of least privilege when setting API key permissions
- If a key is compromised, immediately revoke it and generate a new one

## Release Process

### Automated Release Process

1. Make code changes in a feature or fix branch
2. Update the version in the .csproj file if needed
3. Create a pull request to merge into main
4. After approval and merge, GitHub Actions will:
   - Build the project
   - Run all tests
   - Extract the version from the .csproj file
   - Create the NuGet package
   - Publish to NuGet.org and GitHub Packages

### Manual Release Steps (if needed)

1. Update the version in the .csproj file
2. Commit and push the change
3. Create a tag for the release:

   ```
   git tag v3.1.4
   git push --tags
   ```

4. Watch the GitHub Actions workflow to ensure successful publishing
5. Alternatively, use the workflow_dispatch trigger with the "publishPackages" option set to true

## Common Issues and Solutions

### Versioning Issues

**Problem**: "Package already exists" error.
**Solution**: Ensure your version number is incremented in the .csproj file before publishing again.

**Problem**: Version not updating in the built package.
**Solution**: Make sure the version is correctly set in the .csproj file and the CI/CD is extracting it properly.

### GitHub Actions Issues

**Problem**: "Source parameter was not specified" error.
**Solution**: Ensure the NuGet push command has proper quotes around the source URL:

```
dotnet nuget push ... --source "https://api.nuget.org/v3/index.json" --skip-duplicate
```

**Problem**: Files not found for packaging.
**Solution**: Use specific project paths and proper build configuration:

```
dotnet build YourProject/YourProject.csproj --configuration Release
```

### NuGet Publishing Issues

**Problem**: API key authentication failure.
**Solution**: Verify the API key is valid and has the correct permissions. Regenerate if necessary.

**Problem**: Package validation errors.
**Solution**: Check the NuGet package requirements, including required metadata in your .csproj file.

## Best Practices Checklist

### Project Structure

- [ ] Organize code in logical namespaces
- [ ] Include README.md with clear documentation
- [ ] Include license information
- [ ] Add package icon if desired

### .csproj Configuration

- [ ] Include comprehensive package metadata:
  - Authors
  - Description
  - License expression or file
  - Project URL
  - Repository URL
  - Tags
- [ ] Generate XML documentation file
- [ ] Include symbols package for debugging
- [ ] Specify supported frameworks

### Version Management

- [ ] Define version in the .csproj file
- [ ] Follow semantic versioning principles
- [ ] Create Git tags for released versions
- [ ] Consider using descriptive commit messages to track changes

### CI/CD Workflow

- [ ] Place workflow files at repository root
- [ ] Build in Release configuration
- [ ] Run comprehensive tests
- [ ] Use GitHub Secrets for API keys
- [ ] Only publish on main branch updates or tags
- [ ] Include verbose logging for debugging

### Quality Assurance

- [ ] Maintain high test coverage
- [ ] Include IntelliSense-friendly documentation
- [ ] Validate package before publishing with `dotnet pack --no-build`
- [ ] Test installation in a clean project

## NuGet Package Publishing Timeline

After publishing your package to NuGet.org, it takes time to become fully available:

- **Initial Processing**: 1-10 minutes for submission and validation
- **Indexing and Search**: 15-30 minutes to appear in search results
- **Complete CDN Propagation**: Up to 2 hours for worldwide availability

### Understanding the Process

1. **Validation Phase**
   - Package undergoes virus scanning
   - Metadata and integrity checks are performed
   - License verification takes place

2. **Indexing Phase**
   - Package enters the NuGet.org search index
   - Becomes discoverable through search and package manager UI

3. **Distribution Phase**
   - Package is distributed to global CDN endpoints
   - Becomes available for download worldwide

### Monitoring Package Status

- Check your package status on the NuGet.org account dashboard
- Look for the "Validating" status immediately after upload
- Refresh the gallery periodically to see when your package appears

### Troubleshooting Publishing Issues

- If your package doesn't appear after 2 hours, check for validation errors
- Review package details for compliance issues
- First-time publishers may experience additional verification delays
- High traffic periods on NuGet.org can extend processing times

---

By following these best practices, you'll have a reliable, automated pipeline for building, versioning, and publishing your NuGet packages.
