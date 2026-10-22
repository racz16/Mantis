namespace Mantis.Platform;

public sealed class PlatformException(string message, Exception? innerException = null) : Exception(message, innerException) { }
