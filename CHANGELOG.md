# Changelog

All notable changes to the WebSpark.Slurper project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.5.0] - 2025-11-16

### Added
- Full support for .NET 10.0
- Multi-targeting support for .NET 8.0, .NET 9.0, and .NET 10.0

### Changed
- Updated all Microsoft packages to version 10.0.0 for .NET 10 support
- Updated documentation to reflect multi-targeting capabilities

### Technical Details
- Target Frameworks: net8.0, net9.0, net10.0
- All Microsoft.Extensions.* packages updated to 10.0.0
- System.Text.Json updated to 10.0.0

## [3.3.0] - Previous Release

### Features
- XML data extraction and transformation
- JSON data extraction and transformation
- CSV data extraction and transformation
- HTML data extraction and transformation
- Unified API for all data formats
- Dynamic object support
- Async/await support for all extraction methods
- Dependency injection integration
- Comprehensive error handling
- Built-in logging support
- Plugin system for extensibility
- Performance optimization options (streaming, parallel processing, caching)
- Full nullable reference types support

### Infrastructure
- Multi-targeting support for .NET 8.0 and .NET 9.0
- GitHub Actions CI/CD pipeline
- Automated NuGet package publishing
- Symbol package (snupkg) generation
- SourceLink integration for debugging

[3.5.0]: https://github.com/MarkHazleton/Slurper/compare/v3.3.0...v3.5.0
[3.3.0]: https://github.com/MarkHazleton/Slurper/releases/tag/v3.3.0
