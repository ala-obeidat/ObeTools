using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ObeTools
{
    /// <summary>
    /// Tag length value encoding helper (support UTF8), as used by ZATCA QR codes.
    /// Tags and lengths are one byte each, so a tag must be 0-255 and a value at most 255 bytes.
    /// </summary>
    public static class TLVEncoding
    {
        /// <summary>
        /// Largest tag number and largest value length (in bytes).
        /// </summary>
        public const int MaxTagOrLength = 255;

        #region Methods

        /// <summary>
        /// Decode TLV bytes to dictionary of tag,data
        /// </summary>
        /// <param name="data">TLV encoded bytes</param>
        /// <param name="binaryTags">
        /// Optional list of tag numbers that should be treated as binary.
        /// Binary values are returned as Base64 strings.
        /// </param>
        /// <returns>Dictionary of tag number and value. Decoding stops at the first truncated entry.</returns>
        public static Dictionary<int, string> Decode(byte[] data, List<int> binaryTags = null)
        {
            var result = new Dictionary<int, string>();
            if (data == null || data.Length == 0)
            {
                return result;
            }

            var binarySet = binaryTags != null && binaryTags.Count > 0
                ? new HashSet<int>(binaryTags)
                : null;

            int position = 0;
            while (data.Length - position >= 2)
            {
                int tag = data[position];
                int length = data[position + 1];
                if (data.Length - position - 2 < length)
                {
                    break; // malformed TLV: not enough data for this value
                }

                result[tag] = binarySet != null && binarySet.Contains(tag)
                    ? Convert.ToBase64String(data, position + 2, length)
                    : Encoding.UTF8.GetString(data, position + 2, length);

                position += 2 + length;
            }

            return result;
        }

        /// <summary>
        /// Encode dictionary of tag,data to TLV bytes (values as UTF-8 text).
        /// </summary>
        /// <param name="tagValues">Dictionary of tag number and value</param>
        /// <returns>TLV encoded bytes</returns>
        /// <exception cref="ArgumentOutOfRangeException">A tag is outside 0-255 or a value is longer than 255 UTF-8 bytes
        /// (for example an Arabic name longer than about 127 characters).</exception>
        public static byte[] Encode(Dictionary<int, string> tagValues)
        {
            if (tagValues == null || tagValues.Count == 0)
            {
                return Array.Empty<byte>();
            }

            using var stream = new MemoryStream();
            foreach (var tagValue in tagValues)
            {
                Write(stream, tagValue.Key, Encoding.UTF8.GetBytes(tagValue.Value ?? string.Empty));
            }
            return stream.ToArray();
        }

        /// <summary>
        /// Encode tags with binary values (for example ZATCA phase-2 tags 6-9: hash, signature, public key,
        /// certificate signature), in the given order.
        /// </summary>
        /// <param name="tagValues">tag number and raw value bytes</param>
        /// <returns>TLV encoded bytes</returns>
        /// <exception cref="ArgumentOutOfRangeException">A tag is outside 0-255 or a value is longer than 255 bytes.</exception>
        public static byte[] Encode(IEnumerable<(int Tag, byte[] Value)> tagValues)
        {
            if (tagValues == null)
            {
                return Array.Empty<byte>();
            }

            using var stream = new MemoryStream();
            foreach (var (tag, value) in tagValues)
            {
                Write(stream, tag, value ?? Array.Empty<byte>());
            }
            return stream.ToArray();
        }

        #endregion

        #region Helper

        private static void Write(Stream stream, int tag, byte[] value)
        {
            if (tag < 0 || tag > MaxTagOrLength)
            {
                throw new ArgumentOutOfRangeException(nameof(tag), tag, $"A TLV tag must be between 0 and {MaxTagOrLength}.");
            }
            if (value.Length > MaxTagOrLength)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value.Length,
                    $"The value of tag {tag} is {value.Length} bytes; a TLV value can be at most {MaxTagOrLength} bytes.");
            }

            stream.WriteByte((byte)tag);
            stream.WriteByte((byte)value.Length);
            stream.Write(value, 0, value.Length);
        }

        #endregion
    }
}
