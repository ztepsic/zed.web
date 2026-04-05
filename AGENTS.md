# Project Guidelines

## Tech Stack

- .NET 10 / C# 14, nullable reference types enabled
- NuGet library published as `Zed.Web` — ASP.NET Core companion to the [`Zed`](https://www.nuget.org/packages/Zed) core library
- ASP.NET Core (`Microsoft.AspNetCore.App` framework reference)
- Dependencies: `Zed` (core library), `Newtonsoft.Json`
- Central Package Management (Directory.Packages.props)
- xUnit + Moq + `Zed.Test.Xunit` for testing, Coverlet for code coverage, JunitXml.TestLogger for CI reporting
- Conventional Commits enforced via CommitLint/Husky/Commitizen
- GitVersion for semantic versioning (ContinuousDeployment on `main`)
- GitHub Actions CI (`main.yml`, `publish.yml`) with reusable actions (`versioning`, `pack-nuget`, `publish-nuget`, `release-nuget`)
- `.slnx` solution format
- OpenSpec workflow configured (`openspec/config.yaml`)

## Architecture

This is an ASP.NET Core utility library providing extensions and helpers that bridge the `Zed` core library with ASP.NET Core:

- **Extensions**: `FluentResultExtensions` — converts FluentResults `Result`/`Result<T>` failures into ASP.NET Core `ProblemDetails` responses, handling `HttpStatusCodeAppError`, `ValidationError`, and generic errors

Legacy code (`_old/`) contains the original ASP.NET MVC 5 implementation with additional features (HtmlHelper extensions, routing helpers, UnitOfWork filters, models) pending migration.

> **Note:** The test project (`Zed.Web.Tests/`) is scaffolded but contains no test files yet.

## Code Style

- XML documentation (`///`) on all public members with `<summary>`, `<param>`, `<returns>`, `<typeparam>` tags
- Organize class internals with `#region` blocks (Fields, Constructors, Methods, etc.)
- Private fields use `camelCase` (no underscore prefix)
- Null-guard with throw expressions: `param ?? throw new ArgumentNullException(nameof(param))`

## Build and Test

```sh
# Build
dotnet build

# Run tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

## Test Conventions

- Test classes: `[Subject]Tests` (e.g., `FluentResultExtensionsTests`)
- Test methods: `[Method]_[Scenario]_[ExpectedResult]` (e.g., `ToProblemDetails_Returns_BadRequest_For_ValidationError`)
- Use `[Fact]` for single cases, `[Theory]` for parameterized
- Follow Arrange-Act-Assert pattern

## Commits

Follow [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `perf:`, `refactor:`, `docs:`, `test:`, `chore:`, `ci:`, `build:`, `style:`, `revert:`
