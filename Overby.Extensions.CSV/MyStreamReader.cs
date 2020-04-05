using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Overby.Extensions.Text
{
    public class MyStreamReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly Decoder _decoder;
        private readonly IMemoryOwner<byte> _bufferOwner;
        private Memory<byte> _buffer;

        public MyStreamReader(Stream stream, Encoding encoding = null, int bufferSize = 4096)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _bufferOwner = MemoryPool<byte>.Shared.Rent(bufferSize);

            encoding ??= Encoding.UTF8;
            _decoder = encoding.GetDecoder();
        }

        public async ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken ct = default)
        {
            if (_buffer.IsEmpty)
                await FillBufferAsync(ct);

            _decoder.Convert(_buffer.Span, buffer.Span, flush: _buffer.IsEmpty,
                out var bytesUsed, out var charsUsed, out var completed);

            _buffer = _buffer[bytesUsed..];

            return charsUsed;
        }

        private async ValueTask<bool> FillBufferAsync(CancellationToken ct)
        {
            var buffer = _bufferOwner.Memory;
            var read = await _stream.ReadAsync(buffer, ct);

            if (read > 0)
            {
                _buffer = buffer.Slice(0, read);
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            _bufferOwner.Dispose();
        }
    }
}
