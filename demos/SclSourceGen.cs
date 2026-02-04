// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

#:package Microsoft.Extensions.Hosting@10.0.2
#:project ../src/CliPlayground.SourceGen.Core/CliPlayground.SourceGen.Core.csproj
#:project ../src/CliPlayground.SourceGen/CliPlayground.SourceGen.csproj

using CliPlayground.SourceGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SclSourceGen;

var settings = new HostApplicationBuilderSettings() { Args = args };
var builder = Host.CreateEmptyApplicationBuilder(settings);
builder.Services.AddSingleton<GreetingService>();

Console.WriteLine("Hello World");

namespace SclSourceGen
{
    [Command("sayHello")]
    internal sealed partial class HelloCommand(GreetingService greetingService)
    {
        private readonly GreetingService _greetingService = greetingService;

        public Task ExecuteAsync(
            [Argument] string name = "World"
        )
        {
            _greetingService.Greet(name);
            return Task.CompletedTask;
        }
    }

    internal class GreetingService
    {
        public void Greet(string name) => Console.WriteLine($"Hello, {name}!");
    }
}
