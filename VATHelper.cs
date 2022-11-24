namespace ObeTools
{
    /// <summary>
    /// Calculate VAT taxes
    /// </summary>
    public static class VATHelper
    {
        #region Variable 
        #endregion

        #region Method

        /// <summary>
        /// Get rate price after apply taxes
        /// </summary>
        /// <param name="inputRate">price</param>
        /// <param name="inputTaxRate">tax pecentage</param>
        /// <param name="rateIncludeVAT">If price include VAT or not</param>
        /// <returns>Rate price</returns>
        public static double GetItemRate(double inputRate, double inputTaxRate, bool rateIncludeVAT)
        {
            var result = inputRate;
            if (rateIncludeVAT)
            {
                result = inputRate / ((inputTaxRate / 100.0) + 1);
            }
            return result;
        }

        /// <summary>
        /// Get rate price after apply taxes
        /// </summary>
        /// <param name="inputRate">price</param>
        /// <param name="inputTaxRate">tax pecentage</param>
        /// <param name="rateIncludeVAT">If price include VAT or not</param>
        /// <param name="roundDigits">How many digits after . to return</param>
        /// <returns>Rate price</returns>
        public static double GetItemRate(double inputRate, double inputTaxRate, bool rateIncludeVAT, int roundDigits)
        {
            var result = inputRate;
            if (rateIncludeVAT)
            {
                result = inputRate / ((inputTaxRate / 100.0) + 1);
            }
            return NumberHelper.Round(result, roundDigits);
        }

        /// <summary>
        /// Get Total, Subtotal and VAT after apply taxes
        /// </summary>
        /// <param name="inputTotal">total amount</param>
        /// <param name="inputVat">VAT amount</param>
        /// <param name="inputSub">subtotal amount</param>
        /// <param name="rateIncludeVAT">If price include VAT or not</param>
        /// <param name="roundDigits">How many digits after . to return</param>
        /// <returns>Total, Subtotal and VAT</returns>
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
            return (NumberHelper.Round(total, roundDigits), NumberHelper.Round(vat, roundDigits), NumberHelper.Round(sub, roundDigits));
        }
        #endregion

        #region Helper

        #endregion
    }
}
