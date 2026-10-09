using Xunit;

namespace ObeTools.Tests
{
    public class VatHelperTests
    {
        [Fact]
        public void LegacyApis_AreUnchanged()
        {
            Assert.Equal(100, VATHelper.GetItemRate(115, 15, true, 2));
            Assert.Equal(115, VATHelper.GetItemRate(115, 15, false, 2));
            var (total, vat, sub) = VATHelper.GetRate(115, 15, 100, true, 2);
            Assert.Equal((100, 13.04, 86.96), (total, vat, sub));
        }

        [Fact]
        public void SplitVat_Exclusive_RoundsHalfUp()
        {
            Assert.Equal((1.90m, 0.29m, 2.19m), VATHelper.SplitVat(1.90m, 15, false));
        }

        [Fact]
        public void SplitVat_Inclusive()
        {
            Assert.Equal((100.00m, 15.00m, 115.00m), VATHelper.SplitVat(115m, 15, true));
            var (net, vat, gross) = VATHelper.SplitVat(10m, 15, true);
            Assert.Equal(8.70m, net);
            Assert.Equal(1.30m, vat);
            Assert.Equal(gross, net + vat);
        }

        [Fact]
        public void SplitVat_NegativeRate_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => VATHelper.SplitVat(1m, -1, false));
        }
    }
}
