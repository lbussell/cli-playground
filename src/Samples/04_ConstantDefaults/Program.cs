// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

// Test case for constant default values (Issue: Default value with constants does not work)
//
// NOTE: This test demonstrates working scenarios with constants:
// 1. Constants from static classes (TestConstants.DefaultValue1)
// 2. Constants from the same type (MyOptions.LocalConstant)
// 3. Various types: string, int, bool
//
// Top-level constants (const string x = "val") cannot be used as parameter defaults
// for types declared at file scope - this is a C# language limitation, not a generator issue.

using System.CommandLine;
using CommandLineGenerator;

var myCommand = new RootCommand(description: "Test constant default values");
myCommand.AddOptions<MyOptions>();
myCommand.SetAction(
    (parseResult, ct) =>
    {
        var options = MyOptionsMapper.Parse(parseResult);
        Console.WriteLine($"Option1: '{options.FailingOption1}'");
        Console.WriteLine($"Option2: '{options.FailingOption2}'");
        Console.WriteLine($"Number: {options.NumberOption}");
        Console.WriteLine($"Flag: {options.FlagOption}");
        Console.WriteLine($"Double: {options.DoubleOption}");
        return Task.CompletedTask;
    }
);

return await myCommand.Parse(args).InvokeAsync();

// Constants at file scope
static class TestConstants
{
    public const string DefaultValue1 = "foo";
    public const int DefaultNumber = 42;
    public const bool DefaultFlag = true;
    public const double DefaultDouble = 3.14;
}

// Test using constants from static class and local const
[MapCommandLineOptions]
record MyOptions(
    [Option(Description = "Option with constant from static class")] string FailingOption1 = TestConstants.DefaultValue1,
    [Option(Description = "Option with constant from same type")] string FailingOption2 = MyOptions.LocalConstant,
    [Option(Description = "Number option with constant")] int NumberOption = TestConstants.DefaultNumber,
    [Option(Description = "Boolean option with constant")] bool FlagOption = TestConstants.DefaultFlag,
    [Option(Description = "Double option with constant")] double DoubleOption = TestConstants.DefaultDouble
)
{
    public const string LocalConstant = "bar";
}
