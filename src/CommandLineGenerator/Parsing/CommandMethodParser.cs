// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CommandLineGenerator.Parsing;

/// <summary>
/// Parses methods decorated with [Command] into CommandMethodInfo models.
/// </summary>
internal static class CommandMethodParser
{
    private const string CommandAttributeName = "CommandLineGenerator.CommandAttribute";

    /// <summary>
    /// Extracts metadata from a method decorated with [Command].
    /// </summary>
    public static CommandMethodInfo? Parse(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken ct
    )
    {
        if (ctx.TargetSymbol is not IMethodSymbol methodSymbol)
            return null;

        var commandAttr = ctx.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == CommandAttributeName
        );

        if (commandAttr is null)
            return null;

        var commandName =
            commandAttr.ConstructorArguments.Length > 0
                ? commandAttr.ConstructorArguments[0].Value as string ?? ""
                : "";

        string? description = null;
        foreach (var namedArg in commandAttr.NamedArguments)
        {
            if (namedArg.Key == "Description")
                description = namedArg.Value.Value as string;
        }

        var containingType = methodSymbol.ContainingType;
        var containingNs = containingType.ContainingNamespace.ToDisplayString();
        var containingFullName =
            string.IsNullOrEmpty(containingNs) || containingNs == "<global namespace>"
                ? $"global::{containingType.Name}"
                : $"global::{containingNs}.{containingType.Name}";

        // Extract parameter types (should be options types)
        var optionsTypeNames = methodSymbol
            .Parameters.Select(p =>
            {
                var ns = p.Type.ContainingNamespace?.ToDisplayString() ?? "";
                return string.IsNullOrEmpty(ns) || ns == "<global namespace>"
                    ? $"global::{p.Type.Name}"
                    : $"global::{ns}.{p.Type.Name}";
            })
            .ToImmutableArray();

        // Check if method returns Task (async) or void (sync)
        var returnType = methodSymbol.ReturnType;
        var isAsync =
            returnType.Name == "Task"
            && returnType.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";

        return new CommandMethodInfo(
            containingNs,
            containingType.Name,
            containingFullName,
            methodSymbol.Name,
            commandName,
            description,
            optionsTypeNames,
            isAsync
        );
    }
}
