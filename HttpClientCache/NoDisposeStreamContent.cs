using System.Net;
using System.Text;

namespace HttpClientCache
{
    class NoDisposeStreamContent : HttpContent
    {
        private byte[] _buffer;

        class NoDisposeMemoryStream : MemoryStream
        {
            public NoDisposeMemoryStream(byte[] buffer) : base(buffer)
            {
            }

            private void Reset()
            {
                Position = 0;
            }

            protected override void Dispose(bool disposing)
            {
                Reset();
            }

            public override ValueTask DisposeAsync()
            {
                Reset();
                return ValueTask.CompletedTask;
            }
        }

        public NoDisposeStreamContent(HttpContent content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content, nameof(content));

            _buffer = content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (var headers in content.Headers)
            {
                Headers.TryAddWithoutValidation(headers.Key, headers.Value);
            }
        }

        protected string Text => Encoding.UTF8.GetString(_buffer);

        protected override void Dispose(bool disposing)
        {
        }

        protected async Task CopyToStreamAsync(Stream stream)
        {
            await stream.WriteAsync(_buffer, 0, _buffer.Length);
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            await CopyToStreamAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _buffer.Length;
            return true;
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(new NoDisposeMemoryStream(_buffer));
        }
    }    
}
