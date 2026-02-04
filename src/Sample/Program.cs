// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using SystemCommandLineGenerator;

var builder = ConsoleApp.CreateBuilder(args);
builder.Host.Services.AddSingleton<GreetingService>();
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

// Command class with multiple commands

internal sealed class Commands(GreetingService greetingService)
{
    private readonly GreetingService _greetingService = greetingService;

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
        _greetingService.Greet(options.Name, options.Greeting);
        return Task.CompletedTask;
    }

    [Command("goodbye", Description = "Say goodbye to someone")]
    public Task GoodbyeAsync(GoodbyeOptions options)
    {
        _greetingService.SayGoodbye(options.Name);
        return Task.CompletedTask;
    }
}

internal sealed class GreetingService
{
    public void Greet(string name, string greeting = "Hello") => Console.WriteLine($"{greeting}, {name}!");
    public void SayGoodbye(string name) => Console.WriteLine($"Goodbye, {name}!");
}
