---
name: dotnet-scaffold
description: Scaffold new .NET projects using the dotnet CLI. Use when the user wants to create a new .NET solution, add projects (console, web API, class library, test project, etc.), or set up a standard .NET repository structure. Triggers on requests like "create a new .NET project", "scaffold a C# solution", "set up a dotnet console app with tests".
---

# .NET Project Scaffolding

Scaffold .NET projects using `dotnet new` templates with a consistent structure.

## Standard Structure

```
Repo
├── .editorconfig
├── .gitignore
├── MyProject.slnx
└── src/
    ├── Directory.Build.props
    ├── Directory.Packages.props
    ├── MyProject/
    │   └── MyProject.csproj
    └── MyProject.Tests/
        └── MyProject.Tests.csproj
```

## Workflow

### 1. Initialize Repository Root

```bash
dotnet new editorconfig
dotnet new gitignore
dotnet new sln -n "SolutionName" -f slnx
mkdir src
```

### 2. Create Directory.Build.props

Create `src/Directory.Build.props` with shared build settings:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

All projects under `src/` inherit these properties. Remove `<TargetFramework>`, `<Nullable>`, and `<ImplicitUsings>` from individual `.csproj` files after scaffolding.

### 3. Add Projects

Use `--dry-run` first to verify, then run without it.

```bash
# Console app
dotnet new console -n "MyApp" -f net10.0 -o src/MyApp --dry-run
dotnet new console -n "MyApp" -f net10.0 -o src/MyApp

# Class library
dotnet new classlib -n "MyLib" -f net10.0 -o src/MyLib

# Test project (mstest, xunit, nunit)
dotnet new mstest -n "MyApp.Tests" -f net10.0 -o src/MyApp.Tests
```

Common templates: `console`, `classlib`, `webapi`, `web`, `blazor`, `worker`, `mstest`, `xunit`, `nunit`.

Run `dotnet new list` for all available templates.

### 4. Add Projects to Solution

```bash
dotnet sln SolutionName.slnx add src/MyApp/MyApp.csproj --in-root
dotnet sln SolutionName.slnx add src/MyApp.Tests/MyApp.Tests.csproj --in-root
```

### 5. Add Project References

Test projects typically reference the project under test:

```bash
dotnet add src/MyApp.Tests/MyApp.Tests.csproj reference src/MyApp/MyApp.csproj
```

### 6. Central Package Management (if >1 project)

Create `src/Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <!-- Add shared package versions here -->
    <!-- <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" /> -->
  </ItemGroup>
</Project>
```

In each `.csproj`, use `<PackageReference Include="PackageName" />` without `Version`.

## Notes

- Target framework: Set in `src/Directory.Build.props` (use `net10.0` or latest preview)
- Solution format: Always `-f slnx` (XML-based format)
- Project location: Always under `src/` via `-o src/ProjectName`
- Solution references: Always `--in-root` to avoid solution folders
- After scaffolding: Remove `TargetFramework`, `Nullable`, `ImplicitUsings` from individual `.csproj` files (inherited from `Directory.Build.props`)
