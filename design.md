# New System.CommandLine Source Generator Design

### Before

```cs
[Command("hello")]
class HelloCommand(GreetingService greetingService)
{
    private readonly GreetingService _greetingService = greetingService;

    public Task ExecuteAsync([Argument] string name)
    {
        _greetingService.Greet(name);
        return Task.CompletedTask;
    }
}
```

### After

```cs
[MapCommandLineOptions]
record HelloOptions(
    [Argument]
    // Maps to 'name' argument.
    // Arguments always require annotations.
    string Name,
    // By default (with no annotations), use options. Maps to '--greeting' option.
    // Since there is no default value, this option is required.
    string Greeting);

class HelloCommand(GreetingService greetingService)
{
    private readonly GreetingService _greetingService = greetingService;

    [Command("hello")]
    // Method can be named anything.
    public Task RunMyCommandAsync(HelloOptions options)
    {
        _greetingService.Greet(options.Name);
        return Task.CompletedTask;
    }

    [Command("hello2")]
    // Can have multiple commands in the same class.
    // Enable option composition by adding more options classes as parameters.
    public Task RunMySecondCommandAsync(HelloOptions options, SomeOtherOptions otherOptions)
    {
        // ...
    }
}

[MapCommandLineOptions]
record SomeOtherOptions(string Foo = "bar");
```
