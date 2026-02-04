# Copilot Instructions

## Build & Run

```bash
# Build
dotnet build

# Run sample CLI
dotnet run --project src/Sample -- hello World
dotnet run --project src/Sample -- goodbye

# Format code
./format.ps1              # or: dotnet format && dotnet csharpier format .
```

## Architecture

This is a **source generator** project that generates CLI infrastructure from attributed classes.

### Projects

- **SystemCommandLineGenerator** - Roslyn incremental source generator (targets `netstandard2.0`)
- **Sample** - Example CLI app consuming the generator

### How It Works

1. User writes command classes with `[Command]` attribute and `ExecuteAsync` method
2. Generator produces `ConsoleApp`, `ConsoleAppBuilder`, and attributes - **everything is generated, no runtime library**
3. Generated `AddCommand<T>()` uses type switch to wire up each command

See `src/Sample/Program.cs` for usage examples.

### Generated Output

View generated files at: `src/Sample/obj/Debug/net10.0/generated/SystemCommandLineGenerator/`

- `Attributes.g.cs` - `[Command]`, `[Argument]`, `[Option]` attributes
- `ConsoleApp.g.cs` - `ConsoleApp`, `ConsoleAppBuilder`, command factories

## Key Conventions

### Source Generator Tenets

The purpose of generating code is to avoid writing boilerplate, not to hide bad or ugly code. Generated code:

- Is always easy to read and understand
- Is always of the same or higher quality than hand-written code
- Never uses `unsafe`
- Never uses reflection
- Always uses nullable annotations
- Never uses null-forgiving operators (`!`)
- Follows all other .NET best practices
- Always supports Native AOT

### Source Generator Patterns

- Use `ForAttributeWithMetadataName` for efficient attribute-based discovery
- Use `static` lambdas in pipeline to avoid state capture
- Use `IndentingBuilder` (from StaticCS) for code generation
- Handle global namespace: check for `"<global namespace>"` when building type names
- See `docs/SourceGeneratorGuide.md` for detailed patterns

### System.CommandLine 2.0.0-beta5+ API

This project uses the new System.CommandLine APIs (beta5+, not beta4). Key differences:
- `command.SetAction()` not `SetHandler()`
- `command.Arguments.Add()` not `AddArgument()`
- Name is required for all symbol constructors
- `DefaultValueFactory` property not `SetDefaultValue()`

See `.github/skills/system-commandline/SKILL.md` for complete migration guide.

### Project Structure

- Central package management via `src/Directory.Packages.props`
- Shared build props via `src/Directory.Build.props`
- `EmitCompilerGeneratedFiles=true` to inspect generated code
- `TreatWarningsAsErrors=true` on all projects
