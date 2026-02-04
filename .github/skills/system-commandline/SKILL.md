---
name: system-commandline
description: Guide for System.CommandLine 2.0.0-beta5+ and GA. Use when writing CLI apps with System.CommandLine, migrating from beta4, or reviewing code using the new APIs. Covers breaking changes, new patterns for options/arguments/commands, actions, parsing, and configuration.
---

# System.CommandLine 2.0.0-beta5+ Migration Guide

This skill covers **breaking changes** from System.CommandLine 2.0.0-beta5 and later (including GA). Use this when writing new CLI code or migrating from beta4.

## Key Breaking Changes Summary

| Area | Old (beta4) | New (beta5+) |
|------|-------------|--------------|
| Adding options/args | `Command.AddOption()` | `Command.Options.Add()` |
| Handler | `Command.SetHandler()` | `Command.SetAction()` |
| Name parameter | Optional (derived from alias) | **Required** for all symbols |
| Default values | `SetDefaultValue(object)` | `DefaultValueFactory` property |
| Custom parsing | `ParseArgument<T>` delegate | `CustomParser` property |
| Parser class | `Parser` | `CommandLineParser` (static) |
| Configuration | `CommandLineBuilder` | `ParserConfiguration` / `InvocationConfiguration` |
| Console abstraction | `IConsole` | `TextWriter` (`Output`/`Error`) |

---

## Renaming

| Old Name | New Name |
|----------|----------|
| `Parser` | `CommandLineParser` |
| `OptionResult.IsImplicit` | `OptionResult.Implicit` |
| `Option.IsRequired` | `Option.Required` |
| `Symbol.IsHidden` | `Symbol.Hidden` |
| `Option.ArgumentHelpName` | `Option.HelpName` |
| `OptionResult.Token` | `OptionResult.IdentifierToken` |
| `ParseResult.FindResultFor` | `ParseResult.GetResult` |
| `SymbolResult.ErrorMessage` | `SymbolResult.AddError(string)` |

---

## Mutable Collections (No More Add Methods)

Old `Add*` methods are replaced with mutable collection properties:

```csharp
// OLD (beta4)
command.AddOption(myOption);
command.AddArgument(myArgument);
command.AddCommand(subcommand);
command.AddValidator(validator);
command.AddAlias("alias");
option.AddCompletions("a", "b", "c");

// NEW (beta5+)
command.Options.Add(myOption);
command.Arguments.Add(myArgument);
command.Subcommands.Add(subcommand);
command.Validators.Add(validator);
command.Aliases.Add("alias");
option.CompletionSources.Add("a", "b", "c");
```

**Alias methods removed:**
- `RemoveAlias()` → Use `Aliases.Remove()`
- `HasAlias()` → Use `Aliases.Contains()`

---

## Names and Aliases

**Name is now mandatory** for all symbol constructors (`Argument<T>`, `Option<T>`, `Command`).

```csharp
// OLD (beta4) - name derived from longest alias
Option<bool> option = new("--verbose", "-v");

// NEW (beta5+) - name is first parameter, aliases are separate
Option<bool> option = new("--verbose", "-v")
{
    Description = "Enable verbose output"
};
```

**⚠️ BREAKING: Description parameter removed from constructor**

```csharp
// OLD (beta4) - second param was description
Option<bool> beta4 = new("--help", "An option with description.");

// NEW (beta5+) - second param is alias! Set Description separately
Option<bool> beta5 = new("--help", "-h", "/h")
{
    Description = "An option with description."
};
```

**Get parsed values by name:**

```csharp
RootCommand command = new("The description.")
{
    new Option<int>("--number")
};

ParseResult parseResult = command.Parse(args);
int number = parseResult.GetValue<int>("--number");
```

---

## Default Values and Custom Parsing

**Old approach (not type-safe):**
```csharp
// OLD (beta4)
option.SetDefaultValue("text"); // object, not type-safe!
```

**New approach (type-safe):**
```csharp
// NEW (beta5+) - DefaultValueFactory property
Option<int> number = new("--number")
{
    DefaultValueFactory = _ => 42
};

// NEW (beta5+) - CustomParser property
Argument<Uri> uri = new("uri")
{
    CustomParser = result =>
    {
        if (!Uri.TryCreate(result.Tokens.Single().Value, UriKind.RelativeOrAbsolute, out var uriValue))
        {
            result.AddError("Invalid URI format.");
            return null!;
        }
        return uriValue;
    }
};
```

---

## Parsing and Invocation

**Parsing:**
```csharp
// OLD (beta4) - extension method
ParseResult result = CommandExtensions.Parse(command, args);

// NEW (beta5+) - instance method on Command
ParseResult result = command.Parse(args);

// With configuration
var config = new ParserConfiguration { EnablePosixBundling = false };
ParseResult result = command.Parse(args, config);
```

**Invocation:**
```csharp
// OLD (beta4)
command.SetHandler((FileInfo file) => { /* ... */ }, fileOption);
await command.InvokeAsync(args);

// NEW (beta5+) - SetAction + ParseResult.Invoke
command.SetAction(parseResult =>
{
    FileInfo? file = parseResult.GetValue(fileOption);
    // ... handle
});

ParseResult result = command.Parse(args);
return result.Invoke(); // or await result.InvokeAsync();
```

**Async actions require CancellationToken:**
```csharp
// NEW (beta5+) - CancellationToken is mandatory for async
command.SetAction(async (ParseResult parseResult, CancellationToken token) =>
{
    string? url = parseResult.GetValue(urlOption);
    return await DoWorkAsync(url, token);
});
```

---

## Configuration

**Old `CommandLineBuilder` pattern removed.** Use mutable configuration classes:

```csharp
// Parser configuration
var parserConfig = new ParserConfiguration
{
    EnablePosixBundling = true,  // default: true
    ResponseFileTokenReplacer = null  // disable response files
};

// Invocation configuration
var invocationConfig = new InvocationConfiguration
{
    ProcessTerminationTimeout = TimeSpan.FromSeconds(2),  // default, set null to disable
    EnableDefaultExceptionHandler = true,  // default
    Output = Console.Out,
    Error = Console.Error
};
```

**CommandLineBuilderExtensions mappings:**

| Old Extension | New Approach |
|---------------|--------------|
| `CancelOnProcessTermination()` | `InvocationConfiguration.ProcessTerminationTimeout` |
| `EnablePosixBundling()` | `ParserConfiguration.EnablePosixBundling` |
| `UseExceptionHandler()` | `InvocationConfiguration.EnableDefaultExceptionHandler` |
| `EnableDirectives()` | `RootCommand.Directives` collection |
| `UseHelp()` / `UseVersion()` | Included by default in `RootCommand` |
| `UseTokenReplacer()` | `ParserConfiguration.ResponseFileTokenReplacer` |
| `AddMiddleware()` | Removed (no replacement) |

---

## Directives

```csharp
// Directives are now a collection on RootCommand
var root = new RootCommand();

// SuggestDirective included by default
// Add others as needed:
root.Directives.Add(new DiagramDirective());
root.Directives.Add(new EnvironmentVariablesDirective());
```

---

## Console Abstraction Removed

```csharp
// OLD (beta4) - IConsole interface
void Handler(InvocationContext context)
{
    context.Console.WriteLine("Hello");
}

// NEW (beta5+) - Use TextWriter directly
var config = new InvocationConfiguration
{
    Output = new StringWriter(),  // for testing
    Error = Console.Error
};

// Or just use Console.WriteLine in your action
```

---

## InvocationContext Removed

```csharp
// OLD (beta4)
command.SetHandler(async (InvocationContext context) =>
{
    var value = context.ParseResult.GetValueForOption(option);
    var token = context.GetCancellationToken();
    // ...
});

// NEW (beta5+) - ParseResult and CancellationToken passed directly
command.SetAction(async (ParseResult parseResult, CancellationToken token) =>
{
    var value = parseResult.GetValue(option);
    // ...
});
```

---

## Complete Example

```csharp
using System.CommandLine;

var fileOption = new Option<FileInfo>("--file", "-f")
{
    Description = "The file to process",
    Required = true
};

var verboseOption = new Option<bool>("--verbose", "-v")
{
    Description = "Enable verbose output"
};

var rootCommand = new RootCommand("Process files")
{
    fileOption,
    verboseOption
};

rootCommand.SetAction(parseResult =>
{
    var file = parseResult.GetValue(fileOption);
    var verbose = parseResult.GetValue(verboseOption);
    
    if (verbose)
        Console.WriteLine($"Processing {file?.FullName}");
    
    // Process file...
    return 0;
});

var result = rootCommand.Parse(args);
return result.Invoke();
```

---

## Quick Reference

**Creating symbols:**
```csharp
var arg = new Argument<string>("name") { Description = "..." };
var opt = new Option<int>("--count", "-c") { Description = "..." };
var cmd = new Command("sub", "Description");
```

**Building command tree:**
```csharp
cmd.Options.Add(opt);
cmd.Arguments.Add(arg);
rootCommand.Subcommands.Add(cmd);
```

**Setting action:**
```csharp
cmd.SetAction(parseResult => { /* sync */ });
cmd.SetAction(async (parseResult, token) => { /* async */ });
```

**Parsing and invoking:**
```csharp
var result = command.Parse(args);
return result.Invoke();  // or await result.InvokeAsync()
```
