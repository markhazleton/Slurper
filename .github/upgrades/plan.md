kagekag# .NET 10.0 Upgrade Migration Plan
## WebSpark.Slurper Solution

**Generated:** 2025-01-28  
**Solution:** WebSpark.Slurper.sln  
**Source Branch:** main  
**Target Branch:** upgrade-to-NET10

---

## Executive Summary

### Scenario
Upgrade the WebSpark.Slurper solution from .NET 9.0 to .NET 10.0 (Preview), ensuring all projects target the latest framework version and all NuGet packages are updated to their .NET 10-compatible versions.

### Scope
- **Total Projects:** 3
  - 1 Multi-targeting Class Library (WebSpark.Slurper)
  - 1 ASP.NET Core Web Application (SlurperDemo.Web)
  - 1 Test Project (WebSpark.Slurper.Tests)
- **Current State:**
  - WebSpark.Slurper: net8.0;net9.0 (multi-targeting)
  - SlurperDemo.Web: net9.0
  - WebSpark.Slurper.Tests: net9.0
- **Total Lines of Code:** ~8,481 LOC

### Target State
After successful migration:
- WebSpark.Slurper → net8.0;net9.0;net10.0 (adds net10.0 to multi-targeting)
- SlurperDemo.Web → net10.0
- WebSpark.Slurper.Tests → net10.0
- All Microsoft.Extensions.* packages → 10.0.0
- System.Text.Json → 10.0.0

### Selected Strategy
**Big Bang Strategy** - All projects upgraded simultaneously in a single coordinated operation.

**Rationale:**
- Small solution (only 3 projects)
- Simple, clear dependency structure with no circular dependencies
- All projects currently on .NET 9.0 or multi-targeting modern .NET
- Homogeneous codebase with consistent patterns
- All packages have known .NET 10-compatible versions available
- No security vulnerabilities present
- Low complexity (no legacy .NET Framework projects)
- Good for rapid modernization to preview framework

### Complexity Assessment
**Overall Complexity: LOW**

**Justification:**
- ✅ Small project count (3 projects)
- ✅ Modern .NET projects only (no .NET Framework legacy)
- ✅ Clean dependency graph (library → tests + web app)
- ✅ Only 6 packages require updates (all Microsoft.Extensions.* family)
- ✅ No security vulnerabilities detected
- ✅ All packages already on version 9.0.9, straightforward upgrade to 10.0.0
- ✅ SDK-style projects only
- ✅ No breaking changes expected (incremental version bump)

### Critical Issues
**Good News:** ✅ No security vulnerabilities detected in current dependencies!

### Recommended Approach
**Big Bang Migration** - Update all projects and packages in a single atomic operation, followed by comprehensive testing. This approach is ideal given the small solution size and clean architecture.

---

## Migration Strategy

### 2.1 Approach Selection

**Chosen Strategy:** Big Bang Strategy

**Justification:**
- **Small Solution Size:** Only 3 projects makes coordinated simultaneous upgrade feasible
- **Modern Codebase:** All projects already on .NET 8.0+ or .NET 9.0
- **Clear Dependencies:** Simple linear dependency graph (foundation library → consuming projects)
- **Package Compatibility:** All 6 packages requiring updates are from the same Microsoft.Extensions family
- **Low Risk:** Preview framework upgrade with incremental version numbers
- **Fast Completion:** Single coordinated operation minimizes overall timeline
- **No Multi-Targeting Complexity:** Except for the library which already multi-targets and will simply add net10.0

### 2.2 Dependency-Based Ordering

**Migration Order Principle:** Bottom-up (dependencies first, dependents last)

**Dependency Analysis:**
```
WebSpark.Slurper (foundation, 0 dependencies)
    ↑
    ├── WebSpark.Slurper.Tests (depends on library)
    └── SlurperDemo.Web (depends on library)
```

**Big Bang Execution:** While projects have dependencies, the Big Bang strategy means all target framework updates and package updates happen simultaneously in a single atomic operation. The dependency order informs us of build order but doesn't dictate separate migration phases.

**Why Big Bang Works Here:**
- WebSpark.Slurper is multi-targeting, so adding net10.0 doesn't break existing net9.0 consumers
- Package updates are uniform across all projects
- Single restore and build cycle will handle all projects together

### 2.3 Parallel vs Sequential Execution

**Atomic Operation Approach:**
- All project file updates happen together
- All package reference updates happen together
- Single `dotnet restore` for entire solution
- Single `dotnet build` for entire solution
- Fix any compilation errors discovered during build

**Strategy Considerations:**
The Big Bang strategy eliminates the need for parallelization decisions—all changes are applied as a single coordinated batch, then the entire solution is built and tested together.

---

## Detailed Dependency Analysis

### 3.1 Dependency Graph Summary

**Visual Representation:**
```
Phase 1: Atomic Upgrade (All Projects Simultaneously)
├── WebSpark.Slurper.csproj (net8.0;net9.0;net10.0)
│   ├── 6 package updates (9.0.9 → 10.0.0)
│   └── Multi-targeting: append net10.0
├── SlurperDemo.Web.csproj (net10.0)
│   └── Framework update only
└── WebSpark.Slurper.Tests.csproj (net10.0)
    └── Framework update only
```

### 3.2 Project Groupings

**Phase 0: Preparation**
- Verify .NET 10.0 SDK is installed
- Ensure on upgrade-to-NET10 branch
- Pending changes already committed

**Phase 1: Atomic Upgrade**
All operations performed as single coordinated batch:
- Update all 3 project files to target net10.0
- Update all 6 NuGet packages in WebSpark.Slurper
- Restore dependencies
- Build solution
- Fix any compilation errors
- Verify 0 build errors

**Phase 2: Test Validation**
- Execute WebSpark.Slurper.Tests
- Verify all tests pass
- Validate SlurperDemo.Web application functionality

**Strategy-Specific Grouping Notes:**
Big Bang strategy groups all project updates and all package updates into a single atomic operation. There are no separate tiers or waves—everything upgrades simultaneously to minimize intermediate states and reduce overall migration time.

---

## Project-by-Project Migration Plans

### Project: WebSpark.Slurper

**Current State**
- **Target Framework:** net8.0;net9.0
- **Project Type:** ClassLibrary (multi-targeting)
- **Dependencies:** 0 project dependencies
- **Dependants:** 2 projects (SlurperDemo.Web, WebSpark.Slurper.Tests)
- **Package Count:** 8 total packages
- **Packages Requiring Updates:** 6
- **LOC:** 3,425
- **Files:** 24
- **Complexity:** Low

**Target State**
- **Target Framework:** net8.0;net9.0;net10.0 (append net10.0 to existing multi-targeting)
- **Updated Packages:** 6 packages

**Migration Steps**

1. **Prerequisites**
   - .NET 10.0 SDK installed and verified
   - On upgrade-to-NET10 branch
   - This is the foundation library with no dependencies—first to be updated in the atomic operation

2. **Framework Update**
   - **File:** WebSpark.Slurper\WebSpark.Slurper.csproj
   - **Change:** Update `<TargetFrameworks>` element from `net8.0;net9.0` to `net8.0;net9.0;net10.0`
   - **Rationale:** Maintains backward compatibility while adding .NET 10 support

3. **Package Updates**
   All Microsoft.Extensions.* packages upgrade from 9.0.9 to 10.0.0:

   | Package | Current Version | Target Version | Reason |
   |---------|----------------|----------------|---------|
   | Microsoft.Extensions.Configuration.UserSecrets | 9.0.9 | 10.0.0 | .NET 10 compatibility |
   | Microsoft.Extensions.DependencyInjection | 9.0.9 | 10.0.0 | .NET 10 compatibility |
   | Microsoft.Extensions.Http | 9.0.9 | 10.0.0 | .NET 10 compatibility |
   | Microsoft.Extensions.Logging | 9.0.9 | 10.0.0 | .NET 10 compatibility |
   | Microsoft.Extensions.Logging.Abstractions | 9.0.9 | 10.0.0 | .NET 10 compatibility |
   | System.Text.Json | 9.0.9 | 10.0.0 | .NET 10 compatibility |

   **Unchanged Packages:**
   - Microsoft.CSharp 4.7.0 (compatible)
   - Microsoft.SourceLink.GitHub 8.0.0 (compatible)

4. **Expected Breaking Changes**
   - **Low Risk:** Microsoft.Extensions.* packages typically maintain API compatibility across major versions
   - **System.Text.Json:** May have new features but existing API should remain stable
   - **Multi-targeting:** net10.0 builds independently; net8.0 and net9.0 targets unaffected
   - **Note:** Preview frameworks may have behavior changes; monitor compiler warnings

5. **Code Modifications**
   - **Expected:** No code changes required
   - **Monitor:** Compiler warnings about deprecated APIs
   - **Review Areas:**
     - Dependency injection registration patterns
     - HTTP client factory usage
     - JSON serialization/deserialization code
     - Logging implementations

6. **Testing Strategy**
   - **Unit Tests:** WebSpark.Slurper.Tests project will validate core functionality
   - **Integration Tests:** SlurperDemo.Web will validate real-world usage
   - **Multi-targeting Validation:** Ensure all three target frameworks (net8.0, net9.0, net10.0) build successfully
   - **Key Scenarios:**
     - XML slurping functionality
     - JSON slurping functionality
     - CSV slurping functionality
     - HTML slurping functionality
     - Dependency injection integration
     - HTTP client functionality

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly for all target frameworks
   - [ ] Project builds without errors for net8.0
   - [ ] Project builds without errors for net9.0
   - [ ] Project builds without errors for net10.0
   - [ ] No new compiler warnings introduced
   - [ ] All unit tests pass (verified via Tests project)
   - [ ] No security warnings

---

### Project: SlurperDemo.Web

**Current State**
- **Target Framework:** net9.0
- **Project Type:** AspNetCore (Web Application)
- **Dependencies:** 1 (WebSpark.Slurper)
- **Dependants:** 0
- **Package Count:** 0 explicit packages (uses framework references)
- **LOC:** 3,036
- **Files:** 36
- **Complexity:** Low

**Target State**
- **Target Framework:** net10.0
- **Updated Packages:** None (relies on framework and WebSpark.Slurper)

**Migration Steps**

1. **Prerequisites**
   - WebSpark.Slurper already updated in atomic operation
   - .NET 10.0 SDK available
   - Part of the Big Bang atomic operation

2. **Framework Update**
   - **File:** SlurperDemo.Web\SlurperDemo.Web.csproj
   - **Change:** Update `<TargetFramework>` element from `net9.0` to `net10.0`
   - **Impact:** Web application will use .NET 10 runtime and ASP.NET Core 10.0

3. **Package Updates**
   - **None Required:** Project has no explicit PackageReference elements
   - **Framework References:** ASP.NET Core framework reference will automatically use .NET 10 versions

4. **Expected Breaking Changes**
   - **ASP.NET Core 10.0 Changes:** Monitor for:
     - Minimal API changes (if used)
     - Middleware pipeline changes
     - Razor Pages rendering changes
     - Tag helper behavior changes
   - **Preview Framework:** Potential behavior differences in routing, model binding, or view rendering
   - **Configuration:** No expected changes to appsettings.json or startup configuration

5. **Code Modifications**
   - **Expected:** Minimal to none
   - **Review Areas:**
     - Controllers (HomeController, JSONController, SuperheroController)
     - Razor views (Index.cshtml, JSON views, Superhero views)
     - Program.cs / Startup.cs configuration
     - Middleware pipeline
     - Dependency injection registrations
     - Static file serving
     - Routing configuration

6. **Testing Strategy**
   - **Manual Testing Required:**
     - [ ] Application starts successfully
     - [ ] Home page loads (Slurper Detective Agency)
     - [ ] XML Demo case works (Case #001)
     - [ ] JSON Demo case works (Case #002)
     - [ ] CSV Demo case works (Case #003)
     - [ ] HTML Demo case works (Case #004)
     - [ ] Legacy Demo case works (Case #005)
     - [ ] Superhero Database feature works
     - [ ] All navigation links functional
     - [ ] Static assets load (CSS, JavaScript, images)
   - **Integration Tests:** Verify WebSpark.Slurper integration
   - **Performance:** Monitor startup time and response times

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Application starts without errors
   - [ ] All web pages render correctly
   - [ ] All Slurper demos functional
   - [ ] No console errors in browser
   - [ ] No security warnings

---

### Project: WebSpark.Slurper.Tests

**Current State**
- **Target Framework:** net9.0
- **Project Type:** DotNetCoreApp (Test Project)
- **Dependencies:** 1 (WebSpark.Slurper)
- **Dependants:** 0
- **Package Count:** 4 test packages
- **LOC:** 2,020
- **Files:** 7
- **Complexity:** Low

**Target State**
- **Target Framework:** net10.0
- **Updated Packages:** None (all test packages already compatible)

**Migration Steps**

1. **Prerequisites**
   - WebSpark.Slurper already updated in atomic operation
   - Part of the Big Bang atomic operation
   - Test projects typically migrate after code projects, but in Big Bang all update together

2. **Framework Update**
   - **File:** WebSpark.Slurper.Tests\WebSpark.Slurper.Tests.csproj
   - **Change:** Update `<TargetFramework>` element from `net9.0` to `net10.0`

3. **Package Updates**
   - **No Updates Required:** All test packages already compatible with .NET 10

   | Package | Current Version | Target Version | Status |
   |---------|----------------|----------------|---------|
   | Microsoft.NET.Test.Sdk | 18.0.0 | 18.0.0 | ✅ Compatible |
   | xunit | 2.9.3 | 2.9.3 | ✅ Compatible |
   | xunit.runner.visualstudio | 3.1.5 | 3.1.5 | ✅ Compatible |
   | Xunit.SkippableFact | 1.5.23 | 1.5.23 | ✅ Compatible |

4. **Expected Breaking Changes**
   - **None Expected:** xUnit test framework is stable across .NET versions
   - **Test Execution:** Should work identically on .NET 10 as on .NET 9
   - **Monitor:** Test discovery and execution timing

5. **Code Modifications**
   - **Expected:** None
   - **Review Areas:**
     - Test fixtures and setup
     - Mock objects and test data
     - Assertion patterns
     - Async test patterns

6. **Testing Strategy**
   - **Critical:** All existing tests must pass on .NET 10
   - **Test Categories to Validate:**
     - XML slurping tests
     - JSON slurping tests
     - CSV slurping tests
     - HTML slurping tests
     - Error handling tests
     - Edge case tests
   - **Success Criteria:** 100% test pass rate

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Test discovery works correctly
   - [ ] All tests execute successfully
   - [ ] All tests pass (0 failures)
   - [ ] Test execution time acceptable
   - [ ] No skipped tests (unless intentionally skipped)

---

## Package Update Reference

### Common Package Updates (affecting WebSpark.Slurper only)

All packages are in the WebSpark.Slurper project. The Microsoft.Extensions.* family updates uniformly:

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| Microsoft.Extensions.Configuration.UserSecrets | 9.0.9 | 10.0.0 | 1 (WebSpark.Slurper) | .NET 10 compatibility |
| Microsoft.Extensions.DependencyInjection | 9.0.9 | 10.0.0 | 1 (WebSpark.Slurper) | .NET 10 compatibility |
| Microsoft.Extensions.Http | 9.0.9 | 10.0.0 | 1 (WebSpark.Slurper) | .NET 10 compatibility |
| Microsoft.Extensions.Logging | 9.0.9 | 10.0.0 | 1 (WebSpark.Slurper) | .NET 10 compatibility |
| Microsoft.Extensions.Logging.Abstractions | 9.0.9 | 10.0.0 | 1 (WebSpark.Slurper) | .NET 10 compatibility |
| System.Text.Json | 9.0.9 | 10.0.0 | 1 (WebSpark.Slurper) | .NET 10 compatibility |

### Compatible Packages (No Updates Required)

| Package | Version | Projects | Status |
|---------|---------|----------|--------|
| Microsoft.CSharp | 4.7.0 | WebSpark.Slurper | ✅ Compatible with .NET 10 |
| Microsoft.SourceLink.GitHub | 8.0.0 | WebSpark.Slurper | ✅ Compatible with .NET 10 |
| Microsoft.NET.Test.Sdk | 18.0.0 | WebSpark.Slurper.Tests | ✅ Compatible with .NET 10 |
| xunit | 2.9.3 | WebSpark.Slurper.Tests | ✅ Compatible with .NET 10 |
| xunit.runner.visualstudio | 3.1.5 | WebSpark.Slurper.Tests | ✅ Compatible with .NET 10 |
| Xunit.SkippableFact | 1.5.23 | WebSpark.Slurper.Tests | ✅ Compatible with .NET 10 |

---

## Breaking Changes Catalog

### .NET 10.0 Framework Breaking Changes

**Status:** .NET 10 is in preview. Breaking changes are minimal for incremental versions (9 → 10).

**Areas to Monitor:**
1. **BCL (Base Class Library):**
   - Minor API improvements and deprecations
   - New optional parameters on existing methods
   - Performance optimizations may change behavior edge cases

2. **ASP.NET Core:**
   - Minimal breaking changes expected for preview
   - Middleware pipeline refinements
   - Razor Pages and MVC improvements
   - Potential changes to default configurations

3. **Language Features (C# 13):**
   - New language features are additive
   - Existing code remains compatible

### Package-Specific Breaking Changes

#### Microsoft.Extensions.* Packages (9.0.9 → 10.0.0)

**Microsoft.Extensions.DependencyInjection:**
- **Expected Impact:** None
- **Potential Changes:** New overloads or service registration patterns
- **Mitigation:** Existing registration code should work unchanged

**Microsoft.Extensions.Http:**
- **Expected Impact:** None
- **Potential Changes:** HttpClient factory improvements
- **Mitigation:** Review HTTP client configurations if custom policies applied

**Microsoft.Extensions.Logging:**
- **Expected Impact:** None
- **Potential Changes:** New log levels or formatting options
- **Mitigation:** Existing logging statements unchanged

**Microsoft.Extensions.Logging.Abstractions:**
- **Expected Impact:** None
- **Potential Changes:** Interface additions (backward compatible)
- **Mitigation:** Only affects custom logger implementations

**Microsoft.Extensions.Configuration.UserSecrets:**
- **Expected Impact:** None
- **Potential Changes:** Secret management improvements
- **Mitigation:** Existing secret loading code unchanged

#### System.Text.Json (9.0.9 → 10.0.0)

**Expected Impact:** Low
- **Serialization:** Existing JSON serialization code should work unchanged
- **Deserialization:** Existing JSON deserialization code should work unchanged
- **New Features:** May include new attributes or options (opt-in)
- **Performance:** Possible performance improvements
- **Mitigation:** Test JSON serialization/deserialization thoroughly in Slurper scenarios

### Compilation Errors to Watch For

Based on the Big Bang strategy, after updating all project files and packages, expect:

1. **Namespace Changes:** (Unlikely but possible)
   - Compiler error: "The type or namespace name 'X' could not be found"
   - Resolution: Update using statements or fully qualified names

2. **API Signature Changes:** (Rare for Microsoft.Extensions.*)
   - Compiler error: "No overload for method 'X' takes 'N' arguments"
   - Resolution: Review method signature and adjust parameters

3. **Obsolete API Warnings:** (More common)
   - Compiler warning: "'X' is obsolete: 'Use Y instead'"
   - Resolution: Update to recommended replacement API

4. **Multi-targeting Issues:** (WebSpark.Slurper only)
   - Build errors specific to net10.0 target
   - Resolution: Use conditional compilation if needed (`#if NET10_0`)

---

## Implementation Timeline

### Phase 0: Preparation

**Operations:**
- [x] Verify .NET 10.0 SDK installed
- [x] Committed pending changes to main branch
- [x] Created and switched to upgrade-to-NET10 branch
- [x] Analyzed solution and generated assessment

**Deliverables:**
- ✅ Environment ready for .NET 10 upgrade
- ✅ Clean working directory on upgrade branch

---

### Phase 1: Atomic Upgrade

**Operations** (performed as single coordinated batch):

All project file updates and package updates happen together in one atomic operation:

1. **Update all project target frameworks:**
   - WebSpark.Slurper.csproj: `net8.0;net9.0` → `net8.0;net9.0;net10.0`
   - SlurperDemo.Web.csproj: `net9.0` → `net10.0`
   - WebSpark.Slurper.Tests.csproj: `net9.0` → `net10.0`

2. **Update all package references in WebSpark.Slurper.csproj:**
   - Microsoft.Extensions.Configuration.UserSecrets: `9.0.9` → `10.0.0`
   - Microsoft.Extensions.DependencyInjection: `9.0.9` → `10.0.0`
   - Microsoft.Extensions.Http: `9.0.9` → `10.0.0`
   - Microsoft.Extensions.Logging: `9.0.9` → `10.0.0`
   - Microsoft.Extensions.Logging.Abstractions: `9.0.9` → `10.0.0`
   - System.Text.Json: `9.0.9` → `10.0.0`

3. **Restore dependencies:**
   ```bash
   dotnet restore WebSpark.Slurper.sln
   ```

4. **Build solution and fix all compilation errors:**
   ```bash
   dotnet build WebSpark.Slurper.sln --configuration Release
   ```
   - Review any compiler errors
   - Review any compiler warnings
   - Fix breaking changes based on Breaking Changes Catalog (see §Breaking Changes Catalog)
   - Apply fixes for namespace changes, API signature changes, or obsolete APIs
   - Rebuild until 0 errors

5. **Verify solution builds with 0 errors**

**Deliverables:**
- ✅ All 3 projects targeting .NET 10
- ✅ All 6 packages updated to version 10.0.0
- ✅ Solution builds successfully with 0 errors
- ✅ No new compiler warnings introduced

**Expected Duration:** 30-60 minutes
- File updates: 10 minutes
- Restore and build: 5 minutes
- Fix any errors: 15-40 minutes (depending on issues found)

---

### Phase 2: Test Validation

**Operations:**

1. **Execute unit tests:**
   ```bash
   dotnet test WebSpark.Slurper.Tests\WebSpark.Slurper.Tests.csproj
   ```
   - Verify all tests discovered
   - Verify all tests pass
   - Address any test failures related to framework or package changes

2. **Manual application testing (SlurperDemo.Web):**
   ```bash
   dotnet run --project SlurperDemo.Web\SlurperDemo.Web.csproj
   ```
   - Start application and verify successful startup
   - Test all detective case files:
     - Case #001: XML Archives demo
     - Case #002: JSON Enigma demo
     - Case #003: Spreadsheet Caper (CSV) demo
     - Case #004: Web Page Mystery (HTML) demo
     - Case #005: Legacy Files demo
   - Test Superhero Database special mission
   - Verify all UI elements render correctly
   - Verify all data slurping functionality works

3. **Address any test failures:**
   - Investigate root cause (framework change vs. package change)
   - Apply fixes based on Breaking Changes Catalog
   - Re-run tests to verify fixes

**Deliverables:**
- ✅ All unit tests pass (100% pass rate)
- ✅ SlurperDemo.Web application runs without errors
- ✅ All Slurper demos functional
- ✅ No runtime exceptions or unexpected behavior

**Expected Duration:** 30-45 minutes
- Automated tests: 5 minutes
- Manual testing: 20-30 minutes
- Fix any issues: 5-10 minutes (if needed)

---

### Phase 3: Finalization

**Operations:**

1. **Final verification:**
   - Full solution build (Release configuration)
   - Full test suite execution
   - Code review of changes
   - Documentation updates (README, if applicable)

2. **Commit changes:**
   ```bash
   git add -A
   git commit -m "Upgrade to .NET 10.0: Updated all projects and packages"
   ```

3. **Push to remote:**
   ```bash
   git push origin upgrade-to-NET10
   ```

4. **Create Pull Request:**
   - Title: "Upgrade to .NET 10.0"
   - Description: Include summary of changes from this plan
   - Reviewers: Assign appropriate reviewers

**Deliverables:**
- ✅ All changes committed to upgrade-to-NET10 branch
- ✅ Pull request created for review
- ✅ Documentation updated

**Expected Duration:** 15-30 minutes

---

## Total Estimated Timeline

**Total Duration:** 1.5 - 2.5 hours

- Phase 0 (Preparation): ✅ Complete
- Phase 1 (Atomic Upgrade): 30-60 minutes
- Phase 2 (Test Validation): 30-45 minutes
- Phase 3 (Finalization): 15-30 minutes

**Note:** Times are estimates. Preview framework upgrades may reveal unexpected issues requiring additional time for resolution.

---

## Risk Management

### 5.1 High-Risk Changes

Based on Big Bang strategy, the entire atomic upgrade is the highest-risk point.

| Risk Area | Risk Level | Mitigation Strategy |
|-----------|-----------|---------------------|
| Preview Framework Instability | Medium | Thorough testing; maintain main branch as rollback; monitor .NET 10 release notes |
| Multi-targeting Build Failures | Low | WebSpark.Slurper builds for net8.0/net9.0/net10.0 independently; existing targets unaffected |
| Package API Breaking Changes | Low | Microsoft.Extensions.* packages maintain compatibility; review breaking changes catalog |
| ASP.NET Core Behavior Changes | Medium | Manual testing of all web app scenarios; preview may have rendering or middleware changes |
| Test Failures | Low | Tests already on .NET 9; minimal changes expected for .NET 10 |
| Runtime Exceptions | Medium | Comprehensive manual testing phase; preview runtime may have edge case bugs |

### 5.2 Risk Mitigation

**Before Starting:**
- ✅ Full solution builds on .NET 9 (verified)
- ✅ All tests pass on .NET 9 (assume verified)
- ✅ Clean Git state with upgrade-to-NET10 branch

**During Upgrade:**
- Single atomic operation reduces partial-state risk
- Immediate build after updates reveals compilation issues quickly
- Breaking changes catalog guides error resolution

**After Upgrade:**
- Comprehensive test suite execution
- Manual testing of all application features
- Monitor for runtime exceptions or unexpected behavior

### 5.3 Rollback Plan

**If Critical Issues Arise:**

1. **Immediate Rollback (before commit):**
   ```bash
   git checkout .
   git clean -fd
   ```
   This discards all uncommitted changes and returns to clean state.

2. **Post-Commit Rollback:**
   ```bash
   git reset --hard HEAD~1
   ```
   This removes the upgrade commit (if not yet pushed).

3. **Post-Push Rollback:**
   ```bash
   git revert <commit-sha>
   git push origin upgrade-to-NET10
   ```
   This creates a revert commit.

4. **Branch Abandonment (worst case):**
   - Switch back to main branch
   - Delete upgrade-to-NET10 branch
   - Start fresh with new analysis

**Rollback Criteria:**
- Unresolvable compilation errors after 1 hour
- Critical test failures that can't be fixed quickly
- Runtime exceptions in core functionality
- Performance degradation > 50%
- Blocking issues with preview framework

### 5.4 Contingency Plans

**Alternative Approach 1: Partial Rollback**
- If .NET 10 has critical issues, revert to .NET 9 but keep package updates at 10.0.0 (if compatible)
- May require downgrading packages back to 9.0.9 if interdependent

**Alternative Approach 2: Wait for .NET 10 RC**
- If preview issues are blockers, abort upgrade
- Wait for Release Candidate (RC) or RTM builds
- Retry upgrade with more stable framework

**Alternative Approach 3: Staged Testing**
- If issues found in Phase 2, pause web app testing
- Focus on getting library and tests working first
- Defer SlurperDemo.Web testing until library stable

---

## Testing and Validation Strategy

### 6.1 Multi-Level Testing

#### Per-Project Validation

**After Atomic Upgrade (Phase 1):**
- All 3 projects build without errors
- All 3 projects build without warnings (or only expected warnings)
- No package restore errors
- No dependency conflicts

**Focus:** Compilation and dependency resolution

#### Phase Testing (Phase 2)

**WebSpark.Slurper Library:**
- Builds for all three target frameworks (net8.0, net9.0, net10.0)
- No breaking changes in public API
- All functionality accessible to dependent projects

**WebSpark.Slurper.Tests:**
- All tests discovered correctly
- All tests execute without crashing
- All tests pass (100% pass rate)
- Test execution time comparable to .NET 9

**SlurperDemo.Web:**
- Application starts without errors
- All pages load correctly
- All Slurper demos functional
- No JavaScript console errors
- Responsive UI behavior unchanged

**Focus:** Functional correctness and integration

#### Full Solution Testing (Phase 2)

**End-to-End Scenarios:**

1. **XML Slurping (Case #001):**
   - Load XML data files
   - Parse XML using Slurper
   - Display results in web UI
   - Verify data accuracy

2. **JSON Slurping (Case #002):**
   - Load JSON data files
   - Parse JSON using Slurper
   - Before & After analysis display
   - Verify nested object handling

3. **CSV Slurping (Case #003):**
   - Load CSV data files
   - Parse tabular data
   - Display in table format
   - Verify column mappings

4. **HTML Slurping (Case #004):**
   - Load HTML documents
   - Extract data from markup
   - Display extracted content
   - Verify selector logic

5. **Legacy Slurping (Case #005):**
   - Test XmlSlurper static methods
   - Test JsonSlurper static methods
   - Verify backward compatibility

6. **Superhero Database:**
   - Load superhero data (JSON, XML, CSV)
   - Display hero cards
   - Interactive features work
   - Data binding correct

**Focus:** Real-world usage and user-facing functionality

### 6.2 Testing Checklist

#### Build Validation
- [ ] `dotnet restore` succeeds for entire solution
- [ ] `dotnet build` succeeds for WebSpark.Slurper (all target frameworks)
- [ ] `dotnet build` succeeds for SlurperDemo.Web
- [ ] `dotnet build` succeeds for WebSpark.Slurper.Tests
- [ ] No compilation errors
- [ ] No new compiler warnings (or all warnings justified)

#### Unit Test Validation
- [ ] `dotnet test` discovers all tests in WebSpark.Slurper.Tests
- [ ] All XML slurping tests pass
- [ ] All JSON slurping tests pass
- [ ] All CSV slurping tests pass
- [ ] All HTML slurping tests pass
- [ ] All error handling tests pass
- [ ] All edge case tests pass
- [ ] Test execution time < 2x previous time
- [ ] 0 test failures
- [ ] 0 test crashes

#### Web Application Validation
- [ ] `dotnet run` starts SlurperDemo.Web without errors
- [ ] Home page (Detective Agency) loads and displays correctly
- [ ] Navigation menu functional
- [ ] Case #001 (XML Demo) works end-to-end
- [ ] Case #002 (JSON Demo) works end-to-end
- [ ] Case #003 (CSV Demo) works end-to-end
- [ ] Case #004 (HTML Demo) works end-to-end
- [ ] Case #005 (Legacy Demo) works end-to-end
- [ ] Superhero Database feature works end-to-end
- [ ] All static assets load (CSS, JS, images)
- [ ] No browser console errors
- [ ] Responsive design works (desktop, mobile)
- [ ] All buttons and links functional

#### Performance Validation
- [ ] Application startup time acceptable (< 10 seconds)
- [ ] Page load times comparable to .NET 9
- [ ] Data parsing performance not degraded
- [ ] Memory usage comparable to .NET 9
- [ ] No memory leaks detected during testing

#### Security Validation
- [ ] No new security warnings from NuGet packages
- [ ] No vulnerable dependencies introduced
- [ ] HTTPS configuration unchanged
- [ ] Authentication/authorization (if any) works

---

## Source Control Strategy

### Branching Strategy

**Main Upgrade Branch:** `upgrade-to-NET10`
- Created from: `main`
- Purpose: Contains all .NET 10 upgrade changes
- Integration: Will be merged back to `main` via Pull Request

**No Feature Branches:**
- Big Bang strategy uses single upgrade branch
- All changes committed directly to `upgrade-to-NET10`

### Commit Strategy

**Big Bang Approach: Single Comprehensive Commit (Preferred)**

Since this is a Big Bang migration with a small solution, prefer a single atomic commit containing all changes:

**Commit Message Template:**
```
Upgrade to .NET 10.0

- Updated WebSpark.Slurper to multi-target net8.0;net9.0;net10.0
- Updated SlurperDemo.Web to target net10.0
- Updated WebSpark.Slurper.Tests to target net10.0
- Upgraded 6 Microsoft.Extensions.* packages from 9.0.9 to 10.0.0
- Upgraded System.Text.Json from 9.0.9 to 10.0.0
- All tests passing
- All demos functional

Projects updated:
- WebSpark.Slurper (added net10.0 target)
- SlurperDemo.Web (net9.0 → net10.0)
- WebSpark.Slurper.Tests (net9.0 → net10.0)

Packages updated:
- Microsoft.Extensions.Configuration.UserSecrets 10.0.0
- Microsoft.Extensions.DependencyInjection 10.0.0
- Microsoft.Extensions.Http 10.0.0
- Microsoft.Extensions.Logging 10.0.0
- Microsoft.Extensions.Logging.Abstractions 10.0.0
- System.Text.Json 10.0.0
```

**When to Commit:**
- After Phase 1 (Atomic Upgrade) completes successfully
- After Phase 2 (Test Validation) passes all checks
- Before Phase 3 (Finalization)

**Alternative: Multiple Commits (If Issues Arise)**

If troubleshooting requires incremental commits:

1. **Initial Update Commit:**
   ```
   Upgrade project files and packages to .NET 10.0
   
   - Updated all project target frameworks
   - Updated all package references
   - Solution builds successfully
   ```

2. **Fix Compilation Errors Commit:**
   ```
   Fix compilation errors from .NET 10 upgrade
   
   - Fixed namespace changes
   - Updated API signatures
   - Resolved obsolete API warnings
   ```

3. **Fix Test Failures Commit:**
   ```
   Fix test failures after .NET 10 upgrade
   
   - Updated test expectations
   - Fixed test data issues
   - All tests now passing
   ```

### Review and Merge Process

**Pull Request Requirements:**

1. **PR Title:** "Upgrade to .NET 10.0"

2. **PR Description:**
   ```markdown
   ## Summary
   Upgrades entire WebSpark.Slurper solution to .NET 10.0 (Preview).
   
   ## Changes
   - All projects now target .NET 10
   - All Microsoft.Extensions.* packages updated to 10.0.0
   - System.Text.Json updated to 10.0.0
   
   ## Testing
   - ✅ All unit tests pass (WebSpark.Slurper.Tests)
   - ✅ All web demos functional (SlurperDemo.Web)
   - ✅ No breaking changes in public API
   - ✅ Performance comparable to .NET 9
   
   ## Migration Strategy
   Big Bang approach - all projects upgraded simultaneously.
   
   ## Risks
   - .NET 10 is preview; monitor for stability issues
   - Thorough testing completed
   
   ## Rollback Plan
   Revert this PR if critical issues discovered.
   ```

3. **Review Checklist:**
   - [ ] Code review completed
   - [ ] All builds succeed (verified in PR checks)
   - [ ] All tests pass (verified in PR checks)
   - [ ] Manual testing completed and documented
   - [ ] No merge conflicts with main
   - [ ] Documentation updated (if applicable)

4. **Merge Criteria:**
   - At least 1 approval from code owner
   - All CI/CD checks passing
   - No unresolved comments
   - Manual testing sign-off

5. **Merge Method:**
   - **Squash and Merge (Recommended):** Consolidates all commits into single commit on main
   - **Merge Commit (Alternative):** Preserves full commit history if multiple commits needed

6. **Post-Merge Actions:**
   - Delete upgrade-to-NET10 branch (optional, keep for reference)
   - Tag release: `git tag v1.0.0-net10.0` (optional)
   - Update documentation with .NET 10 requirements
   - Announce upgrade to team/users

---

## Success Criteria

### 9.1 Technical Success Criteria

- [x] .NET 10.0 SDK verified installed
- [ ] WebSpark.Slurper targets net8.0;net9.0;net10.0
- [ ] SlurperDemo.Web targets net10.0
- [ ] WebSpark.Slurper.Tests targets net10.0
- [ ] All 6 packages updated to version 10.0.0:
  - [ ] Microsoft.Extensions.Configuration.UserSecrets 10.0.0
  - [ ] Microsoft.Extensions.DependencyInjection 10.0.0
  - [ ] Microsoft.Extensions.Http 10.0.0
  - [ ] Microsoft.Extensions.Logging 10.0.0
  - [ ] Microsoft.Extensions.Logging.Abstractions 10.0.0
  - [ ] System.Text.Json 10.0.0
- [ ] Zero security vulnerabilities in dependencies (already true, maintained)
- [ ] All 3 projects build without errors
- [ ] All 3 projects build without warnings (or only expected .NET 10 preview warnings)
- [ ] All automated tests pass (100% pass rate in WebSpark.Slurper.Tests)
- [ ] Performance within acceptable thresholds (startup time, page load, data parsing)

### 9.2 Quality Criteria

- [ ] Code quality maintained (no degradation)
- [ ] Test coverage maintained (no tests removed or disabled)
- [ ] Documentation updated:
  - [ ] README.md reflects .NET 10 requirement (if applicable)
  - [ ] Package documentation updated (if applicable)
- [ ] No known regressions in functionality
- [ ] Public API unchanged (WebSpark.Slurper library)
- [ ] All Slurper demos functional:
  - [ ] XML slurping works
  - [ ] JSON slurping works
  - [ ] CSV slurping works
  - [ ] HTML slurping works
  - [ ] Legacy static methods work

### 9.3 Process Criteria

- [x] Big Bang Strategy principles followed throughout migration
- [x] Source control strategy followed:
  - [x] Created upgrade-to-NET10 branch
  - [x] Committed pre-upgrade state
  - [ ] Atomic commit with all changes (or justified multiple commits)
  - [ ] Descriptive commit message(s)
  - [ ] Pull request created with full description
- [ ] Testing strategy executed:
  - [ ] Build validation completed
  - [ ] Unit test validation completed
  - [ ] Web application validation completed
  - [ ] Performance validation completed
- [ ] Risk management followed:
  - [ ] Rollback plan available
  - [ ] Contingency plans documented
  - [ ] Issues tracked and resolved
- [ ] Code review completed (post-PR)
- [ ] Merge to main successful

### 9.4 Strategy-Specific Success Criteria

**Big Bang Strategy Validation:**
- [ ] All projects upgraded simultaneously (no intermediate states)
- [ ] Single atomic operation completed (project updates + package updates)
- [ ] No partial upgrades or multi-stage migrations performed
- [ ] Fast completion timeline (< 3 hours total)
- [ ] Minimal overhead from coordination (single branch, single commit preferred)

---

## Appendices

### A. File Change Summary

**Files to be Modified:**

1. **WebSpark.Slurper\WebSpark.Slurper.csproj**
   - `<TargetFrameworks>`: `net8.0;net9.0` → `net8.0;net9.0;net10.0`
   - 6 `<PackageReference>` elements: version `9.0.9` → `10.0.0`

2. **SlurperDemo.Web\SlurperDemo.Web.csproj**
   - `<TargetFramework>`: `net9.0` → `net10.0`

3. **WebSpark.Slurper.Tests\WebSpark.Slurper.Tests.csproj**
   - `<TargetFramework>`: `net9.0` → `net10.0`

**Total Files Modified:** 3 project files

**No Code Files Modified:** (unless breaking changes discovered during Phase 1)

### B. Command Reference

**Restore Dependencies:**
```bash
dotnet restore C:\GitHub\MarkHazleton\Slurper\WebSpark.Slurper.sln
```

**Build Solution:**
```bash
dotnet build C:\GitHub\MarkHazleton\Slurper\WebSpark.Slurper.sln --configuration Release
```

**Build Specific Project:**
```bash
dotnet build C:\GitHub\MarkHazleton\Slurper\WebSpark.Slurper\WebSpark.Slurper.csproj --configuration Release
```

**Run Tests:**
```bash
dotnet test C:\GitHub\MarkHazleton\Slurper\WebSpark.Slurper.Tests\WebSpark.Slurper.Tests.csproj
```

**Run Web Application:**
```bash
dotnet run --project C:\GitHub\MarkHazleton\Slurper\SlurperDemo.Web\SlurperDemo.Web.csproj
```

**Check Installed SDKs:**
```bash
dotnet --list-sdks
```

**Verify .NET 10 SDK:**
```bash
dotnet --version
```

### C. Useful Resources

**Official Documentation:**
- [.NET 10 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/10.0) (Preview)
- [ASP.NET Core 10.0 Breaking Changes](https://docs.microsoft.com/en-us/aspnet/core/migration/90-to-100)
- [.NET 10 Breaking Changes](https://docs.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [System.Text.Json Migration Guide](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to)

**Package Documentation:**
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/)
- [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http/)
- [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging/)
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json/)

**Community Resources:**
- [.NET Blog - .NET 10 Announcements](https://devblogs.microsoft.com/dotnet/)
- [GitHub Issues - dotnet/runtime](https://github.com/dotnet/runtime/issues)
- [GitHub Issues - dotnet/aspnetcore](https://github.com/dotnet/aspnetcore/issues)

---

## Notes and Assumptions

1. **Preview Framework:** .NET 10 is in preview as of this plan's creation. Expect potential instability, bugs, or behavior changes. Monitor official release notes.

2. **SDK Installation:** Plan assumes .NET 10 SDK is already installed. If not, download from https://dotnet.microsoft.com/download/dotnet/10.0

3. **Multi-targeting Strategy:** WebSpark.Slurper maintains backward compatibility by multi-targeting. This allows consuming projects to choose their target framework.

4. **Package Compatibility:** All test packages (xUnit, Microsoft.NET.Test.Sdk) are already .NET 10 compatible based on assessment. No updates required.

5. **No Security Vulnerabilities:** Assessment confirmed no security issues in current dependencies. Maintain this by keeping packages up-to-date.

6. **Big Bang Rationale:** Small solution size (3 projects, <10k LOC total) makes Big Bang approach low-risk and efficient.

7. **Testing Emphasis:** Manual testing of SlurperDemo.Web is critical since it's a user-facing application. All demo cases must be validated.

8. **Commit Strategy Flexibility:** While single commit is preferred, multiple commits are acceptable if troubleshooting requires incremental work.

9. **Timeline Flexibility:** Estimates provided are best-effort. Preview frameworks may introduce unexpected challenges requiring additional time.

10. **Rollback Readiness:** Keep main branch stable. Upgrade branch can be abandoned if blocking issues found.

---

**End of Migration Plan**

*This plan follows the Big Bang Strategy for rapid, coordinated upgrade of a small .NET solution to .NET 10.0 Preview. Execute phases sequentially, validate thoroughly, and maintain rollback readiness.*