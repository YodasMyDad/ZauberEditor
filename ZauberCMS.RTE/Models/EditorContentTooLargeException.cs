using System.Globalization;

namespace ZauberCMS.RTE.Models;

/// <summary>Thrown when editor content is larger than <see cref="EditorSettings.MaxContentBytes"/> and cannot be read.</summary>
public sealed class EditorContentTooLargeException : Exception
{
    /// <summary>Creates the exception with a default message.</summary>
    public EditorContentTooLargeException()
        : this("The editor content is larger than the configured limit.")
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">The error message.</param>
    public EditorContentTooLargeException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The cause.</param>
    public EditorContentTooLargeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates the exception for content of the given size.</summary>
    /// <param name="length">The content size in UTF-8 bytes.</param>
    /// <param name="maxBytes">The configured limit in bytes.</param>
    public EditorContentTooLargeException(long length, long maxBytes)
        : this(string.Create(CultureInfo.InvariantCulture, $"The editor content is {length} bytes, more than the {maxBytes} byte limit."))
    {
        Length = length;
        MaxBytes = maxBytes;
    }

    /// <summary>The content size in UTF-8 bytes, when known.</summary>
    public long Length { get; }

    /// <summary>The configured limit in bytes, when known.</summary>
    public long MaxBytes { get; }
}
