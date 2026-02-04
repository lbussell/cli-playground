// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

// CLI with Microsoft.Extensions.Hosting
// Uses ConsoleAppBuilder for the hosting infrastructure

using SystemCommandLineGenerator;

var builder = ConsoleApp.CreateBuilder(args);
builder.AddCommand<Commands>();

return await builder.RunAsync();

// Options records

[MapCommandLineOptions]
internal record HelloOptions(
    [Argument] string Name,
    string Greeting = "Hello"
);

[MapCommandLineOptions]
internal record GoodbyeOptions(
    [Argument] string Name
);

[MapCommandLineOptions]
internal record VerboseOptions(
    [Option(Alias = "-v", Description = "Enable verbose output")]
    bool Verbose = false
);

// Command class

internal sealed class Commands
{
    [Command("", Description = "Root command - shows welcome message")]
    public Task RootAsync()
    {
        Console.WriteLine("Welcome! Use 'hello' or 'goodbye' subcommands.");
        return Task.CompletedTask;
    }

    [Command("hello", Description = "Greet someone")]
    public Task HelloAsync(HelloOptions options, VerboseOptions verbose)
    {
        if (verbose.Verbose)
        {
            Console.WriteLine($"[DEBUG] Greeting: {options.Greeting}, Name: {options.Name}");
        }
        Console.WriteLine($"{options.Greeting}, {options.Name}!");
        return Task.CompletedTask;
    }

    [Command("goodbye", Description = "Say goodbye to someone")]
    public Task GoodbyeAsync(GoodbyeOptions options)
    {
        Console.WriteLine($"Goodbye, {options.Name}!");
        return Task.CompletedTask;
    }
}
