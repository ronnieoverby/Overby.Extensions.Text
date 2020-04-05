using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace Overby.Extensions.Text
{
    public class MyStreamPeeker : IDisposable
    {
        private readonly MyStreamReader _reader;
        private readonly IMemoryOwner<char> _bufferOwner;
		char? _temp;
		Memory<char> _buffer;
		int _i;

		public MyStreamPeeker(MyStreamReader reader, int bufferSize = 1024)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _bufferOwner = MemoryPool<char>.Shared.Rent(bufferSize);
        }

		public async ValueTask<char?> ReadAsync(CancellationToken ct = default)
		{
			if (_temp.HasValue)
			{
				// recent peek has stored the current character
				var c = _temp.Value;
				_temp = null;
				return c;
			}

			if (_buffer.IsEmpty || _i == _buffer.Length)
				await FillBufferAsync(ct);

			return _buffer.IsEmpty
				? default(char?)
				: _buffer.Span[_i++];
		}

		public async ValueTask<char?> PeekAsync(CancellationToken ct = default)
		{
			if (_temp.HasValue)
			{
				// recent peek has stored the current character
				return _temp.Value;
			}

			if (_buffer.IsEmpty || _i == _buffer.Length)
				await FillBufferAsync(ct);

			return _buffer.IsEmpty
				? default
				: _temp = _buffer.Span[_i++];
		}

		private async ValueTask FillBufferAsync(CancellationToken ct)
		{
			var read = await _reader.ReadAsync(_bufferOwner.Memory);
			_buffer = _bufferOwner.Memory.Slice(0, read);
			_i = 0;
		}


		public void Dispose()
        {
            using (_bufferOwner)
                _reader.Dispose();
        }
    }
}
