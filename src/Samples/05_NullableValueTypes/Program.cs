// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

// Test case for nullable value types (int?, double?, DateTime?, etc.)
//
// This sample demonstrates:
// 1. Nullable value types like int? where null is distinct from any valid value
// 2. Options that don't require values (not marked as Required)
// 3. Proper handling of null vs. 0 or other default values

using System.CommandLine;
using CommandLineGenerator;

var myCommand = new RootCommand(description: "Test nullable value types");
myCommand.AddOptions<QueryOptions>();
myCommand.SetAction(
    (parseResult, ct) =>
    {
        var options = QueryOptionsMapper.Parse(parseResult);
        Console.WriteLine(
            $"Limit: {(options.Limit.HasValue ? options.Limit.Value.ToString() : "null (no limit)")}"
        );
        Console.WriteLine(
            $"Timeout: {(options.Timeout.HasValue ? options.Timeout.Value.ToString() + " seconds" : "null (no timeout)")}"
        );
        Console.WriteLine(
            $"MaxPrice: {(options.MaxPrice.HasValue ? "$" + options.MaxPrice.Value.ToString("F2") : "null (no max price)")}"
        );
        Console.WriteLine(
            $"MinDate: {(options.MinDate.HasValue ? options.MinDate.Value.ToString("yyyy-MM-dd") : "null (no min date)")}"
        );
        return Task.CompletedTask;
    }
);

return await myCommand.Parse(args).InvokeAsync();

[MapCommandLineOptions]
internal sealed record QueryOptions(
    [Option(Alias = "-n", Description = "Limit number of results")] int? Limit,
    [Option(Alias = "-t", Description = "Timeout in seconds")] int? Timeout,
    [Option(Alias = "-p", Description = "Maximum price filter")] double? MaxPrice,
    [Option(Alias = "-d", Description = "Minimum date filter (yyyy-MM-dd)")] DateTime? MinDate
);
