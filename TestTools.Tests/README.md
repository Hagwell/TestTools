# TestTools.Tests

This project contains NUnit-based unit tests for the DHCW Test Tools application.

## How to Run Tests

1. Open a terminal in the `TestTools.Tests` directory.
2. Run all tests:
   ```powershell
   dotnet test --collect:"XPlat Code Coverage"
   ```
   This will run all tests and collect code coverage data.

3. Generate an HTML code coverage report:
   ```powershell
   reportgenerator -reports:"TestTools.Tests/TestResults/**/coverage.cobertura.xml" -targetdir:"TestTools.Tests/TestCoverageReport" -reporttypes:Html
   ```
   The HTML report will be available in the `TestCoverageReport` folder.

## Project Structure
- `TestTools.Tests.csproj`: NUnit test project file
- `UnitTest1.cs`: Example test file (replace with real tests)
- `.github/copilot-instructions.md`: Copilot custom instructions

## Best Practices
- Place each test class in its own file, named after the class under test (e.g., `HomeControllerTests.cs`).
- Use `[SetUp]` and `[TearDown]` for test initialization and cleanup.
- Use clear, descriptive test names.
- Mock dependencies where appropriate.
- Ensure all major logic paths are covered.

## Code Coverage
- Code coverage is collected using Coverlet and reported with ReportGenerator.
- The HTML report provides a user-friendly overview of test coverage.

## Requirements
- .NET 8.0 SDK
- NUnit
- Coverlet
- ReportGenerator

## Example Test Command
```powershell
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"TestTools.Tests/TestResults/**/coverage.cobertura.xml" -targetdir:"TestTools.Tests/TestCoverageReport" -reporttypes:Html
```
