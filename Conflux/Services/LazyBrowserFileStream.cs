using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Conflux.Services;

internal sealed class LazyBrowserFileStream(IBrowserFile file, int maxAllowedSize) 
    : Stream
{
    private Stream? _underlyingStream;
    private bool _disposed;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => file.Size;

    public override long Position
    {
        get => _underlyingStream?.Position ?? 0;
        set => throw new NotSupportedException();
    }

    public override void Flush() => _underlyingStream?.Flush();

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        EnsureStreamIsOpen();

        return _underlyingStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        EnsureStreamIsOpen();
        return _underlyingStream.ReadAsync(buffer, cancellationToken);
    }

    [MemberNotNull(nameof(_underlyingStream))]
    private void EnsureStreamIsOpen() => _underlyingStream ??= file.OpenReadStream(maxAllowedSize);

    protected override void Dispose(bool disposing) {
        if (_disposed) {
            return;
        }

        _underlyingStream?.Dispose();
        _disposed = true;

        base.Dispose(disposing);
    }

    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    public override void SetLength(long value)
        => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
}