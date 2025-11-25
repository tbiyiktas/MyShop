# MyShop

## About

MyShop is a clean‑architecture sample e‑commerce application that demonstrates the **Specification pattern**, **factory‑based IncludeBuilder**, and a **layered architecture** (Domain, Application, Persistence, Web API, MVC). The recent refactor replaces fragile reflection with a type‑safe factory, improving testability and SOLID compliance.

**Recommendations**:
- **Add more unit tests** for edge‑cases of the new factory (e.g., null arguments, invalid includes).
- **Document the public API** (specifications, services) with XML comments and a generated API reference.
- **Introduce CI/CD** (GitHub Actions) to run `dotnet build` and `dotnet test` on every push.
- **Consider versioning** the specification package and publishing it as a NuGet library for reuse.

## Getting Started

```bash
# Clone the repository
git clone https://github.com/tbiyiktas/MyShop.git
cd MyShop

# Restore packages and build
dotnet restore
dotnet build

# Run the test suite
dotnet test tests/MyShop.Application.Tests/MyShop.Application.Tests.csproj
```

## Project Structure

- `src/MyShop.Domain` – domain entities and value objects.
- `src/MyShop.Application` – specifications, services, and abstractions.
- `src/MyShop.Persistence` – EF Core implementations and the `IncludeExpressionFactory`.
- `src/MyShop.WebApi` – minimal API exposing CRUD operations.
- `src/MyShop.WebMvc` – MVC front‑end consuming the Web API.
- `tests/MyShop.Application.Tests` – unit tests for specifications and builders.

## Build & Test

```bash
# Build all projects
dotnet build

# Run all tests
dotnet test
```

## Contributing

Feel free to open issues or submit pull requests. Follow the existing code‑style conventions and run the test suite before submitting changes.

## License

This project is licensed under the MIT License.