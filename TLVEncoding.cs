using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        /// <returns>Dictionary of tag number and value</returns>
        public static Dictionary<int, string> Decode(byte[] data)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            if (data.Any())
            {
                var hexString = HexStringFromHexBytes(data);
                result = FromTagValue(hexString);
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
                _ = data.Append(GetTagValue(tagValue));
            }
            return HexStringToHexBytes(data.ToString());
        }
        #endregion

        #region Helper
        private static byte[] HexStringToHexBytes(string inputHex)
        {
            var resultantArray = new byte[inputHex.Length / 2];
            for (var i = 0; i < resultantArray.Length; i++)
            {
                resultantArray[i] = System.Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
            }
            return resultantArray;
        }
        private static string HexStringFromHexBytes(byte[] inputHex)
        {
            return BitConverter.ToString(inputHex).Replace("-", "");
        }
        private static string GetTagValue(KeyValuePair<int, string> tagValueItem)
        {
            string tagNum = tagValueItem.Key.ToString();
            string tagValue = tagValueItem.Value;
            int length = tagValue.Length;
            if (Regex.IsMatch(tagValue, @"\p{IsArabic}"))
            {
                length = 0;
                for (int i = 0; i < tagValue.Length; i++)
                {
                    length++;
                    if (Regex.IsMatch(tagValue[i].ToString(), @"\p{IsArabic}"))
                    {
                        length++;
                    }
                }
            }

            var lengthBytes = length.ToString("X");
            var valueBytes = GetHexa(tagValue);
            if (lengthBytes.Length == 1)
            {
                lengthBytes = "0" + lengthBytes;
            }
            if (tagNum.Length == 1)
            {
                tagNum = "0" + tagNum;
            }
            return $"{tagNum}{lengthBytes}{valueBytes}";
        }
        private static Dictionary<int, string> FromTagValue(string input)
        {
            var result = new Dictionary<int, string>();
            var tagValueItem = input;
            while (!string.IsNullOrEmpty(tagValueItem))
            {
                var tagNum = Convert.ToInt32(tagValueItem[..2], 16);
                var lengthNum = Convert.ToInt32(tagValueItem[2..4], 16) * 2;
                var tagHexValue = tagValueItem.Substring(4, lengthNum);
                var tagValue = Encoding.UTF8.GetString(HexStringToHexBytes(tagHexValue));
                result.Add(tagNum, tagValue);
                tagValueItem = tagValueItem[(lengthNum + 4)..];
            }
            return result;
        }
        private static string GetHexa(string value)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.UTF8.GetBytes(value);
            foreach (var t in bytes)
            {
                _ = sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }
        #endregion
    }
}
