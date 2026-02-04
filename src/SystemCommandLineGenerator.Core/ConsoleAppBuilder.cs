// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SystemCommandLineGenerator;

/// <summary>
/// Builder for CLI applications with integrated dependency injection.
/// </summary>
public sealed class ConsoleAppBuilder
{
    private readonly string[] _args;
    private readonly List<CommandRegistration> _commands = [];
    private CommandRegistration? _rootCommand;

    internal ConsoleAppBuilder(string[] args)
    {
        _args = args;
        var settings = new HostApplicationBuilderSettings { Args = args };
        Host = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(settings);
    }

    /// <summary>
    /// The underlying host application builder. Use to configure services, configuration, etc.
    /// </summary>
    public HostApplicationBuilder Host { get; }

    /// <summary>
    /// Registers a command handler with the CLI application.
    /// The command type must have a [Command] attribute and an ExecuteAsync method.
    /// </summary>
    /// <typeparam name="T">The command handler type.</typeparam>
    public ConsoleAppBuilder AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T
    >()
        where T : class
    {
        var commandType = typeof(T);

        if (!GeneratedCommandRegistry.TryGetFactory(commandType, out var entry))
        {
            throw new InvalidOperationException(
                $"Type '{commandType.FullName}' is not a registered command. " +
                "Ensure it has the [Command] attribute and an ExecuteAsync method."
            );
        }

        Host.Services.AddTransient<T>();

        var registration = new CommandRegistration(entry.CommandName, entry.IsRoot, entry.Factory);

        if (entry.IsRoot)
        {
            if (_rootCommand is not null)
                throw new InvalidOperationException("Only one root command can be registered.");
            _rootCommand = registration;
        }
        else
        {
            _commands.Add(registration);
        }

        return this;
    }

    /// <summary>
    /// Builds the host, resolves commands, and runs the CLI application.
    /// </summary>
    /// <returns>The exit code from the CLI invocation.</returns>
    public async Task<int> RunAsync()
    {
        // Register the RootCommand factory
        Host.Services.AddSingleton<RootCommand>(sp =>
        {
            RootCommand root;

            if (_rootCommand is not null)
            {
                root = (RootCommand)_rootCommand.Factory(sp);
            }
            else
            {
                root = new RootCommand();
            }

            // Attach all subcommands
            foreach (var cmd in _commands)
            {
                root.Subcommands.Add(cmd.Factory(sp));
            }

            return root;
        });

        var host = Host.Build();
        var rootCommand = host.Services.GetRequiredService<RootCommand>();

        return await rootCommand.Parse(_args).InvokeAsync();
    }

    private sealed record CommandRegistration(
        string CommandName,
        bool IsRoot,
        Func<IServiceProvider, Command> Factory
    );
}

/// <summary>
/// Registry for generated command factories. Populated by generated code.
/// </summary>
public static class GeneratedCommandRegistry
{
    private static readonly Dictionary<Type, CommandEntry> s_commands = new();

    /// <summary>
    /// Registers a command factory. Called by generated code.
    /// </summary>
    public static void Register(Type commandType, string commandName, bool isRoot, Func<IServiceProvider, Command> factory)
    {
        s_commands[commandType] = new CommandEntry(commandName, isRoot, factory);
    }

    /// <summary>
    /// Tries to get a command factory for the given type.
    /// </summary>
    public static bool TryGetFactory(Type commandType, out CommandEntry entry)
    {
        return s_commands.TryGetValue(commandType, out entry!);
    }

    /// <summary>
    /// Entry containing command metadata and factory.
    /// </summary>
    public sealed record CommandEntry(string CommandName, bool IsRoot, Func<IServiceProvider, Command> Factory);
}
