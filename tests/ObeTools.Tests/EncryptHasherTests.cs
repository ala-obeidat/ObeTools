using System;
using System.Linq;

using Xunit;

namespace ObeTools.Tests
{
    public class EncryptHasherTests
    {
        // Tokens produced by ObeTools 1.6.7: existing links, QR codes and tokens must keep decrypting.
        [Theory]
        [InlineData("UURTXQJid2FjMlw2ejE_", "", "12345")]
        [InlineData("U0JXWTI5K2Q6YSpkPDlDMXRlA2RbZB4yDzUMNW80", "", "Hello, World!")]
        [InlineData("UEtVWNicMNmkOdmLYdmxZtm0OG9m2Z432a5h2KVh2b9k2Ysz2IE32KQ0", "", "مرحبا بالعالم")]
        [InlineData("XElEUgENQg1mVWUMMBU2UGJeZWg3AmZDZgw1FjR.YgE0STRKN30zeWF1Y3o0VmRBYXcxA2YGNEo3cWRzNHs0QmFyNw__", "mysalt1", "acc_9f8e7d_638000000000000000@")]
        public void Decrypt_AcceptsTokensFrom_1_6_7(string token, string salt, string expected)
        {
            Assert.Equal(expected, EncryptHasher.Decrypt(token, salt));
        }

        [Theory]
        [InlineData("12345", "")]
        [InlineData("مرحبا", "")]
        [InlineData("x", "longer-salt")]
        public void Encrypt_RoundTrips(string input, string salt)
        {
            Assert.Equal(input, EncryptHasher.Decrypt(EncryptHasher.Encrypt(input, salt), salt));
        }

        [Fact]
        public void Encrypt_AcceptsTheMaximumLength()
        {
            var input = new string('a', EncryptHasher.MaxEncryptLength);
            Assert.Equal(input, EncryptHasher.Decrypt(EncryptHasher.Encrypt(input)));
        }

        [Fact]
        public void Encrypt_TooLong_ThrowsClearArgumentException()
        {
            var e = Assert.Throws<ArgumentException>(() => EncryptHasher.Encrypt(new string('a', EncryptHasher.MaxEncryptLength + 1)));
            Assert.Equal("input", e.ParamName);
        }

        [Fact]
        public void Encrypt_ShortSalt_ThrowsInsteadOfMakingAnUndecryptableToken()
        {
            var e = Assert.Throws<ArgumentException>(() => EncryptHasher.Encrypt("12345", "abc"));
            Assert.Equal("salt", e.ParamName);
        }

        [Fact]
        public void Encrypt_And_Decrypt_AreMarkedObsolete()
        {
            foreach (var name in new[] { "Encrypt", "Decrypt", "ToURLToken", "FromURLToken", "CustomEncrypt", "GenerateRandomNumber", "RemoveJavaScript" })
            {
                var method = typeof(EncryptHasher).GetMethods().First(m => m.Name == name);
                Assert.True(method.IsDefined(typeof(ObsoleteAttribute), false), name);
            }
        }

        [Fact]
        public void UrlToken_RoundTrips()
        {
            var token = new Payload { Id = 7, Name = "x" }.ToURLToken();
            var back = token.FromURLToken<Payload>();
            Assert.Equal(7, back.Id);
            Assert.Equal("x", back.Name);
        }

        public class Payload
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Theory]
        [InlineData("SGVsbG8", "Hello")]   // no padding
        [InlineData("SGVsbG8_", "Hello")]  // safe padding
        [InlineData("SGVsbA", "Hell")]
        [InlineData("SGVsbA__", "Hell")]
        [InlineData("SGVs", "Hel")]
        public void FromSafeBase64_PadsExactly(string input, string expected)
        {
            Assert.Equal(expected, EncryptHasher.FromSafeBase64(input));
            Assert.True(EncryptHasher.TryFromSafeBase64(input, out var output));
            Assert.Equal(expected, output);
        }

        [Theory]
        [InlineData("SGVsb")]       // impossible length
        [InlineData("!!!!")]
        public void FromSafeBase64_Garbage(string input)
        {
            Assert.Throws<FormatException>(() => EncryptHasher.FromSafeBase64(input));
            Assert.False(EncryptHasher.TryFromSafeBase64(input, out var output));
            Assert.Null(output);
        }

        [Fact]
        public void ToSafeBase64_FormatIsUnchanged()
        {
            Assert.Equal("-..-AQ__", EncryptHasher.ToSafeBase64(new byte[] { 0xfb, 0xff, 0xfe, 0x01 }));
        }

        // Output format (uppercase hex, no dashes) is stored by consumers: must never change.
        [Fact]
        public void Hashes_AreUnchangedFrom_1_6_7()
        {
            Assert.Equal("08482AE61662032A5A308B71337098C78A1932E284060186275BD3534DD01CED90B09F4B5A05E3F7B0179D4D56E36B5162349BFA7F357C1C3A7E0180E215A881", EncryptHasher.HashSha512("ObeTools"));
            Assert.Equal("C2394166C99AD8A64473F3C79C387351721328B7F926A4913CB499318565FA4F", EncryptHasher.HashSha256("ObeTools"));
            Assert.Equal("E281C1D8951AC807C7929C5B795421CC28C343D3FB5AEE313F90F30D6DC533FF8C6B2AB871203797264EA2C9D7319188DB3730DBA35BFA12F30A2251019AAC4E", EncryptHasher.HashHmacSha512("ObeTools", "secret"));
            Assert.Equal("2062D8566612FCE470618298F4446A70EA8A83897A6FD857FE7D7EF468D6BAED", EncryptHasher.HashHmacSha256("ObeTools", "secret"));
        }

        [Fact]
        public void HashSha512_EmptyAndNull_AreReturnedAsIs_AndTryHashRefusesThem()
        {
            Assert.Equal("", EncryptHasher.HashSha512(""));
            Assert.Null(EncryptHasher.HashSha512(null));
            Assert.False(EncryptHasher.TryHashSha512("", out var hash));
            Assert.Null(hash);
            Assert.True(EncryptHasher.TryHashSha512("ObeTools", out hash));
            Assert.Equal(EncryptHasher.HashSha512("ObeTools"), hash);
        }

        [Fact]
        public void Hmac_NullSecret_ThrowsClearError()
        {
            Assert.Equal("secret", Assert.Throws<ArgumentNullException>(() => EncryptHasher.HashHmacSha512("x", null)).ParamName);
            Assert.Equal("secret", Assert.Throws<ArgumentNullException>(() => EncryptHasher.HashHmacSha256("x", null)).ParamName);
        }

        [Fact]
        public void FixedTimeEquals_ComparesHexIgnoringCase()
        {
            var hash = EncryptHasher.HashSha256("ObeTools");
            Assert.True(EncryptHasher.FixedTimeEquals(hash, hash.ToLowerInvariant()));
            Assert.False(EncryptHasher.FixedTimeEquals(hash, EncryptHasher.HashSha256("obetools")));
            Assert.False(EncryptHasher.FixedTimeEquals(hash, hash.Substring(2)));
            Assert.False(EncryptHasher.FixedTimeEquals("", ""));
            Assert.False(EncryptHasher.FixedTimeEquals(null, hash));
            Assert.False(EncryptHasher.FixedTimeEquals("ZZ", "ZZ"));
        }

        [Fact]
        public void PasswordHash_VerifiesAndIsSalted()
        {
            var a = EncryptHasher.HashPassword("correct horse");
            var b = EncryptHasher.HashPassword("correct horse");

            Assert.StartsWith("pbkdf2-sha256$600000$", a);
            Assert.NotEqual(a, b);
            Assert.True(EncryptHasher.VerifyPassword("correct horse", a));
            Assert.False(EncryptHasher.VerifyPassword("correct horsE", a));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("sha512$1$AA==$AA==")]
        [InlineData("pbkdf2-sha256$abc$AA==$AA==")]
        [InlineData("pbkdf2-sha256$999999999$AA==$AA==")]
        [InlineData("pbkdf2-sha256$1000$%%%$AA==")]
        [InlineData("pbkdf2-sha256$1000$$")]
        public void VerifyPassword_MalformedStoredValue_IsFalse(string stored)
        {
            Assert.False(EncryptHasher.VerifyPassword("x", stored));
        }

        [Fact]
        public void HashPassword_EmptyThrows()
        {
            Assert.Throws<ArgumentException>(() => EncryptHasher.HashPassword(""));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(9)]
        public void GenerateNumericCode_HasExactlyTheDigits(int digits)
        {
            for (int i = 0; i < 200; i++)
            {
                var code = EncryptHasher.GenerateNumericCode(digits);
                Assert.Equal(digits, code.ToString().Length);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public void GenerateNumericCode_OutOfRange(int digits)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EncryptHasher.GenerateNumericCode(digits));
        }

        [Fact]
        public void GenerateRandomNumber_KeepsItsOldBehaviour()
        {
            Assert.Equal(5, EncryptHasher.GenerateRandomNumber(4).ToString().Length);
        }

        [Fact]
        public void CustomEncrypt_IsUnchangedFrom_1_6_7()
        {
            Assert.Equal("fed_e9Jj Ds_cba", EncryptHasher.CustomEncrypt("abcdef", "s"));
            Assert.Equal("dc_Jj DDs_ba", EncryptHasher.CustomEncrypt("abcde", "s"));
        }

        [Theory]
        [InlineData("<b>bold</b> text", "bold text")]
        [InlineData("a <script>alert(1)</script> b", "a alert(1) b")]
        [InlineData("x <img src=x onerror=alert(1)", "x")]
        [InlineData("<div\nclass='a'>y</div>", "y")]
        [InlineData("plain", "plain")]
        public void StripTags(string input, string expected)
        {
            Assert.Equal(expected, EncryptHasher.StripTags(input));
            Assert.Equal(expected, EncryptHasher.RemoveJavaScript(input));
        }
    }
}
