// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace SystemCommandLineGenerator;

/// <summary>
/// Factory for creating CLI application builders.
/// </summary>
public static class ConsoleApp
{
    /// <summary>
    /// Creates a new CLI application builder.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A configured ConsoleAppBuilder.</returns>
    public static ConsoleAppBuilder CreateBuilder(string[] args) => new(args);
}
