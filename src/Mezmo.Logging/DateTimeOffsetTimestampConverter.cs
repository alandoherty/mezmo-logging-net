using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mezmo.Logging
{
    /// <summary>
    /// Implements a <see cref="JsonConverter{T}"/> for converting a <see cref="DateTimeOffset"/> into a UNIX millisecond-precision timestamp as a string.
    /// </summary>
    class DateTimeOffsetTimestampConverter : JsonConverter<DateTimeOffset>
    {
        /// <inheritdoc/>
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("The converter only supporting writing");
        }
        
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            // Write the timestamp to a stack char array
            Span<char> bytes = stackalloc char[16];
            value.ToUnixTimeMilliseconds().TryFormat(bytes, out int charsWritten);
            
            // Write the value directly to the JSON
            writer.WriteStringValue(bytes.Slice(0, charsWritten));
        }
    }
}