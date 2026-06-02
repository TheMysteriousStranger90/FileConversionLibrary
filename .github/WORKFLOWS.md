# GitHub Actions Workflows

This repository uses GitHub Actions for CI/CD automation.

## Workflows

### 1. **CI** (`ci.yml`)
Runs on every push and pull request to `develop` and `master` branches.
- ✅ Builds the solution (.NET 9)
- ✅ Runs all unit tests
- ✅ Collects code coverage
- ✅ Uploads test results as artifacts

### 2. **CodeQL Security Scan** (`codeql.yml`)
Runs on push, pull request, and weekly schedule (Monday 03:00 UTC).
- 🔒 Analyzes C# code for security vulnerabilities
- 🔍 Detects code quality issues
- 📊 Uploads results to GitHub Security tab

### 3. **Publish to NuGet** (`nuget-publish.yml`)
Automatically publishes to NuGet.org when a new release is created.
- 📦 Extracts version from release tag (e.g., `v1.8.0`)
- 🔨 Builds and tests the library
- 📤 Publishes to NuGet.org
- 💾 Saves package as artifact

## Setup Instructions

### Required Secrets

To enable NuGet publishing, add the following secret to your repository:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Add:
   - **Name:** `NUGET_API_KEY`
   - **Value:** Your NuGet API key from https://www.nuget.org/account/apikeys

### Creating a Release

To publish a new version to NuGet:

1. Update version in `src/FileConversionLibrary/FileConversionLibrary.csproj`
2. Commit and push changes
3. Create a new release on GitHub:
   ```bash
   git tag v1.8.0
   git push origin v1.8.0
   ```
4. Go to GitHub → **Releases** → **Draft a new release**
5. Choose the tag `v1.8.0`
6. Add release notes
7. Click **Publish release**

The `nuget-publish.yml` workflow will automatically:
- Extract version from the tag
- Build and test the project
- Publish to NuGet.org

### Manual Publishing

You can also trigger publishing manually:

1. Go to **Actions** → **Publish to NuGet**
2. Click **Run workflow**
3. Enter version (e.g., `1.8.0`)
4. Click **Run workflow**

## Status Badges

Add these to your README.md:

```markdown
[![CI](https://github.com/TheMysteriousStranger90/FileConversionLibrary/actions/workflows/ci.yml/badge.svg)](https://github.com/TheMysteriousStranger90/FileConversionLibrary/actions/workflows/ci.yml)
[![CodeQL](https://github.com/TheMysteriousStranger90/FileConversionLibrary/actions/workflows/codeql.yml/badge.svg)](https://github.com/TheMysteriousStranger90/FileConversionLibrary/actions/workflows/codeql.yml)
[![NuGet](https://img.shields.io/nuget/v/FileConversionLibrary.svg)](https://www.nuget.org/packages/FileConversionLibrary/)
```

## Requirements

- **.NET 9.0** SDK
- **Ubuntu latest** runner
- **NuGet API Key** (for publishing)
