using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ObeTools
{
    public static class EncryptHasher
    {
        #region Variable
        private const string Key = "ABC DDD jJ9eutBMue44KhPGZp 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";
        private static readonly System.Random Random = new Random();
        private static readonly Dictionary<char, char> Base64Symbol = new Dictionary<char, char>()
        {
            { '+', '-' },
            { '=', '_' },
            { '/', '.' }
        };
        private static readonly Dictionary<char, char> SafeSymbol = new Dictionary<char, char>()
        {
            { '-', '+' },
            { '_', '=' },
            { '.', '/' }
        };
        #endregion

        #region Method

        public static T FromURLToken<T>(this string input)
        {
            return JsonSerializer.Deserialize<T>(Decrypt(input));
        }
        public static string ToURLToken<T>(this T input)
        {
            return Encrypt(JsonSerializer.Serialize(input));
        }


        public static string ToSafeBase64(char[] input)
        {
            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            return base64String.ToSafeBase64String();
        }
        public static string ToSafeBase64(byte[] input)
        {
            var base64String = Convert.ToBase64String(input);
            return base64String.ToSafeBase64String();
        }
        public static string FromSafeBase64(string input)
        {
            char[] result = new char[input.Length];
            for (int k = 0; k < result.Length; k++)
            {
                var tempChar = input[k];
                if (SafeSymbol.ContainsKey(tempChar))
                {
                    tempChar = SafeSymbol[tempChar];
                }

                result[k] = tempChar;
            }
            return Encoding.UTF8.GetString(Convert.FromBase64String(new string(result)));
        }
        public static string ToSafeBase64String(this string base64String)
        {
            char[] result = new char[base64String.Length];
            for (int k = 0; k < result.Length; k++)
            {
                var tempChar = base64String[k];
                if (Base64Symbol.ContainsKey(tempChar))
                {
                    tempChar = Base64Symbol[tempChar];
                }

                result[k] = tempChar;
            }
            return new string(result);
        }
        public static string Encrypt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            string str2 = Guid.NewGuid().ToString().Replace("-", "");
            char[] chArray = new char[(str.Length * 2) + 4];
            string str3 = string.Empty;
            int num = Random.Next(0, 2502 - str.Length);
            if (num < 10)
            {
                str3 = "000" + num;
            }
            else if (num < 100)
            {
                str3 = "00" + num;
            }
            else if (num < 1000)
            {
                str3 = "0" + num;
            }
            else
            {
                str3 = num.ToString();
            }
            chArray[0] = (char)(str3[0] ^ 'a');
            chArray[1] = (char)(str3[1] ^ 's');
            chArray[2] = (char)(str3[2] ^ 'e');
            chArray[3] = (char)(str3[3] ^ 'l');
            for (int i = 0; i < str.Length; i++)
            {
                chArray[4 + (i * 2)] = (char)(str[i] ^ Key[num + i]);
                chArray[5 + (i * 2)] = str2[i % 30];
            }
            return ToSafeBase64(chArray);
        }
        public static string Decrypt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            str = FromSafeBase64(str);
            int num = (str.Length - 4) / 2;
            char[] chArray = new char[] { (char)(str[0] ^ 'a'), (char)(str[1] ^ 's'), (char)(str[2] ^ 'e'), (char)(str[3] ^ 'l') };
            char[] chArray2 = new char[num];
            int num2 = Convert.ToInt32(new string(chArray));
            for (int i = 0; i < num; i++)
            {
                chArray2[i] = (char)(str[(i * 2) + 4] ^ Key[num2 + i]);
            }
            return new string(chArray2);
        }

        /// <summary>
        /// Get Hash of string (Hmac sha 512)
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <param name="secret">key string for secret hash</param>
        /// <returns>[input] hash</returns>
        public static string HashHmacSha512(string input, string secret)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] key = Encoding.UTF8.GetBytes(secret);
            using HMACSHA512 hmac = new HMACSHA512(key);
            byte[] computedHash = hmac.ComputeHash(bytes);
            return BitConverter.ToString(computedHash).Replace("-", "");
        }

        /// <summary>
        /// Get Hash of string (Hmac sha 256)
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <param name="secret">key string for secret hash</param>
        /// <returns>[input] hash</returns>
        public static string HashHmacSha256(string input, string secret)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] key = Encoding.UTF8.GetBytes(secret);
            using HMACSHA256 hmac = new HMACSHA256(key);
            byte[] computedHash = hmac.ComputeHash(bytes);
            return BitConverter.ToString(computedHash).Replace("-", "");
        }

        /// <summary>
        /// Get Hash of string (sha 512)
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <returns>[input] hash</returns>
        public static string HashSha512(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using SHA512Managed hmac = new SHA512Managed();
            byte[] computedHash = hmac.ComputeHash(bytes);
            return BitConverter.ToString(computedHash).Replace("-", "");
        }

        /// <summary>
        /// Get Hash of string (sha 256)
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <returns>[input] hash</returns>
        public static string HashSha256(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using var hmac = new SHA256Managed();
            byte[] computedHash = hmac.ComputeHash(bytes);
            return BitConverter.ToString(computedHash).Replace("-", "");
        }
        public static string RemoveJavaScript(string data)
        {
            return Regex.Replace(data, "<.*?>", string.Empty).Trim();
        }
        public static int GenerateRandomNumber(int digit)
        {
            return Random.Next(Convert.ToInt32(Math.Pow(10, digit)), Convert.ToInt32(Math.Pow(10, digit + 1) - 1));
        }
        public static string CustomEncrypt(string input, string salt)
        {
            int length = input.Length;
            int halflength = length / 2;
            return ReverseString($"{input[..halflength]}_{salt}{Key.Substring(length, length)}_{input.Substring(halflength, halflength)}");
        }
        #endregion

        #region Helper
        private static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        #endregion
    }
}
