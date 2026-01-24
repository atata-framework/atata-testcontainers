namespace Atata.Testcontainers;

/// <summary>
/// Represents a builder for creating and configuring a container session.
/// </summary>
public class ContainerSessionBuilder : ContainerSessionBuilder<IContainer, ContainerSession, ContainerSessionBuilder>
{
    /// <summary>
    /// Configures the builder to use a <see cref="ContainerBuilder"/> with the specified image name.
    /// </summary>
    /// <param name="imageName">The image name.</param>
    /// <returns>The same <see cref="ContainerSessionBuilder"/> instance.</returns>
    public ContainerSessionBuilder UseImage(string imageName)
    {
        Guard.ThrowIfNullOrWhitespace(imageName);

        return Use(() => new ContainerBuilder(imageName));
    }

    private protected override string BuildContainerCreatorIsNotSetErrorMessage() =>
        $"Container creator is not set. Use either '{nameof(UseImage)}' or '{nameof(Use)}' method to set it.";
}
