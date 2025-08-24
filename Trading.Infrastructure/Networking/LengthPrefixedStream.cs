using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trading.Contracts;

namespace Trading.Infrastructure.Networking
{
    /// <summary>
    /// Provides helpers to read and write JSON messages framed with a 4‑byte
    /// little‑endian unsigned integer prefix specifying the payload length.
    /// The payload is encoded in UTF‑8 and deserialized to dynamic objects
    /// using System.Text.Json.  Consumers should cast to appropriate types
    /// based on the "schema" field.
    /// </summary>
    public class LengthPrefixedStream : IAsyncDisposable
    {
        private readonly Stream _stream;
        private readonly JsonSerializerOptions _serializerOptions;

        public LengthPrefixedStream(Stream stream)
        {
            _stream = stream;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Writes a message object as UTF‑8 JSON with a little‑endian length prefix.
        /// </summary>
        public async Task WriteMessageAsync<T>(T message, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(message, _serializerOptions);
            Span<byte> header = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(header, (uint)payload.Length);
            await _stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
            await _stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the next message from the stream.  Returns null if the stream
        /// ends before a complete frame can be read.
        /// </summary>
        public async Task<JsonDocument?> ReadMessageAsync(CancellationToken cancellationToken)
        {
            var header = new byte[4];
            int read = 0;
            while (read < 4)
            {
                int n = await _stream.ReadAsync(header.AsMemory(read, 4 - read), cancellationToken).ConfigureAwait(false);
                if (n == 0)
                {
                    // remote closed
                    return null;
                }
                read += n;
            }
            uint length = BinaryPrimitives.ReadUInt32LittleEndian(header);
            var buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int n = await _stream.ReadAsync(buffer.AsMemory(offset, (int)length - offset), cancellationToken).ConfigureAwait(false);
                if (n == 0)
                {
                    return null;
                }
                offset += n;
            }
            var doc = JsonDocument.Parse(buffer);
            return doc;
        }

        public ValueTask DisposeAsync()
        {
            _stream.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}