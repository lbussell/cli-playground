// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommandLineGenerator.Parsing;

/// <summary>
/// Parses types decorated with [MapCommandLineOptions] into OptionsTypeInfo models.
/// </summary>
internal static class OptionsTypeParser
{
    private const string MapCommandLineOptionsAttributeName =
        "CommandLineGenerator.MapCommandLineOptionsAttribute";
    private const string ArgumentAttributeName = "CommandLineGenerator.ArgumentAttribute";
    private const string OptionAttributeName = "CommandLineGenerator.OptionAttribute";

    /// <summary>
    /// Extracts metadata from a type decorated with [MapCommandLineOptions].
    /// </summary>
    public static OptionsTypeInfo? Parse(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var mapAttr = ctx.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == MapCommandLineOptionsAttributeName
        );

        if (mapAttr is null)
            return null;

        // Check for UseKebabCase property
        var useKebabCase = true;
        foreach (var namedArg in mapAttr.NamedArguments)
        {
            if (namedArg.Key == "UseKebabCase" && namedArg.Value.Value is bool val)
            {
                useKebabCase = val;
            }
        }

        // Get members from primary constructor parameters (for records) or properties
        var members = new List<OptionsMemberInfo>();

        // Check for primary constructor (records and classes with primary constructors)
        var primaryCtor = typeSymbol.InstanceConstructors.FirstOrDefault(c =>
            c.Parameters.Length > 0
            && c.DeclaringSyntaxReferences.Any(r =>
                r.GetSyntax(ct) is RecordDeclarationSyntax or ClassDeclarationSyntax
            )
        );

        if (primaryCtor is not null)
        {
            foreach (var param in primaryCtor.Parameters)
            {
                var memberInfo = ExtractMemberInfo(param, useKebabCase);
                members.Add(memberInfo);
            }
        }
        else
        {
            // Fall back to public properties with public setters or init
            foreach (var prop in typeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (prop.DeclaredAccessibility != Accessibility.Public)
                    continue;
                if (
                    prop.SetMethod is null
                    || prop.SetMethod.DeclaredAccessibility != Accessibility.Public
                )
                    continue;

                var memberInfo = ExtractMemberInfoFromProperty(prop, useKebabCase);
                members.Add(memberInfo);
            }
        }

        var ns = typeSymbol.ContainingNamespace.ToDisplayString();
        var fullTypeName =
            string.IsNullOrEmpty(ns) || ns == "<global namespace>"
                ? $"global::{typeSymbol.Name}"
                : $"global::{ns}.{typeSymbol.Name}";

        return new OptionsTypeInfo(
            ns,
            typeSymbol.Name,
            fullTypeName,
            members.ToImmutableArray(),
            useKebabCase
        );
    }

    private static OptionsMemberInfo ExtractMemberInfo(IParameterSymbol param, bool useKebabCase)
    {
        var isArgument = false;
        string? explicitName = null;
        string? alias = null;
        string? description = null;

        foreach (var attr in param.GetAttributes())
        {
            var attrName = attr.AttributeClass?.ToDisplayString();

            if (attrName == ArgumentAttributeName)
            {
                isArgument = true;
                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "Name")
                        explicitName = namedArg.Value.Value as string;
                    if (namedArg.Key == "Description")
                        description = namedArg.Value.Value as string;
                }
            }
            else if (attrName == OptionAttributeName)
            {
                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "Name")
                        explicitName = namedArg.Value.Value as string;
                    if (namedArg.Key == "Alias")
                        alias = namedArg.Value.Value as string;
                    if (namedArg.Key == "Description")
                        description = namedArg.Value.Value as string;
                }
            }
        }

        var cliName =
            explicitName ?? (useKebabCase ? Utilities.ToKebabCase(param.Name) : param.Name);
        var isBoolean = param.Type.SpecialType == SpecialType.System_Boolean;

        return new OptionsMemberInfo(
            param.Name,
            param.Type.ToDisplayString(),
            cliName,
            isArgument,
            isBoolean,
            param.NullableAnnotation == NullableAnnotation.Annotated,
            param.HasExplicitDefaultValue,
            param.HasExplicitDefaultValue
                ? FormatDefaultValue(param.ExplicitDefaultValue, param.Type)
                : null,
            alias,
            description
        );
    }

    private static OptionsMemberInfo ExtractMemberInfoFromProperty(
        IPropertySymbol prop,
        bool useKebabCase
    )
    {
        var isArgument = false;
        string? explicitName = null;
        string? alias = null;
        string? description = null;

        foreach (var attr in prop.GetAttributes())
        {
            var attrName = attr.AttributeClass?.ToDisplayString();

            if (attrName == ArgumentAttributeName)
            {
                isArgument = true;
                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "Name")
                        explicitName = namedArg.Value.Value as string;
                    if (namedArg.Key == "Description")
                        description = namedArg.Value.Value as string;
                }
            }
            else if (attrName == OptionAttributeName)
            {
                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "Name")
                        explicitName = namedArg.Value.Value as string;
                    if (namedArg.Key == "Alias")
                        alias = namedArg.Value.Value as string;
                    if (namedArg.Key == "Description")
                        description = namedArg.Value.Value as string;
                }
            }
        }

        var cliName = explicitName ?? (useKebabCase ? Utilities.ToKebabCase(prop.Name) : prop.Name);
        var isBoolean = prop.Type.SpecialType == SpecialType.System_Boolean;

        // Properties don't have default values in the same way; we'd need to analyze initializers
        return new OptionsMemberInfo(
            prop.Name,
            prop.Type.ToDisplayString(),
            cliName,
            isArgument,
            isBoolean,
            prop.NullableAnnotation == NullableAnnotation.Annotated,
            false,
            null,
            alias,
            description
        );
    }

    private static string FormatDefaultValue(object? value, ITypeSymbol type)
    {
        if (value is null)
            return "null";

        if (type.SpecialType == SpecialType.System_String)
            return $"\"{value}\"";

        if (type.SpecialType == SpecialType.System_Boolean)
            return value.ToString()!.ToLowerInvariant();

        return value.ToString()!;
    }
}
