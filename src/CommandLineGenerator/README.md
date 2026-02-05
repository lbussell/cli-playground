# CommandLineGenerator

A package that generates [System.CommandLine](https://github.com/dotnet/command-line-api) boilerplate for you.

## Features

- Generate CLI commands from simple C# classes with attributes
- Zero runtime reflection - everything is generated at compile time
- Native AOT compatible
- No runtime library dependency - all code is generated

## Usage

1. Add the package to your project:

```xml
<PackageReference Include="LoganBussell.CommandLineGenerator" Version="0.1.0" />
```

2. Create a command class with the `[Command]` attribute:

```csharp
[Command("hello", Description = "Say hello")]
public class HelloCommand
{
    [Argument(Description = "Name to greet")]
    public required string Name { get; set; }

    [Option("-c", "--count", Description = "Number of times to greet")]
    public int Count { get; set; } = 1;

    public Task<int> ExecuteAsync()
    {
        for (int i = 0; i < Count; i++)
        {
            Console.WriteLine($"Hello, {Name}!");
        }
        return Task.FromResult(0);
    }
}
```

3. Build your CLI app:

```csharp
return await ConsoleApp.RunAsync(args);
```

## How It Works

The source generator analyzes your command classes at compile time and generates:

- `[Command]`, `[Argument]`, and `[Option]` attributes
- `ConsoleApp` and `ConsoleAppBuilder` classes
- Command factory methods that wire up System.CommandLine

All generated code is visible in your IDE and can be inspected at `obj/Debug/net*/generated/CommandLineGenerator/`.

## License

MIT
