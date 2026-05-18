# Changelog

All notable changes to the WebSpark.Slurper project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.5.2] - 2026-05-18

### Added

- 10 new test files covering the previously untested factory-pattern API (113 new tests, 234 total):
  - `SlurperFactoryTests` — all four `Create*` methods, plugin registration, error paths
  - `Extractors/XmlExtractorTests` — Extract, ExtractFromFile, async variants, logger constructor
  - `Extractors/JsonExtractorTests` — Extract, file, async, options, error handling
  - `Extractors/CsvExtractorTests` — type conversion, quoting, dialects, parallel processing, streaming
  - `Extractors/HtmlExtractorTests` — XHTML parsing, file, async variants
  - `InputValidatorTests` — ValidateSourceContent, ValidateFilePath, ValidateUrl
  - `Serializers/ExpandoObjectExtensionsTests` — ToJson, ToJsonEnvelope
  - `ServiceCollectionExtensionsTests` — AddSlurper DI registration (both overloads)
  - `ExceptionTests` — all exception types and inheritance hierarchy
  - `YamlExtractorPluginTests` — CanHandle, Extract, multi-document YAML

### Changed

- Migrated test framework from xUnit / Xunit.SkippableFact to MSTest 4.2.3
- Upgraded MSTest packages: TestFramework, TestAdapter, Analyzers to 4.2.3
- Upgraded Microsoft.NET.Test.Sdk to 18.5.1
- Added coverlet.collector 6.0.4 for code coverage collection
- SlurperSpark branding applied to GitHub Pages (slurper.makeboldspark.com) and demo web app
- Added Make Bold Solutions / Make Bold Spark attribution to footer and README
- Updated HTTP User-Agent version string to 3.5.2 in JsonExtractor and HttpClientService

### Security

- Resolved all 24 open Dependabot npm security alerts (13 HIGH, 6 MEDIUM, 5 LOW)
- Upgraded `copy-webpack-plugin` 13 → 14.0.0 (fixes serialize-javascript RCE — GHSA-5c6j-r48x-rmvq)
- Upgraded `css-minimizer-webpack-plugin` 7 → 8.0.0 (fixes serialize-javascript DoS — GHSA-qj8w-gfj5-8c6v)
- Upgraded `@babel/plugin-transform-modules-systemjs` → 7.29.4 (fixes arbitrary code gen — GHSA-fv7c-fp4j-7gwp)
- Upgraded `fast-uri` → 3.1.2 (fixes path traversal and host confusion — GHSA-q3j6-qgpj-74h6, GHSA-v39h-62p7-jpjc)
- Upgraded `follow-redirects` → 1.16.0 (fixes auth header leak to cross-domain redirects)
- Upgraded `immutable` → 5.1.5 (fixes prototype pollution — GHSA-3q56-9cc2-46b4)
- Upgraded `minimatch` → 10.2.5 (fixes ReDoS — GHSA-c2c7-rcm5-vvqj)
- Upgraded `picomatch` → 4.0.4 (fixes method injection and ReDoS)
- Upgraded `qs` → 6.15.1 (fixes DoS via arrayLimit bypass)
- Upgraded `webpack` → 5.106.2 (fixes SSRF via buildHttp — GHSA-8fgc-7cc6-rx7x, GHSA-38r7-794h-5758)
- Fixed stale root-level `package.json` `copy-webpack-plugin ^13` reference
- Closed Dependabot PRs #16 and #17 as superseded by direct main-branch updates
- Both `package-lock.json` files now report 0 vulnerabilities (`npm audit`)

### Infrastructure

- Test coverage: line 32.4% → 76.1%, branch 31.3% → 68.2%, method 43.2% → 87.0%
- 234 tests passing (up from 96), 3 intentionally skipped, 0 failing

## [3.5.1] - 2026-01-12

### Security

- Fixed HIGH severity DoS vulnerability in `qs` dependency (CVE - arrayLimit bypass)
- Fixed HIGH severity command injection vulnerability in `glob` dependency
- Fixed HIGH severity ASN.1 Validator Desynchronization in `node-forge`
- Fixed HIGH severity ASN.1 Unbounded Recursion in `node-forge`
- Fixed MEDIUM severity ASN.1 OID Integer Truncation in `node-forge`

### Changed

- Updated npm package `qs` from 6.13.0 to 6.14.1
- Updated npm package `express` from 4.21.2 to 4.22.1
- Updated npm package `node-forge` from 1.3.1 to 1.3.2
- Updated npm package `glob` from 11.0.3 to 11.1.0
- Added npm overrides to force secure `qs` version across all transitive dependencies

### Infrastructure Updates

- All npm dependencies are now free of known security vulnerabilities
- npm audit reports 0 vulnerabilities

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

[3.5.2]: https://github.com/MarkHazleton/Slurper/compare/v3.5.1...v3.5.2
[3.5.1]: https://github.com/MarkHazleton/Slurper/compare/v3.5.0...v3.5.1
[3.5.0]: https://github.com/MarkHazleton/Slurper/compare/v3.3.0...v3.5.0
[3.3.0]: https://github.com/MarkHazleton/Slurper/releases/tag/v3.3.0
