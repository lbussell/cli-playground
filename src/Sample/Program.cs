// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using SystemCommandLineGenerator;

var builder = ConsoleApp.CreateBuilder(args);
builder.Host.Services.AddSingleton<GreetingService>();
builder.AddCommand<AppRootCommand>();
builder.AddCommand<HelloCommand>();
builder.AddCommand<GoodbyeCommand>();

return await builder.RunAsync();

[Command("")]
internal sealed class AppRootCommand
{
    public Task ExecuteAsync()
    {
        Console.WriteLine("Welcome! Use 'hello' or 'goodbye' subcommands.");
        return Task.CompletedTask;
    }
}

[Command("hello")]
internal sealed class HelloCommand(GreetingService greetingService)
{
    private readonly GreetingService _greetingService = greetingService;

    public Task ExecuteAsync([Argument] string name)
    {
        _greetingService.Greet(name);
        return Task.CompletedTask;
    }
}

[Command("goodbye")]
internal sealed class GoodbyeCommand(GreetingService greetingService)
{
    private readonly GreetingService _greetingService = greetingService;

    public Task ExecuteAsync([Argument] string name)
    {
        _greetingService.SayGoodbye(name);
        return Task.CompletedTask;
    }
}

internal sealed class GreetingService
{
    public void Greet(string name) => Console.WriteLine($"Hello, {name}!");
    public void SayGoodbye(string name) => Console.WriteLine($"Goodbye, {name}!");
}
