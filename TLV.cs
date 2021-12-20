using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ObeTools
{
    public static class TLV
    {
        #region Methods
        public static Dictionary<int, string> Decode(byte[] data)
        {
            return null;
        }
        public static byte[] Encode(Dictionary<int, string> tagValues)
        {
            if (tagValues == null || tagValues.Count == 0)
            {
                return null;
            }
            var data = new StringBuilder();
            foreach (var tagValue in tagValues)
            {
                data.Append(GetTagValue(tagValue));
            }
            return HexStringToHex(data.ToString());
        }
        #endregion

        #region Helper
        private static byte[] HexStringToHex(string inputHex)
        {
            var resultantArray = new byte[inputHex.Length / 2];
            for (var i = 0; i < resultantArray.Length; i++)
            {
                resultantArray[i] = System.Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
            }
            return resultantArray;
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
        private static string GetHexa(string value)
        {
            var sb = new StringBuilder();
            var bytes = Encoding.UTF8.GetBytes(value);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }
        #endregion
    }
}
