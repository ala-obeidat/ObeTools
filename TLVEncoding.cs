using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObeTools
{
    /// <summary>
    /// Tag length value encoding helper (support UTF8)
    /// </summary> 
    public static class TLVEncoding
    {
        #region Methods

        /// <summary>
        /// Decode TLV bytes to dictionary of tag,data
        /// </summary>
        /// <param name="data">TLV encoded bytes</param>
        /// <param name="binaryTags">
        /// Optional list of tag numbers that should be treated as binary.
        /// Binary values are returned as Base64 strings.
        /// </param>
        /// <returns>Dictionary of tag number and value</returns>
        public static Dictionary<int, string> Decode(byte[] data, List<int> binaryTags = null)
        {
            var result = new Dictionary<int, string>();

            if (data != null && data.Any())
            {
                var hexString = HexStringFromHexBytes(data);
                result = FromTagValue(hexString, binaryTags);
            }

            return result;
        }

        /// <summary>
        /// Encode dictionary of tag,data to TLV bytes
        /// </summary>
        /// <param name="tagValues">Dictionary of tag number and value</param>
        /// <returns>TLV encoded bytes</returns>
        public static byte[] Encode(Dictionary<int, string> tagValues)
        {
            if (tagValues == null || tagValues.Count == 0)
            {
                return Array.Empty<byte>();
            }

            var data = new StringBuilder();

            foreach (var tagValue in tagValues)
            {
                data.Append(GetTagValue(tagValue));
            }

            return HexStringToHexBytes(data.ToString());
        }

        #endregion

        #region Helper

        private static byte[] HexStringToHexBytes(string inputHex)
        {
            if (string.IsNullOrEmpty(inputHex))
            {
                return Array.Empty<byte>();
            }

            var resultantArray = new byte[inputHex.Length / 2];

            for (var i = 0; i < resultantArray.Length; i++)
            {
                resultantArray[i] = Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
            }

            return resultantArray;
        }

        private static string HexStringFromHexBytes(byte[] inputHex)
        {
            if (inputHex == null || inputHex.Length == 0)
            {
                return string.Empty;
            }

            return BitConverter.ToString(inputHex).Replace("-", "");
        }

        /// <summary>
        /// Build TLV hex string for one tag/value pair (UTF-8, length in bytes).
        /// </summary>
        private static string GetTagValue(KeyValuePair<int, string> tagValueItem)
        {
            // Tag number encoded as hex, 2 digits (supports tags 0–255)
            var tagNumHex = tagValueItem.Key.ToString("X2");

            // Value bytes in UTF-8
            var valueBytes = Encoding.UTF8.GetBytes(tagValueItem.Value ?? string.Empty);

            // Length is number of bytes, encoded as hex, 2 digits
            var lengthHex = valueBytes.Length.ToString("X2");

            // Convert value bytes to hex
            var valueHexBuilder = new StringBuilder(valueBytes.Length * 2);
            foreach (var b in valueBytes)
            {
                valueHexBuilder.Append(b.ToString("X2"));
            }

            return $"{tagNumHex}{lengthHex}{valueHexBuilder}";
        }

        /// <summary>
        /// Parse TLV hex string into dictionary of tag/value.
        /// Tags listed in <paramref name="binaryTags"/> are treated as binary and returned as Base64.
        /// Other tags are treated as UTF-8 text.
        /// </summary>
        private static Dictionary<int, string> FromTagValue(string input, List<int> binaryTags = null)
        {
            var result = new Dictionary<int, string>();

            if (string.IsNullOrEmpty(input))
            {
                return result;
            }

            var tagValueItem = input;
            var binarySet = binaryTags != null && binaryTags.Count > 0
                ? new HashSet<int>(binaryTags)
                : null;

            while (!string.IsNullOrEmpty(tagValueItem))
            {
                if (tagValueItem.Length < 4)
                {
                    // Not enough data for tag + length
                    break;
                }

                // First byte = tag (as hex)
                var tagNum = Convert.ToInt32(tagValueItem[..2], 16);

                // Second byte = length in bytes; *2 because we are working with hex chars
                var lengthNum = Convert.ToInt32(tagValueItem[2..4], 16) * 2;

                if (tagValueItem.Length < 4 + lengthNum)
                {
                    // Not enough data for this value; malformed TLV
                    break;
                }

                var tagHexValue = tagValueItem.Substring(4, lengthNum);
                var bytes = HexStringToHexBytes(tagHexValue);

                string tagValue;

                // If tag is in binaryTags, return as Base64; otherwise, decode as UTF-8
                if (binarySet != null && binarySet.Contains(tagNum))
                {
                    tagValue = Convert.ToBase64String(bytes);
                }
                else
                {
                    tagValue = Encoding.UTF8.GetString(bytes);
                }

                result[tagNum] = tagValue;

                // Move to the next tag: skip current tag (2), length (2), and value (lengthNum chars)
                tagValueItem = tagValueItem[(lengthNum + 4)..];
            }

            return result;
        }

        private static string GetHexa(string value)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);

            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        #endregion
    }
}
