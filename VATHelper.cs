namespace ObeTools
{
    public static class VATHelper
    {
        #region Variable 
        #endregion

        #region Method
        public static double GetItemRate(double inputRate, double inputTaxRate, bool rateIncludeVAT, int roundDigits)
        {
            var result = inputRate;
            if (rateIncludeVAT)
            {
                result = inputRate / ((inputTaxRate / 100.0) + 1);
            }
            return RoundDouble.Round(result, roundDigits);
        }
        public static (double total, double vat, double sub) GetRate(double inputTotal, double inputVat, double inputSub, bool rateIncludeVAT, int roundDigits)
        {
            var total = inputTotal;
            var vat = inputVat;
            var sub = inputSub;
            if (rateIncludeVAT && inputSub > 0)
            {
                total = inputSub;
                var vatRate = inputVat / inputSub;
                sub = total / (1 + vatRate);
                vat = total - sub;
            }
            return (RoundDouble.Round(total, roundDigits), RoundDouble.Round(vat, roundDigits), RoundDouble.Round(sub, roundDigits));
        }
        #endregion

        #region Helper

        #endregion
    }
}
