// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace CommandLineGenerator;

/// <summary>
/// Represents metadata about a type decorated with [MapCommandLineOptions].
/// Contains all information needed to generate the options mapper.
/// </summary>
internal sealed record OptionsTypeInfo(
    string Namespace,
    string TypeName,
    string FullTypeName,
    ImmutableArray<OptionsMemberInfo> Members,
    bool UseKebabCase
);

/// <summary>
/// Represents metadata about a single option or argument member.
/// </summary>
internal sealed record OptionsMemberInfo(
    string PropertyName,
    string TypeName,
    string CliName,
    bool IsArgument,
    bool IsBoolean,
    bool IsNullable,
    bool HasDefaultValue,
    string? DefaultValue,
    string? Alias,
    string? Description
);

/// <summary>
/// Represents metadata about a method decorated with [Command].
/// </summary>
internal sealed record CommandMethodInfo(
    string ContainingNamespace,
    string ContainingClassName,
    string ContainingClassFullName,
    string MethodName,
    string CommandName,
    string? Description,
    ImmutableArray<string> OptionsTypeNames,
    bool IsAsync
)
{
    public bool IsRoot => string.IsNullOrEmpty(CommandName);
}
