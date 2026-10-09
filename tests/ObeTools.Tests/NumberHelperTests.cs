using System;
using System.Globalization;
using System.Threading;

using ObeTools.Model;

using Xunit;

namespace ObeTools.Tests
{
    public class NumberHelperTests
    {
        private static string En(double v) => NumberHelper.NumberToWords(v, NumberWordType.English, "ريال", "SAR", "هللة", "Halala");
        private static string Ar(double v) => NumberHelper.NumberToWords(v, NumberWordType.Arabic, "ريال", "SAR", "هللة", "Halala");

        [Theory]
        [InlineData(0, "Zero SAR")]
        [InlineData(1, "One SAR")]
        [InlineData(11, "Eleven SAR")]
        [InlineData(21, "Twenty One SAR")]
        [InlineData(40, "Forty SAR")]
        [InlineData(345, "Three Hundred Forty Five SAR")]
        [InlineData(1001, "One Thousand One SAR")]
        [InlineData(1100, "One Thousand One Hundred SAR")]
        [InlineData(1000001, "One Million One SAR")]
        [InlineData(12345678, "Twelve Million Three Hundred Forty Five Thousand Six Hundred Seventy Eight SAR")]
        [InlineData(123456789012, "One Hundred Twenty Three Billion Four Hundred Fifty Six Million Seven Hundred Eighty Nine Thousand Twelve SAR")]
        [InlineData(0.5, "Zero SAR and Fifty Halala")]
        [InlineData(0.01, "Zero SAR and One Halala")]
        [InlineData(1.5, "One SAR and Fifty Halala")]
        [InlineData(1234.56, "One Thousand Two Hundred Thirty Four SAR and Fifty Six Halala")]
        [InlineData(9.999, "Ten SAR")]
        [InlineData(1.005, "One SAR and One Halala")]   // half away from zero
        [InlineData(0.001, "Zero SAR")]
        [InlineData(-15.5, "Minus Fifteen SAR and Fifty Halala")]
        public void English(double value, string expected)
        {
            Assert.Equal(expected, En(value));
        }

        // Reviewed table; send to a native speaker before changing.
        [Theory]
        [InlineData(0, "صفر ريال")]
        [InlineData(1, "واحد ريال")]
        [InlineData(2, "إثنان ريال")]
        [InlineData(11, "أحد عشر ريال")]
        [InlineData(12, "إثنا عشر ريال")]
        [InlineData(21, "واحد وعشرون ريال")]
        [InlineData(100, "مئة ريال")]
        [InlineData(200, "مئتين ريال")]
        [InlineData(345, "ثلاثمائة وخمسة وأربعون ريال")]
        [InlineData(800, "ثمانمائة ريال")]
        [InlineData(1000, "ألف ريال")]
        [InlineData(1001, "ألف وواحد ريال")]
        [InlineData(1234, "ألف ومئتين وأربعة وثلاثون ريال")]
        [InlineData(2000, "ألفين ريال")]
        [InlineData(3000, "ثلاثة آلاف ريال")]
        [InlineData(8000, "ثمانية آلاف ريال")]
        [InlineData(10000, "عشرة آلاف ريال")]
        [InlineData(11000, "أحد عشر ألفاً ريال")]
        [InlineData(20000, "عشرون ألفاً ريال")]
        [InlineData(100000, "مئة ألف ريال")]
        [InlineData(103000, "مئة وثلاثة آلاف ريال")]
        [InlineData(200000, "مئتين ألف ريال")]
        [InlineData(1000000, "مليون ريال")]
        [InlineData(1000001, "مليون وواحد ريال")]
        [InlineData(2000000, "مليونين ريال")]
        [InlineData(3000000, "ثلاثة ملايين ريال")]
        [InlineData(15000000, "خمسة عشر مليوناً ريال")]
        [InlineData(1000000000, "مليار ريال")]
        [InlineData(5000000000, "خمسة مليارات ريال")]
        [InlineData(0.5, "صفر ريال وخمسون هللة")]
        [InlineData(1.5, "واحد ريال وخمسون هللة")]
        [InlineData(1234.56, "ألف ومئتين وأربعة وثلاثون ريال وستة وخمسون هللة")]
        [InlineData(-15.5, "سالب خمسة عشر ريال وخمسون هللة")]
        public void Arabic(double value, string expected)
        {
            Assert.Equal(expected, Ar(value));
        }

        [Fact]
        public void All_IsEnglishBrArabic()
        {
            Assert.Equal("One SAR and Fifty Halala<br>واحد ريال وخمسون هللة",
                NumberHelper.NumberToWords(1.5, NumberWordType.All, "ريال", "SAR", "هللة", "Halala"));
        }

        [Fact]
        public void All_DoesNotThrowWhereItUsedTo()
        {
            Assert.Equal("Ten SAR<br>عشرة ريال", NumberHelper.NumberToWords(9.999, NumberWordType.All, "ريال", "SAR"));
        }

        [Fact]
        public void DefaultCentLabels()
        {
            Assert.Equal("One SAR and Fifty Halalah", NumberHelper.NumberToWords(1.5, NumberWordType.English, "ريال", "SAR"));
            Assert.Equal("واحد ريال وخمسون هللة", NumberHelper.NumberToWords(1.5, NumberWordType.Arabic, "ريال", "SAR"));
        }

        [Theory]
        [InlineData("ar-SA")]
        [InlineData("fr-FR")]
        [InlineData("de-DE")]
        public void DoesNotDependOnCurrentCulture(string culture)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                Assert.Equal("One SAR and Fifty Halala", En(1.5));
                Assert.Equal("One Thousand Two Hundred Thirty Four SAR and Fifty Six Halala", En(1234.56));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Theory]
        [InlineData(1e12)]
        [InlineData(-1e12)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(1e30)]
        public void OutOfRange_Throws(double value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => En(value));
        }

        [Fact]
        public void AmountToWords_Decimal()
        {
            Assert.Equal("Nine Hundred Ninety Nine Billion Nine Hundred Ninety Nine Million Nine Hundred Ninety Nine Thousand Nine Hundred Ninety Nine SAR and Ninety Nine Halala",
                NumberHelper.AmountToWords(NumberHelper.MaxWordsAmount, NumberWordType.English, "ريال", "SAR", "هللة", "Halala"));
        }

        [Fact]
        public void EmptyCurrency_IsOmitted()
        {
            Assert.Equal("Five", NumberHelper.NumberToWords(5, NumberWordType.English, "", ""));
        }

        // Round keeps 1.6 behaviour (double + banker's rounding); invoices already issued depend on it.
        [Fact]
        public void Round_IsUnchanged()
        {
            Assert.Equal(0.28, NumberHelper.Round(0.285, 2));
            Assert.Equal(1, NumberHelper.Round(1.005, 2));
            Assert.Equal(2, NumberHelper.Round(2.5, 0));
        }

        [Theory]
        [InlineData("0.285", 2, "0.29")]
        [InlineData("1.005", 2, "1.01")]
        [InlineData("2.5", 0, "3")]
        [InlineData("-0.285", 2, "-0.29")]
        [InlineData("0.284", 2, "0.28")]
        public void RoundMoney_IsHalfAwayFromZero(string value, int digits, string expected)
        {
            Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture),
                NumberHelper.RoundMoney(decimal.Parse(value, CultureInfo.InvariantCulture), digits));
        }

        public class Sample
        {
            public double D { get; set; } = 1.005;
            public double? Nd { get; set; } = 0.285;
            public decimal M { get; set; } = 0.285m;
            public decimal? Nm { get; set; } = 1.005m;
            public double ReadOnly => 1.23456;
            public double PrivateSet { get; private set; } = 2.345;
        }

        [Fact]
        public void RoundObject_Legacy_OnlyDoubles_SkipsReadOnly()
        {
            var s = new Sample();
            NumberHelper.RoundObject(s, 2);

            Assert.Equal(1, s.D);
            Assert.Equal(0.285, s.Nd);   // untouched, as in 1.6
            Assert.Equal(0.285m, s.M);    // untouched, as in 1.6
            Assert.Equal(Math.Round(2.345, 2), s.PrivateSet);
        }

        [Fact]
        public void RoundObject_WithMode_RoundsAllNumericKinds()
        {
            var s = new Sample();
            NumberHelper.RoundObject(s, 2, MidpointRounding.AwayFromZero);

            Assert.Equal(0.29m, s.M);
            Assert.Equal(1.01m, s.Nm);
            Assert.Equal(Math.Round(0.285, 2, MidpointRounding.AwayFromZero), s.Nd);
        }
    }
}
