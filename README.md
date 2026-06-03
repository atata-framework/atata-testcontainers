# Atata.Testcontainers

[![Atata Templates](https://img.shields.io/badge/get-Atata_Templates-green.svg?color=4BC21F)](https://marketplace.visualstudio.com/items?itemName=YevgeniyShunevych.AtataTemplates)\
[![Slack](https://img.shields.io/badge/join-Slack-green.svg?colorB=4EB898)](https://join.slack.com/t/atata-framework/shared_invite/zt-5j3lyln7-WD1ZtMDzXBhPm0yXLDBzbA)
[![Atata docs](https://img.shields.io/badge/docs-Atata_Framework-orange.svg)](https://atata.io)
[![X](https://img.shields.io/badge/follow-@AtataFramework-blue.svg)](https://x.com/AtataFramework)

**Atata.Testcontainers** is a C#/.NET library that adds Docker container sessions to [Atata](https://github.com/atata-framework/atata) using Testcontainers library.

*The package targets .NET 8.0 and .NET Framework 4.6.2.*

## Features

- **Sessions**. Adds both generic and non-generic container session types.
- **Artifacts**. Saves container logs as Atata artifacts.

## Installation

Install the package via .NET CLI:

```bash
dotnet add package Atata.Testcontainers
```

Or using Package Manager:

```powershell
Install-Package Atata.Testcontainers
```

## Dependencies

- [Atata](https://www.nuget.org/packages/Atata)
- [Testcontainers](https://www.nuget.org/packages/Testcontainers)

## Usage

Add container sessions to `AtataContextBuilder` or `AtataContext` using the provided `AddContainer` extension methods.

### Add non-generic container session to `AtataContextBuilder`

```cs
builder.Sessions.AddContainer(x => x
    .UseImage("hello-world:latest"));
```

### Add non-generic container session to `AtataContext`

```cs
var containerSession = await Context.Sessions.AddContainer()
    .UseImage("hello-world:latest")
    .BuildAsync();
```

### Add generic container session to `AtataContextBuilder`

```cs
builder.Sessions.AddContainer<RedisContainer>(x => x
    .Use(() => new RedisBuilder("redis:8.8.0")));
```

```cs
var containerSession = Context.Sessions.GetRecursively<ContainerSession<RedisContainer>>();

string connectionString = containerSession.Container.GetConnectionString();
```

*`RedisContainer` comes from Testcontainers.Redis package.*

### Add non-generic container session to `AtataContext`

```cs
var containerSession = await Context.Sessions.AddContainer<RedisContainer>(x => x
    .Use(() => new RedisBuilder("redis:8.8.0")))
    .BuildAsync();

string connectionString = containerSession.Container.GetConnectionString();
```

### Configure container logs saving

```cs
var containerSession = await Context.Sessions.AddContainer()
    .UseImage("hello-world:latest")
    .UseLogsSaveConfiguration(x => x.StdoutFileIncluded = false)
    .BuildAsync();
```

## API

### `ContainerSessionAtataSessionsBuilderExtensions`

A set of extension methods for `AtataSessionsBuilder` to add and configure `ContainerSessionBuilder` and `ContainerSessionBuilder<TContainer>` session builders.

```cs
public static class ContainerSessionAtataSessionsBuilderExtensions
{
    // Adds a new instance of ContainerSessionBuilder builder.
    public static AtataContextBuilder AddContainer(
        this AtataSessionsBuilder builder,
        Action<ContainerSessionBuilder>? configure = null);

    // Adds a new instance of ContainerSessionBuilder<TContainer> builder.
    public static AtataContextBuilder AddContainer<TContainer>(
        this AtataSessionsBuilder builder,
        Action<ContainerSessionBuilder<TContainer>>? configure = null)
        where TContainer : IContainer;

    // Configures existing nameless ContainerSessionBuilder session builder.
    public static AtataContextBuilder ConfigureContainer(
        this AtataSessionsBuilder builder,
        Action<ContainerSessionBuilder> configure,
        ConfigurationMode mode = default);

    // Configures existing nameless ContainerSessionBuilder<TContainer> session builder.
    public static AtataContextBuilder ConfigureContainer<TContainer>(
        this AtataSessionsBuilder builder,
        Action<ContainerSessionBuilder<TContainer>> configure,
        ConfigurationMode mode = default)
        where TContainer : IContainer;

    // Configures existing ContainerSessionBuilder session builder that has the specified name.
    public static AtataContextBuilder ConfigureContainer(
        this AtataSessionsBuilder builder,
        string? name,
        Action<ContainerSessionBuilder> configure,
        ConfigurationMode mode = default);

    // Configures existing ContainerSessionBuilder<TContainer> session builder that has the specified name.
    public static AtataContextBuilder ConfigureContainer<TContainer>(
        this AtataSessionsBuilder builder,
        string? name,
        Action<ContainerSessionBuilder<TContainer>> configure,
        ConfigurationMode mode = default)
        where TContainer : IContainer;
}
```

### `ContainerSessionAtataSessionCollectionExtensions`

```cs
public static class ContainerSessionAtataSessionCollectionExtensions
{
    // Creates a new ContainerSessionBuilder and adds it to the collection.
    public static ContainerSessionBuilder AddContainer(
        this AtataSessionCollection collection,
        Action<ContainerSessionBuilder>? configure = null);

    // Creates a new ContainerSessionBuilder<TContainer> and adds it to the collection.
    public static ContainerSessionBuilder<TContainer> AddContainer<TContainer>(
        this AtataSessionCollection collection,
        Action<ContainerSessionBuilder<TContainer>>? configure = null)
        where TContainer : IContainer;
}
```

### `ContainerSessionBuilder<TContainer, TSession, TBuilder>`

Represents a builder for creating and configuring a container session.

```cs
public abstract class ContainerSessionBuilder<TContainer, TSession, TBuilder> :
    AtataSessionBuilder<TSession, TBuilder>
    where TContainer : IContainer
    where TSession : ContainerSession<TContainer>, new()
    where TBuilder : ContainerSessionBuilder<TContainer, TSession, TBuilder>
{
    // Gets the configuration for saving container logs.
    public ContainerLogsSaveConfiguration LogsSaveConfiguration { get; }

    // Configures the builder to use a specific container builder.
    public TBuilder Use<TContainerBuilder>(Func<TContainerBuilder> containerBuilderCreator)
        where TContainerBuilder : IAbstractBuilder<TContainerBuilder, TContainer, CreateContainerParameters>;

    // Adds a specific container builder configuration.
    public TBuilder Configure<TContainerBuilder>(Func<TContainerBuilder, TContainerBuilder> configure)
        where TContainerBuilder : IAbstractBuilder<TContainerBuilder, TContainer, CreateContainerParameters>;

    // Configures the builder to use a specific logger for the container.
    public TBuilder UseContainerLogger(Func<ILogger> containerLoggerCreator);

    // Configures the builder to use a specific configuration for saving container logs.
    public TBuilder UseLogsSaveConfiguration(Action<ContainerLogsSaveConfiguration> configure);

    // Configures the builder to use a specific instance of <see cref="ContainerLogsSaveConfiguration"/>.
    public TBuilder UseLogsSaveConfiguration(ContainerLogsSaveConfiguration configuration);
}
```

### `ContainerSessionBuilder<TContainer>`

Represents a builder for creating and configuring a container session for a specific container type.

```cs
public class ContainerSessionBuilder<TContainer> :
    ContainerSessionBuilder<TContainer, ContainerSession<TContainer>, ContainerSessionBuilder<TContainer>>
    where TContainer : IContainer
{
}
```

### `ContainerSessionBuilder`

Represents a builder for creating and configuring a container session.

```cs
public class ContainerSessionBuilder : ContainerSessionBuilder<IContainer, ContainerSession, ContainerSessionBuilder>
{
    // Configures the builder to use a ContainerBuilder with the specified image name.
    public ContainerSessionBuilder UseImage(string imageName);
}
```

### `ContainerLogsSaveConfiguration`

A configuration for saving container logs.

```cs
public sealed class ContainerLogsSaveConfiguration
{
    // Gets the default configuration instance.
    public static ContainerLogsSaveConfiguration Default { get; }

    // Gets or sets the template for the stdout log file name.
    // The default value is "{container-image-fullname}-stdout.log".
    public string StdoutFileNameTemplate { get; set; }

    // Gets or sets a value indicating whether to include the stdout log file.
    // The default value is true.
    public bool StdoutFileIncluded { get; set; }

    // Gets or sets the template for the stderr log file name.
    // The default value is "{container-image-fullname}-stderr.log".
    public string StderrFileNameTemplate { get; set; }

    // Gets or sets a value indicating whether to include the stderr log file.
    // The default value is true.
    public bool StderrFileIncluded { get; set; }

    // Gets or sets a value indicating whether to include timestamps in the logs.
    // The default value is true.
    public bool TimestampsIncluded { get; set; }

    // Creates a new instance of ContainerLogsSaveConfiguration that is a copy of the current instance.
    public ContainerLogsSaveConfiguration Clone();
}
```

### `ContainerSession<TContainer>`

Represents a session that manages `TContainer` instance 
and provides a set of functionality to manipulate the container.

The session has additional variables in `AtataSession.Variables`:
`{container-image-fullname}`, `{container-image-repository}`, `{container-image-registry}`,
`{container-image-tag}`, `{container-image-digest}`.

```cs
public class ContainerSession<TContainer> : AtataSession
    where TContainer : IContainer
{
    // Gets the current `ContainerSession<TContainer> instance in scope of AtataContext.Current.
    // Returns null if there is no such session or AtataContext.Current is null.
    public static ContainerSession<TContainer>? Current { get; }

    // Gets the container.
    public TContainer Container { get; }

    // Creates ContainerSessionBuilder<TContainer> instance for ContainerSession<TContainer> configuration.
    public static ContainerSessionBuilder<TContainer> CreateBuilder();

    // Extracts the file from container to Artifacts directory.
    public async Task<FileSubject> ExtractFileToArtifactsAsync(
        string containerFilePath,
        string? artifactType = null,
        string? artifactTitle = null,
        CancellationToken cancellationToken = default);
```

### `ContainerSession`

```cs
public class ContainerSession : ContainerSession<IContainer>
{
    // Creates ContainerSessionBuilder instance for ContainerSession configuration.
    public static new ContainerSessionBuilder CreateBuilder();
}
```

## Community

- Slack: [https://atata-framework.slack.com](https://join.slack.com/t/atata-framework/shared_invite/zt-5j3lyln7-WD1ZtMDzXBhPm0yXLDBzbA)
- X: https://x.com/AtataFramework
- Stack Overflow: https://stackoverflow.com/questions/tagged/atata

## Feedback

Any feedback, issues and feature requests are welcome.

If you faced an issue please report it to [Atata.Testcontainers Issues](https://github.com/atata-framework/atata-testcontainers/issues),
[ask a question on Stack Overflow](https://stackoverflow.com/questions/ask?tags=atata+csharp) using [atata](https://stackoverflow.com/questions/tagged/atata) tag
or use another [Atata Contact](https://atata.io/contact/) way.

## Contact author

Contact me if you need a help in test automation using Atata Framework, or if you are looking for a quality test automation implementation for your project.

- LinkedIn: https://www.linkedin.com/in/yevgeniy-shunevych
- Email: yevgeniy.shunevych@gmail.com
- Consulting: https://atata.io/consulting/

## Contributing

Check out [Contributing Guidelines](CONTRIBUTING.md) for details.

## SemVer

Atata Framework tries to follow [Semantic Versioning 2.0](https://semver.org/) when possible.
Sometimes Selenium.WebDriver dependency package can contain breaking changes in minor version releases,
so those changes can break Atata as well.
But Atata manages its sources according to SemVer.
Thus backward compatibility is mostly followed and updates within the same major version
(e.g. from 2.1 to 2.2) should not require code changes.

## License

Atata is an open source software, licensed under the Apache License 2.0.
See [LICENSE](LICENSE) for details.
