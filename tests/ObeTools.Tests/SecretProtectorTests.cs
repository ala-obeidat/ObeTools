using System;
using System.Collections.Generic;

using Xunit;

namespace ObeTools.Tests
{
    public class SecretProtectorTests
    {
        private static readonly byte[] Key = SecretProtector.GenerateKey();

        [Theory]
        [InlineData("acc_9f8e7d_638000000000000000@")]
        [InlineData("")]
        [InlineData("مرحبا 👋 emoji")]
        public void RoundTrips(string plaintext)
        {
            var token = SecretProtector.Protect(plaintext, Key, "ld-login");

            Assert.StartsWith("v2.", token);
            Assert.Matches("^v2\\.[A-Za-z0-9_-]+$", token);
            Assert.True(SecretProtector.TryUnprotect(token, Key, "ld-login", out var back));
            Assert.Equal(plaintext, back);
        }

        [Fact]
        public void SameInput_GivesDifferentTokens()
        {
            Assert.NotEqual(SecretProtector.Protect("x", Key, "p"), SecretProtector.Protect("x", Key, "p"));
        }

        [Fact]
        public void WrongPurpose_Fails()
        {
            var token = SecretProtector.Protect("42", Key, "waybill-link");
            Assert.False(SecretProtector.TryUnprotect(token, Key, "ld-login", out var value));
            Assert.Null(value);
        }

        [Fact]
        public void WrongKey_Fails()
        {
            var token = SecretProtector.Protect("42", Key, "p");
            Assert.False(SecretProtector.TryUnprotect(token, SecretProtector.GenerateKey(), "p", out _));
        }

        [Fact]
        public void AnyChangedCharacter_Fails()
        {
            var token = SecretProtector.Protect("12345", Key, "p");
            for (int i = 3; i < token.Length; i++)
            {
                var chars = token.ToCharArray();
                chars[i] = chars[i] == 'A' ? 'B' : 'A';
                Assert.False(SecretProtector.TryUnprotect(new string(chars), Key, "p", out _), $"position {i}");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("v2.")]
        [InlineData("v2.!!!!")]
        [InlineData("v2.AAAA")]
        [InlineData("UURTXQJid2FjMlw2ejE_")] // a legacy Encrypt token
        public void Garbage_FailsWithoutThrowing(string token)
        {
            Assert.False(SecretProtector.TryUnprotect(token, Key, "p", out var value));
            Assert.Null(value);
        }

        [Fact]
        public void KeyRing_AcceptsOlderKeys()
        {
            var oldKey = SecretProtector.GenerateKey();
            var newKey = SecretProtector.GenerateKey();
            var oldToken = SecretProtector.Protect("legacy", oldKey, "p");

            Assert.True(SecretProtector.TryUnprotect(oldToken, new List<byte[]> { newKey, oldKey }, "p", out var value));
            Assert.Equal("legacy", value);
            Assert.False(SecretProtector.TryUnprotect(oldToken, new List<byte[]> { newKey }, "p", out _));
        }

        [Fact]
        public void BadKey_ThrowsOnProtect()
        {
            Assert.Throws<ArgumentException>(() => SecretProtector.Protect("x", new byte[16], "p"));
            Assert.Throws<ArgumentNullException>(() => SecretProtector.Protect("x", (byte[])null, "p"));
            Assert.Throws<ArgumentNullException>(() => SecretProtector.Protect("x", Key, null));
        }

        [Fact]
        public void ParseKey()
        {
            var key = SecretProtector.ParseKey(Convert.ToBase64String(Key));
            Assert.Equal(Key, key);
            Assert.Throws<ArgumentException>(() => SecretProtector.ParseKey(Convert.ToBase64String(new byte[31])));
            Assert.Throws<ArgumentException>(() => SecretProtector.ParseKey("not base64!"));
        }

        [Fact]
        public void IsProtectedToken()
        {
            Assert.True(SecretProtector.IsProtectedToken(SecretProtector.Protect("x", Key, "p")));
            Assert.False(SecretProtector.IsProtectedToken("UURTXQJid2FjMlw2ejE_"));
            Assert.False(SecretProtector.IsProtectedToken(null));
        }
    }
}
