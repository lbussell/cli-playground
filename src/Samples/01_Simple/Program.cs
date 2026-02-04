// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

// Simple CLI without Microsoft.Extensions.Hosting
// Uses generated mapper directly with System.CommandLine

using System.CommandLine;
using SystemCommandLineGenerator;

var root = new RootCommand("Description");

// Add generated arguments and options
foreach (var arg in CommandLineOptionsMapper.Arguments) root.Add(arg);
foreach (var opt in CommandLineOptionsMapper.Options) root.Add(opt);

root.SetAction((parseResult, ct) =>
{
    var options = CommandLineOptionsMapper.Parse(parseResult);
    Console.WriteLine($"{options.Greeting}, {options.Name}!");
    return Task.CompletedTask;
});

return await root.Parse(args).InvokeAsync();

[MapCommandLineOptions]
internal sealed record CommandLineOptions([Argument] string Name, string Greeting = "Hello");
