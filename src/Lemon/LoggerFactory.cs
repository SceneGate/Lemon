namespace SceneGate.Lemon;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Logger factory for the framework.
/// </summary>
public static class LoggerFactory
{
    private static ILoggerFactory factory;

    /// <summary>
    /// Gets or sets the logger factory to use for the framework.
    /// </summary>
    public static ILoggerFactory Instance {
        get => factory ??= NullLoggerFactory.Instance;
        set => factory = value;
    }
}
