using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ObeTools
{
    /// <summary>
    /// Authenticated encryption (AES-256-GCM) for tokens, links and other values that must stay secret and
    /// must not be forged. Replacement for <see cref="EncryptHasher.Encrypt"/>/<see cref="EncryptHasher.Decrypt"/>.
    /// </summary>
    /// <remarks>
    /// <para>Token format: <c>"v2." + base64url(nonce[12] | ciphertext | tag[16])</c>, URL-safe without padding.</para>
    /// <para>The key (32 random bytes, see <see cref="GenerateKey"/>) is supplied by the APPLICATION from a secret
    /// store or environment variable; it is never a constant in a package.</para>
    /// <para>The purpose (for example "ld-login", "waybill-link", "clickpay-udf1") is bound to the token as associated
    /// data: a token minted for one purpose fails for every other purpose.</para>
    /// <para>For key rotation, pass the key ring newest first to <see cref="TryUnprotect(string, IEnumerable{byte[]}, string, out string)"/>
    /// and always protect with the newest key.</para>
    /// </remarks>
    public static class SecretProtector
    {
        /// <summary>
        /// Prefix of every token produced by <see cref="Protect(string, byte[], string)"/>.
        /// </summary>
        public const string TokenPrefix = "v2.";

        /// <summary>
        /// Key size in bytes (AES-256).
        /// </summary>
        public const int KeySize = 32;

        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const string AadPrefix = "ObeTools.SecretProtector.v2|";

        /// <summary>
        /// Create a new random 32-byte key. Store it as base64 (see <see cref="ParseKey"/>).
        /// </summary>
        /// <returns>32 random bytes</returns>
        public static byte[] GenerateKey()
        {
            var key = new byte[KeySize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }

        /// <summary>
        /// Read a key stored as standard base64 (for example from an environment variable).
        /// </summary>
        /// <param name="base64Key">base64 of 32 bytes</param>
        /// <returns>key bytes</returns>
        /// <exception cref="ArgumentException">The value is not base64 of exactly 32 bytes.</exception>
        public static byte[] ParseKey(string base64Key)
        {
            byte[] key;
            try
            {
                key = Convert.FromBase64String(base64Key ?? string.Empty);
            }
            catch (FormatException)
            {
                throw new ArgumentException("The key is not valid base64.", nameof(base64Key));
            }
            if (key.Length != KeySize)
            {
                throw new ArgumentException($"The key must be {KeySize} bytes.", nameof(base64Key));
            }
            return key;
        }

        /// <summary>
        /// Is this value in the <see cref="SecretProtector"/> token format (as opposed to a legacy token)?
        /// </summary>
        /// <param name="token">token</param>
        /// <returns>true when it starts with <see cref="TokenPrefix"/></returns>
        public static bool IsProtectedToken(string token)
        {
            return token != null && token.StartsWith(TokenPrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Encrypt and authenticate a value.
        /// </summary>
        /// <param name="plaintext">value to protect (UTF-8; any Unicode text round-trips)</param>
        /// <param name="key">32-byte key</param>
        /// <param name="purpose">what the token is for; must be the same when unprotecting</param>
        /// <returns>URL-safe token</returns>
        public static string Protect(string plaintext, byte[] key, string purpose)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return Protect(plaintext, new ReadOnlySpan<byte>(key), purpose);
        }

        /// <summary>
        /// Encrypt and authenticate a value.
        /// </summary>
        /// <param name="plaintext">value to protect (UTF-8; any Unicode text round-trips)</param>
        /// <param name="key">32-byte key</param>
        /// <param name="purpose">what the token is for; must be the same when unprotecting</param>
        /// <returns>URL-safe token</returns>
        public static string Protect(string plaintext, ReadOnlySpan<byte> key, string purpose)
        {
            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }
            if (purpose == null)
            {
                throw new ArgumentNullException(nameof(purpose));
            }
            if (key.Length != KeySize)
            {
                throw new ArgumentException($"The key must be {KeySize} bytes.", nameof(key));
            }

            byte[] data = Encoding.UTF8.GetBytes(plaintext);
            byte[] output = new byte[NonceSize + data.Length + TagSize];
            var nonce = new Span<byte>(output, 0, NonceSize);
            var cipher = new Span<byte>(output, NonceSize, data.Length);
            var tag = new Span<byte>(output, NonceSize + data.Length, TagSize);

            using (var rng = RandomNumberGenerator.Create())
            {
                var nonceBytes = new byte[NonceSize];
                rng.GetBytes(nonceBytes);
                nonceBytes.CopyTo(nonce);
            }

            using (var aes = CreateAesGcm(key))
            {
                aes.Encrypt(nonce, data, cipher, tag, Aad(purpose));
            }

            return TokenPrefix + ToBase64Url(output);
        }

        /// <summary>
        /// Decrypt and verify a token. Never throws.
        /// </summary>
        /// <param name="token">token from <see cref="Protect(string, byte[], string)"/></param>
        /// <param name="key">32-byte key</param>
        /// <param name="purpose">the purpose used when protecting</param>
        /// <param name="plaintext">the value, or null when the token is not valid</param>
        /// <returns>false for any failure: wrong format, key or purpose, or a changed token</returns>
        public static bool TryUnprotect(string token, byte[] key, string purpose, out string plaintext)
        {
            plaintext = null;
            return key != null && TryUnprotect(token, new ReadOnlySpan<byte>(key), purpose, out plaintext);
        }

        /// <summary>
        /// Decrypt and verify a token. Never throws.
        /// </summary>
        /// <param name="token">token from <see cref="Protect(string, byte[], string)"/></param>
        /// <param name="key">32-byte key</param>
        /// <param name="purpose">the purpose used when protecting</param>
        /// <param name="plaintext">the value, or null when the token is not valid</param>
        /// <returns>false for any failure: wrong format, key or purpose, or a changed token</returns>
        public static bool TryUnprotect(string token, ReadOnlySpan<byte> key, string purpose, out string plaintext)
        {
            plaintext = null;
            if (purpose == null || key.Length != KeySize || !TryDecodeToken(token, out var raw))
            {
                return false;
            }
            return TryDecrypt(raw, key, purpose, out plaintext);
        }

        /// <summary>
        /// Decrypt and verify a token with a key ring (key rotation). Keys are tried in order, so pass the newest first.
        /// Never throws.
        /// </summary>
        /// <param name="token">token from <see cref="Protect(string, byte[], string)"/></param>
        /// <param name="keys">keys, newest first</param>
        /// <param name="purpose">the purpose used when protecting</param>
        /// <param name="plaintext">the value, or null when no key opens the token</param>
        /// <returns>true when one of the keys opened the token</returns>
        public static bool TryUnprotect(string token, IEnumerable<byte[]> keys, string purpose, out string plaintext)
        {
            plaintext = null;
            if (keys == null || purpose == null || !TryDecodeToken(token, out var raw))
            {
                return false;
            }
            foreach (var key in keys)
            {
                if (key != null && key.Length == KeySize && TryDecrypt(raw, key, purpose, out plaintext))
                {
                    return true;
                }
            }
            plaintext = null;
            return false;
        }

        #region Helper

        private static bool TryDecodeToken(string token, out byte[] raw)
        {
            raw = null;
            if (!IsProtectedToken(token))
            {
                return false;
            }
            if (!TryFromBase64Url(token.Substring(TokenPrefix.Length), out raw))
            {
                return false;
            }
            return raw.Length >= NonceSize + TagSize;
        }

        private static bool TryDecrypt(byte[] raw, ReadOnlySpan<byte> key, string purpose, out string plaintext)
        {
            plaintext = null;
            int dataLength = raw.Length - NonceSize - TagSize;
            var data = new byte[dataLength];
            try
            {
                using var aes = CreateAesGcm(key);
                aes.Decrypt(
                    new ReadOnlySpan<byte>(raw, 0, NonceSize),
                    new ReadOnlySpan<byte>(raw, NonceSize, dataLength),
                    new ReadOnlySpan<byte>(raw, NonceSize + dataLength, TagSize),
                    data,
                    Aad(purpose));
            }
            catch (CryptographicException)
            {
                return false;
            }

            try
            {
                plaintext = new UTF8Encoding(false, true).GetString(data);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static AesGcm CreateAesGcm(ReadOnlySpan<byte> key)
        {
#if NET8_0_OR_GREATER
            return new AesGcm(key, TagSize);
#else
            return new AesGcm(key);
#endif
        }

        private static byte[] Aad(string purpose)
        {
            return Encoding.UTF8.GetBytes(AadPrefix + purpose);
        }

        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static bool TryFromBase64Url(string value, out byte[] bytes)
        {
            bytes = null;
            if (value.Length % 4 == 1)
            {
                return false;
            }
            var sb = new StringBuilder(value.Length + 2);
            foreach (var c in value)
            {
                if (c == '-') sb.Append('+');
                else if (c == '_') sb.Append('/');
                else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) sb.Append(c);
                else return false;
            }
            sb.Append('=', (4 - (value.Length % 4)) % 4);
            try
            {
                bytes = Convert.FromBase64String(sb.ToString());
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        #endregion
    }
}
