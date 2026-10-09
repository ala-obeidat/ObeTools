using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace ObeTools.Tests
{
    public class TlvEncodingTests
    {
        private static readonly Dictionary<int, string> Zatca = new Dictionary<int, string>
        {
            { 1, "شركة أوبي" },
            { 2, "300000000000003" },
            { 3, "2026-10-09T12:00:00Z" },
            { 4, "115.00" },
            { 5, "15.00" }
        };

        [Fact]
        public void Encode_BytesAreUnchangedFrom_1_6_7()
        {
            Assert.Equal(
                "0111D8B4D8B1D983D8A920D8A3D988D8A8D98A020F3330303030303030303030303030330314323032362D31302D30395431323A30303A30305A04063131352E3030050531352E3030",
                Convert.ToHexString(TLVEncoding.Encode(Zatca)));
        }

        [Fact]
        public void RoundTrips()
        {
            Assert.Equal(Zatca, TLVEncoding.Decode(TLVEncoding.Encode(Zatca)));
        }

        [Fact]
        public void ValueOf255Bytes_IsAllowed()
        {
            var value = new string('a', 255);
            Assert.Equal(value, TLVEncoding.Decode(TLVEncoding.Encode(new Dictionary<int, string> { { 1, value } }))[1]);
        }

        [Fact]
        public void LongArabicSellerName_Throws_InsteadOfCorruptingTheQr()
        {
            var name = new string('ش', 128); // 256 UTF-8 bytes
            Assert.Throws<ArgumentOutOfRangeException>(() => TLVEncoding.Encode(new Dictionary<int, string> { { 1, name } }));
        }

        [Theory]
        [InlineData(256)]
        [InlineData(-1)]
        public void TagOutOfRange_Throws(int tag)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TLVEncoding.Encode(new Dictionary<int, string> { { tag, "x" } }));
        }

        [Fact]
        public void BinaryEncode_RoundTripsWithBinaryTags()
        {
            var hash = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
            var signature = Enumerable.Range(0, 72).Select(i => (byte)(255 - i)).ToArray();

            var bytes = TLVEncoding.Encode(new List<(int, byte[])> { (6, hash), (7, signature) });
            var decoded = TLVEncoding.Decode(bytes, new List<int> { 6, 7 });

            Assert.Equal(Convert.ToBase64String(hash), decoded[6]);
            Assert.Equal(Convert.ToBase64String(signature), decoded[7]);
        }

        [Fact]
        public void Decode_StopsAtTruncatedEntry()
        {
            var bytes = TLVEncoding.Encode(Zatca);
            var decoded = TLVEncoding.Decode(bytes.Take(bytes.Length - 1).ToArray());
            Assert.Equal(4, decoded.Count);
        }

        [Fact]
        public void EmptyInputs()
        {
            Assert.Empty(TLVEncoding.Encode((Dictionary<int, string>)null));
            Assert.Empty(TLVEncoding.Decode(null));
            Assert.Empty(TLVEncoding.Decode(Array.Empty<byte>()));
        }
    }
}
