using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ObeTools
{
    /// <summary>
    /// Hashing, password hashing, random codes and (legacy) URL obfuscation.
    /// </summary>
    /// <remarks>
    /// <see cref="Encrypt"/>/<see cref="Decrypt"/> are obfuscation with a key that is public (it ships in this
    /// open-source package), not encryption, and they have no integrity check. Use <see cref="SecretProtector"/>
    /// for anything that must stay secret or must not be forged.
    /// </remarks>
    public static class EncryptHasher
    {
        #region Variable
        internal const string ObsoleteObfuscation = "Obfuscation with a public key - not encryption, and no integrity check. Use SecretProtector.";

        private const string Key = "ABC DDD jJ9eutBMue44KhPGZp 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";
        private static readonly Random Random = new Random();
        private static readonly object RandomLock = new object();
        private const string defaultSalt = "asel";

        /// <summary>
        /// Longest input <see cref="Encrypt"/> accepts.
        /// </summary>
        public const int MaxEncryptLength = 2502;

        private static readonly Regex TagPattern = new Regex(
            "<[^>]*(>|$)",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

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

        private const string PasswordHashPrefix = "pbkdf2-sha256";
        private const int PasswordSaltSize = 16;
        private const int PasswordHashSize = 32;
        private const int MaxPasswordIterations = 10_000_000;

        /// <summary>
        /// PBKDF2-SHA256 iterations used by <see cref="HashPassword"/> (OWASP 2023 recommendation).
        /// </summary>
        public const int PasswordIterations = 600_000;
        #endregion

        #region Method

        /// <summary>
        /// Get object from encrypted serialized string.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="input">serialized string</param>
        /// <returns>Object of type [T]</returns>
        [Obsolete(ObsoleteObfuscation)]
        public static T FromURLToken<T>(this string input)
        {
            return JsonSerializer.Deserialize<T>(Decrypt(input));
        }

        /// <summary>
        /// Convert object to encrypted serialized string.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="input">Object data to be serialized</param>
        /// <returns>encrypted serialized string</returns>
        [Obsolete(ObsoleteObfuscation)]
        public static string ToURLToken<T>(this T input)
        {
            return Encrypt(JsonSerializer.Serialize(input));
        }

        /// <summary>
        /// Convert Array of characturs to safe(url encoding) base64
        /// </summary>
        /// <param name="input">Array to be converted</param>
        /// <returns>Safe base64 string</returns>
        /// <remarks>
        /// The alphabet maps '+' to '-', '=' to '_' and '/' to '.'. It is NOT RFC 4648 base64url; it is kept for compatibility.
        /// </remarks>
        public static string ToSafeBase64(char[] input)
        {
            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            return base64String.ToSafeBase64String();
        }

        /// <summary>
        /// Convert Array of bytes to safe(url encoding) base64
        /// </summary>
        /// <param name="input">Array to be converted</param>
        /// <returns>Safe(url encoding) base64 string</returns>
        public static string ToSafeBase64(byte[] input)
        {
            var base64String = Convert.ToBase64String(input);
            return base64String.ToSafeBase64String();
        }

        /// <summary>
        /// Get flat string from safe(url encoding) base64 string. Missing padding is added.
        /// </summary>
        /// <param name="input">safe base64 string</param>
        /// <param name="tries">Ignored (kept for compatibility); the padding is now computed exactly.</param>
        /// <returns>flat string</returns>
        /// <exception cref="FormatException">The input is not valid safe base64.</exception>
        public static string FromSafeBase64(string input, int tries = 3)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            return Encoding.UTF8.GetString(FromSafeBase64Bytes(input));
        }

        /// <summary>
        /// Get flat string from safe(url encoding) base64 string without throwing.
        /// </summary>
        /// <param name="input">safe base64 string</param>
        /// <param name="output">flat string, or null when the input is not valid</param>
        /// <returns>true when the input was valid</returns>
        public static bool TryFromSafeBase64(string input, out string output)
        {
            output = null;
            if (input == null)
            {
                return false;
            }
            try
            {
                output = Encoding.UTF8.GetString(FromSafeBase64Bytes(input));
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] FromSafeBase64Bytes(string input)
        {
            var sb = new StringBuilder(input.Length + 3);
            foreach (var c in input)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                sb.Append(SafeSymbol.TryGetValue(c, out var mapped) ? mapped : c);
            }

            // Re-pad exactly: strip whatever padding is there, then add (4 - len % 4) % 4.
            int end = sb.Length;
            while (end > 0 && sb[end - 1] == '=')
            {
                end--;
            }
            sb.Length = end;
            if (end % 4 == 1)
            {
                throw new FormatException("The input is not a valid safe base64 string.");
            }
            sb.Append('=', (4 - (end % 4)) % 4);
            return Convert.FromBase64String(sb.ToString());
        }

        /// <summary>
        /// Convert base 64 string to safe(url encoding)
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Safe(url encoding) base64 string</returns>
        public static string ToSafeBase64String(this string input)
        {
            char[] result = new char[input.Length];
            for (int k = 0; k < result.Length; k++)
            {
                var tempChar = input[k];
                if (Base64Symbol.ContainsKey(tempChar))
                {
                    tempChar = Base64Symbol[tempChar];
                }

                result[k] = tempChar;
            }
            return new string(result);
        }

        /// <summary>
        /// Obfuscate a string by Obe method. This is NOT encryption: the key is public and there is no integrity check.
        /// </summary>
        /// <param name="input">flat string, at most <see cref="MaxEncryptLength"/> characters. Characters outside the
        /// Basic Multilingual Plane (emoji) do not round-trip.</param>
        /// <param name="salt">salt to be added to used in encryption, at least 4 characters (only the first 4 matter)
        /// [if not passed we will use default method salt]</param>
        /// <returns>encrypted string</returns>
        /// <exception cref="ArgumentException">The input is too long or the salt is shorter than 4 characters.</exception>
        [Obsolete(ObsoleteObfuscation)]
        public static string Encrypt(string input, string salt = "")
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(salt))
            {
                salt = defaultSalt;
            }
            if (input.Length > MaxEncryptLength)
            {
                throw new ArgumentException($"Encrypt accepts at most {MaxEncryptLength} characters.", nameof(input));
            }
            if (salt.Length < 4)
            {
                throw new ArgumentException("The salt must have at least 4 characters; a shorter salt cannot be decrypted.", nameof(salt));
            }
            string str2 = Guid.NewGuid().ToString().Replace("-", "");
            char[] chArray = GenerateEncrypt(input, str2, salt);
            return ToSafeBase64(chArray);
        }


        /// <summary>
        /// Reverse <see cref="Encrypt"/>. Any changed token decrypts silently to different text: never trust the result.
        /// </summary>
        /// <param name="input">encrypted string</param>
        /// <param name="salt">salt to be added to used in encryption [if not passed we will use default method salt]</param>
        /// <returns>flat string</returns>
        [Obsolete(ObsoleteObfuscation)]
        public static string Decrypt(string input, string salt = "")
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(salt))
            {
                salt = defaultSalt;
            }
            input = FromSafeBase64(input);
            char[] chArray2 = GenerateDecrypt(input, salt);
            return new string(chArray2);
        }



        /// <summary>
        /// Get Hash of string (Hmac sha 512), uppercase hex without dashes.
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <param name="secret">key string for secret hash</param>
        /// <returns>[input] hash; null or empty input is returned as is</returns>
        /// <exception cref="ArgumentNullException"><paramref name="secret"/> is null.</exception>
        public static string HashHmacSha512(string input, string secret)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret), "An HMAC needs a secret key.");
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] key = Encoding.UTF8.GetBytes(secret);
            using HMACSHA512 hmac = new HMACSHA512(key);
            return ToHex(hmac.ComputeHash(bytes));
        }

        /// <summary>
        /// Get Hash of string (Hmac sha 256), uppercase hex without dashes.
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <param name="secret">key string for secret hash</param>
        /// <returns>[input] hash; null or empty input is returned as is</returns>
        /// <exception cref="ArgumentNullException"><paramref name="secret"/> is null.</exception>
        public static string HashHmacSha256(string input, string secret)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret), "An HMAC needs a secret key.");
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] key = Encoding.UTF8.GetBytes(secret);
            using HMACSHA256 hmac = new HMACSHA256(key);
            return ToHex(hmac.ComputeHash(bytes));
        }

        /// <summary>
        /// Get Hash of string (sha 512), uppercase hex without dashes.
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <returns>[input] hash. NOTE: null returns null and "" returns "", so comparing against an empty stored
        /// hash matches an empty input. Use <see cref="TryHashSha512"/> to refuse empty input.</returns>
        public static string HashSha512(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA512.Create();
            return ToHex(sha.ComputeHash(bytes));
        }

        /// <summary>
        /// Get Hash of string (sha 512) and refuse null or empty input.
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <param name="hash">uppercase hex hash, or null when the input is empty</param>
        /// <returns>false when the input is null or empty</returns>
        public static bool TryHashSha512(string input, out string hash)
        {
            hash = string.IsNullOrEmpty(input) ? null : HashSha512(input);
            return hash != null;
        }

        /// <summary>
        /// Get Hash of string (sha 256), uppercase hex without dashes.
        /// </summary>
        /// <param name="input">String to get hash for it</param>
        /// <returns>[input] hash; null or empty input is returned as is</returns>
        public static string HashSha256(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA256.Create();
            return ToHex(sha.ComputeHash(bytes));
        }

        /// <summary>
        /// Compare two hex strings (hashes, HMACs) in constant time, ignoring case.
        /// </summary>
        /// <param name="hexA">first hex string</param>
        /// <param name="hexB">second hex string</param>
        /// <returns>true when both are valid, non-empty hex of the same value</returns>
        public static bool FixedTimeEquals(string hexA, string hexB)
        {
            if (!TryFromHex(hexA, out var a) || !TryFromHex(hexB, out var b) || a.Length == 0)
            {
                return false;
            }
            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        /// <summary>
        /// Hash a human password with PBKDF2-SHA256 (600,000 iterations, 16-byte random salt).
        /// Format: <c>pbkdf2-sha256$600000$&lt;base64 salt&gt;$&lt;base64 hash&gt;</c>.
        /// </summary>
        /// <param name="password">password (not null or empty)</param>
        /// <returns>versioned hash string to store</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("The password must not be empty.", nameof(password));
            }
            byte[] salt = new byte[PasswordSaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            byte[] hash = Pbkdf2(password, salt, PasswordIterations, PasswordHashSize);
            return $"{PasswordHashPrefix}${PasswordIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Check a password against a value produced by <see cref="HashPassword"/> (constant-time compare).
        /// </summary>
        /// <param name="password">password to check</param>
        /// <param name="stored">stored hash string</param>
        /// <returns>true when the password matches; false for any malformed input</returns>
        public static bool VerifyPassword(string password, string stored)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored))
            {
                return false;
            }
            var parts = stored.Split('$');
            if (parts.Length != 4 || parts[0] != PasswordHashPrefix
                || !int.TryParse(parts[1], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var iterations)
                || iterations < 1 || iterations > MaxPasswordIterations)
            {
                return false;
            }
            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                if (salt.Length == 0 || expected.Length == 0)
                {
                    return false;
                }
                byte[] actual = Pbkdf2(password, salt, iterations, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Remove HTML tags (complete or unterminated) from a string.
        /// This is NOT an XSS defence: HTML-encode values when you output them.
        /// </summary>
        /// <param name="data">html string</param>
        /// <returns>text without tags, trimmed</returns>
        public static string StripTags(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return data;
            }
            return TagPattern.Replace(data, string.Empty).Trim();
        }

        /// <summary>
        /// Remove tags from string. Renamed to <see cref="StripTags"/>.
        /// </summary>
        /// <param name="data">html string</param>
        /// <returns>text without tags</returns>
        [Obsolete("Renamed to StripTags. It strips tags only; HTML-encode output to prevent XSS.")]
        public static string RemoveJavaScript(string data)
        {
            return StripTags(data);
        }

        /// <summary>
        /// Cryptographically random numeric code with exactly <paramref name="digits"/> digits (no leading zero),
        /// for OTPs and verification codes.
        /// </summary>
        /// <param name="digits">1 to 9</param>
        /// <returns>random number between 10^(digits-1) and 10^digits - 1</returns>
        public static int GenerateNumericCode(int digits)
        {
            if (digits < 1 || digits > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(digits), "digits must be between 1 and 9.");
            }
            int min = digits == 1 ? 0 : Pow10(digits - 1);
            return RandomNumberGenerator.GetInt32(min, Pow10(digits));
        }

        /// <summary>
        /// Get random number. NOTE: returns <paramref name="digit"/> + 1 digits and overflows for 9 or more;
        /// kept unchanged for compatibility.
        /// </summary>
        /// <param name="digit">Number of digits for random number</param>
        /// <returns>Random number</returns>
        [Obsolete("Returns digit+1 digits and is not cryptographically random. Use GenerateNumericCode.")]
        public static int GenerateRandomNumber(int digit)
        {
            int min = Convert.ToInt32(Math.Pow(10, digit));
            int max = Convert.ToInt32(Math.Pow(10, digit + 1) - 1);
            lock (RandomLock)
            {
                return Random.Next(min, max);
            }
        }

        /// <summary>
        /// Simple one-way scramble of a string with salt. NOTE: drops the last character of odd-length input;
        /// kept unchanged for compatibility.
        /// </summary>
        /// <param name="input">flat string</param>
        /// <param name="salt">salt to be added to encrypted text</param>
        /// <returns>encrypted text</returns>
        [Obsolete("Not encryption and loses a character on odd-length input. Use HashHmacSha256 or SecretProtector.")]
        public static string CustomEncrypt(string input, string salt)
        {
            int length = input.Length;
            int halflength = length / 2;
            return ReverseString($"{input[..halflength]}_{salt}{Key.Substring(length, length)}_{input.Substring(halflength, halflength)}");
        }


        #endregion

        #region Helper
        private static char[] GenerateEncrypt(string input, string str2, string salt)
        {
            var saltLength = salt.Length;
            char[] chArray = new char[(input.Length * 2) + saltLength];
            int num;
            lock (RandomLock)
            {
                num = Random.Next(0, 2502 - input.Length);
            }
            string str3 = num.ToString("0000", System.Globalization.CultureInfo.InvariantCulture);
            if (str3.Length < saltLength)
            {
                str3 += salt.Substring(0, saltLength - str3.Length);
            }
            for (int c = 0; c < saltLength; c++)
            {
                chArray[c] = (char)(str3[c] ^ salt[c]);
            }
            for (int i = 0; i < input.Length; i++)
            {
                chArray[saltLength + (i * 2)] = (char)(input[i] ^ Key[num + i]);
                chArray[(saltLength + 1) + (i * 2)] = str2[i % 30];
            }

            return chArray;
        }

        private static char[] GenerateDecrypt(string input, string salt)
        {
            var saltLength = salt.Length;
            int num = (input.Length - saltLength) / 2;
            char[] chArray = new char[saltLength];
            for (int c = 0; c < saltLength; c++)
            {
                chArray[c] = (char)(input[c] ^ salt[c]);
            }

            char[] chArray2 = new char[num];
            int num2 = Convert.ToInt32(new string(chArray[..4]), System.Globalization.CultureInfo.InvariantCulture);
            for (int i = 0; i < num; i++)
            {
                chArray2[i] = (char)(input[(i * 2) + saltLength] ^ Key[num2 + i]);
            }
            return chArray2;
        }

        private static string ReverseString(string input)
        {
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private static int Pow10(int exponent)
        {
            int result = 1;
            for (int i = 0; i < exponent; i++)
            {
                result *= 10;
            }
            return result;
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int length)
        {
#if NET6_0_OR_GREATER
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, length);
#else
            using var kdf = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return kdf.GetBytes(length);
#endif
        }

        internal static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        internal static bool TryFromHex(string hex, out byte[] bytes)
        {
            bytes = null;
            if (hex == null || hex.Length % 2 != 0)
            {
                return false;
            }
            var result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                int hi = HexValue(hex[2 * i]);
                int lo = HexValue(hex[(2 * i) + 1]);
                if (hi < 0 || lo < 0)
                {
                    return false;
                }
                result[i] = (byte)((hi << 4) | lo);
            }
            bytes = result;
            return true;
        }

        private static int HexValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            return -1;
        }

        #endregion
    }
}
