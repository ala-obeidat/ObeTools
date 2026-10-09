using System;

using Xunit;

namespace ObeTools.Tests
{
    public class Base1024EncodingTests
    {
        [Fact]
        public void EveryNumberBelow65536_RoundTrips()
        {
            for (int i = 0; i < 65536; i++)
            {
                Assert.Equal(i, Base1024Encoding.FromBase1024(Base1024Encoding.ToBase1024(i)));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(int.MaxValue)]
        public void LargeNumbers_RoundTrip(int value)
        {
            Assert.Equal(value, Base1024Encoding.FromBase1024(Base1024Encoding.ToBase1024(value)));
        }

        [Fact]
        public void NumbersThatWorkedIn_1_6_7_EncodeTheSame()
        {
            Assert.Equal("ρ≛", Base1024Encoding.ToBase1024(123456));
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData("مرحبا")]
        [InlineData("")]
        [InlineData("a")]
        public void Strings_RoundTripWithoutTrailingNull(string input)
        {
            Assert.Equal(input, Base1024Encoding.Base1024ToString(Base1024Encoding.StringToBase1024(input)));
        }

        [Fact]
        public void StringsFrom_1_6_7_StillDecode()
        {
            Assert.Equal("Hi", Base1024Encoding.Base1024ToString("ثℸβℹ")); // had a trailing separator
        }

        [Fact]
        public void Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Base1024Encoding.ToBase1024(-1));
        }

        [Fact]
        public void UnknownSymbol_Throws()
        {
            Assert.Throws<FormatException>(() => Base1024Encoding.FromBase1024("€"));
        }
    }
}
