namespace ObeTools
{
    /// <summary>
    /// Calculate VAT taxes
    /// </summary>
    public static class VATHelper
    {
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
        /// Get Total, Subtotal and VAT after apply taxes.
        /// </summary>
        /// <remarks>
        /// When <paramref name="rateIncludeVAT"/> is false the three inputs are only rounded.
        /// When it is true the parameters change meaning: <paramref name="inputSub"/> is read as the GROSS amount
        /// (VAT included) and <paramref name="inputVat"/>/<paramref name="inputSub"/> as the VAT rate. Example: a price of
        /// 100 that includes 15% VAT, computed as if it were net (vat 15, total 115):
        /// GetRate(115, 15, 100, true, 2) = (total 100, vat 13.04, sub 86.96).
        /// Prefer <see cref="SplitVat"/> for new code.
        /// </remarks>
        /// <param name="inputTotal">total amount (ignored when <paramref name="rateIncludeVAT"/> is true)</param>
        /// <param name="inputVat">VAT amount (with <paramref name="rateIncludeVAT"/>: rate * inputSub)</param>
        /// <param name="inputSub">subtotal amount (with <paramref name="rateIncludeVAT"/>: the gross amount)</param>
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

        /// <summary>
        /// Split an amount into net, VAT and gross with decimal maths, rounding half away from zero.
        /// VAT is rounded once and the third figure is derived, so Net + Vat == Gross always holds.
        /// </summary>
        /// <param name="amount">the amount (net, or gross when <paramref name="amountIncludesVat"/>)</param>
        /// <param name="ratePercent">VAT rate in percent, e.g. 15</param>
        /// <param name="amountIncludesVat">true when <paramref name="amount"/> already includes VAT</param>
        /// <param name="digits">decimals (default 2)</param>
        /// <returns>Net, Vat and Gross</returns>
        /// <example>SplitVat(1.90m, 15, false) = (1.90, 0.29, 2.19); SplitVat(115m, 15, true) = (100.00, 15.00, 115.00)</example>
        public static (decimal Net, decimal Vat, decimal Gross) SplitVat(decimal amount, decimal ratePercent, bool amountIncludesVat, int digits = 2)
        {
            if (ratePercent < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(ratePercent), ratePercent, "The VAT rate must not be negative.");
            }

            if (amountIncludesVat)
            {
                var gross = NumberHelper.RoundMoney(amount, digits);
                var net = NumberHelper.RoundMoney(gross / (1 + (ratePercent / 100m)), digits);
                return (net, gross - net, gross);
            }
            else
            {
                var net = NumberHelper.RoundMoney(amount, digits);
                var vat = NumberHelper.RoundMoney(net * ratePercent / 100m, digits);
                return (net, vat, net + vat);
            }
        }
        #endregion

    }
}
