# Managing NuGet Packages: Best Practices

This document outlines best practices for managing NuGet packages, including versioning, continuous integration/continuous deployment (CI/CD) with GitHub Actions, and publishing to NuGet.org.

## Table of Contents

- [Setting Up GitHub Actions for NuGet Publishing](#setting-up-github-actions-for-nuget-publishing)
- [Automated Versioning with GitVersion](#automated-versioning-with-gitversion)
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
│       └── build-and-test.yml
├── src
└── ...
```

### Sample Workflow

A comprehensive workflow for building, testing, and publishing a NuGet package:

```yaml
name: Build, Test, and Publish

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0  # Required for GitVersion to work properly

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x

      # Use GitVersion for automated versioning
      - name: Install GitVersion
        uses: gittools/actions/gitversion/setup@v0.10.2
        with:
          versionSpec: "5.x"

      - name: Determine Version
        uses: gittools/actions/gitversion/execute@v0.10.2
        id: gitversion
        with:
          useConfigFile: true
          configFilePath: GitVersion.json

      - name: Restore dependencies
        run: dotnet restore YourProject/YourProject.csproj

      - name: Build with version
        run: dotnet build YourProject/YourProject.csproj --configuration Release -p:Version=${{ steps.gitversion.outputs.nuGetVersion }}

      - name: Test
        run: dotnet test YourProject.Tests/YourProject.Tests.csproj --configuration Release --verbosity normal

      - name: Pack with version
        run: dotnet pack YourProject/YourProject.csproj --configuration Release -p:Version=${{ steps.gitversion.outputs.nuGetVersion }} -p:PackageVersion=${{ steps.gitversion.outputs.nuGetVersion }}

      # Only publish to NuGet when pushing to main (not on PR)
      - name: Publish to NuGet
        if: github.event_name == 'push' && github.ref == 'refs/heads/main'
        run: |
          find YourProject/bin/Release -name "*.nupkg" -type f | xargs -I {} dotnet nuget push {} --api-key ${{ secrets.NUGET_API_KEY }} --source "https://api.nuget.org/v3/index.json" --skip-duplicate
```

## Automated Versioning with GitVersion

GitVersion is a powerful tool that determines a semantic version number based on your Git history.

### GitVersion Setup

1. Create a `GitVersion.json` file at the root of your repository:

```json
{
  "mode": "ContinuousDelivery",
  "next-version": "3.1.1",
  "major-version-bump-message": "^(breaking|major):",
  "minor-version-bump-message": "^(feature|minor):",
  "patch-version-bump-message": "^(fix|patch):",
  "no-bump-message": "^(none|skip):",
  "legacy-semver-padding": 0,
  "build-metadata-padding": 0,
  "commits-since-version-source-padding": 0,
  "tag-pre-release-weight": 0,
  "increment": "Patch",
  "continuous-delivery-fallback-tag": "ci",
  "tag-prefix": "v",
  "branches": {
    "main": {
      "regex": "^master$|^main$",
      "tag": "",
      "increment": "Patch",
      "prevent-increment-of-merged-branch-version": true,
      "track-merge-target": false,
      "tracks-release-branches": false,
      "is-release-branch": true
    },
    "develop": {
      "regex": "^dev(elop)?(ment)?$",
      "tag": "beta",
      "increment": "Minor",
      "prevent-increment-of-merged-branch-version": false,
      "track-merge-target": true,
      "tracks-release-branches": true,
      "is-release-branch": false
    },
    "feature": {
      "regex": "^features?[/-]",
      "tag": "alpha.{BranchName}",
      "increment": "Inherit",
      "prevent-increment-of-merged-branch-version": false,
      "track-merge-target": false,
      "tracks-release-branches": false,
      "is-release-branch": false
    },
    "release": {
      "regex": "^releases?[/-]",
      "tag": "rc",
      "increment": "None",
      "prevent-increment-of-merged-branch-version": true,
      "track-merge-target": false,
      "tracks-release-branches": false,
      "is-release-branch": true
    },
    "hotfix": {
      "regex": "^hotfix(es)?[/-]",
      "tag": "hotfix",
      "increment": "Patch",
      "prevent-increment-of-merged-branch-version": false,
      "track-merge-target": false,
      "tracks-release-branches": false,
      "is-release-branch": false
    }
  },
  "ignore": {
    "sha": []
  }
}
```

2. Remove hardcoded version numbers from your `.csproj` file.

### Semantic Versioning with Commit Messages

GitVersion can automatically update your version based on commit messages:

- **Patch version** (3.1.0 → 3.1.1): Start commit messages with `fix:` or `patch:`
- **Minor version** (3.1.0 → 3.2.0): Start commit messages with `feature:` or `minor:`
- **Major version** (3.1.0 → 4.0.0): Start commit messages with `breaking:` or `major:`

Example:

```
fix: Resolve issue with XML parsing
```

### Manual Version Control

For explicit version control, update the `next-version` in your GitVersion.json file:

```json
"next-version": "3.1.1",
```

### Git Tags for Version Control

After a successful release, create a git tag to mark the version:

```
git tag v3.1.1
git push --tags
```

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
2. Use conventional commit messages (`fix:`, `feature:`, `breaking:`)
3. Create a pull request to merge into main
4. After approval and merge, GitHub Actions will:
   - Build the project
   - Run all tests
   - Determine the version number with GitVersion
   - Create the NuGet package
   - Publish to NuGet.org

### Manual Release Steps (if needed)

1. Update GitVersion.json with the new version number
2. Commit and push the change
3. Create a tag for the release:

   ```
   git tag v3.1.1
   git push --tags
   ```

4. Watch the GitHub Actions workflow to ensure successful publishing

## Common Issues and Solutions

### Versioning Issues

**Problem**: Version not incrementing automatically.
**Solution**: Ensure commits follow the conventional format or manually update the `next-version` in GitVersion.json.

**Problem**: "Package already exists" error.
**Solution**: Ensure your version number is incremented before publishing again.

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

- [ ] Use GitVersion for automated versioning
- [ ] Follow semantic versioning principles
- [ ] Use conventional commit messages
- [ ] Create Git tags for released versions

### CI/CD Workflow

- [ ] Place workflow files at repository root
- [ ] Build in Release configuration
- [ ] Run comprehensive tests
- [ ] Use GitHub Secrets for API keys
- [ ] Only publish on main branch updates
- [ ] Include verbose logging for debugging

### Quality Assurance

- [ ] Maintain high test coverage
- [ ] Include IntelliSense-friendly documentation
- [ ] Validate package before publishing with `dotnet pack --no-build`
- [ ] Test installation in a clean project

---

By following these best practices, you'll have a reliable, automated pipeline for building, versioning, and publishing your NuGet packages.
