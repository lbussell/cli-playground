// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;

namespace CommandLineGenerator;

/// <summary>
/// Utility functions for code generation.
/// </summary>
internal static class Utilities
{
    /// <summary>
    /// Converts a PascalCase name to kebab-case.
    /// </summary>
    public static string ToKebabCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Insert hyphen before each uppercase letter (except the first), then lowercase
        var result = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i > 0 && char.IsUpper(c))
            {
                result.Append('-');
            }
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }

    /// <summary>
    /// Escapes a string for use in generated C# string literals.
    /// </summary>
    public static string EscapeString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    /// <summary>
    /// Gets the fully qualified mapper class name for an options type.
    /// </summary>
    public static string GetMapperName(OptionsTypeInfo optType)
    {
        var hasNamespace =
            !string.IsNullOrEmpty(optType.Namespace) && optType.Namespace != "<global namespace>";
        return hasNamespace
            ? $"global::{optType.Namespace}.{optType.TypeName}Mapper"
            : $"global::{optType.TypeName}Mapper";
    }
}
