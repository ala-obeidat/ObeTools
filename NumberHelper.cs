using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ObeTools.Model;

namespace ObeTools
{
    /// <summary>
    /// Methods to handle numbers
    /// </summary>
    public static class NumberHelper
    {
        #region Variable

        /// <summary>
        /// Largest whole amount <see cref="NumberToWords"/> can spell (999,999,999,999.99).
        /// </summary>
        public const decimal MaxWordsAmount = 999_999_999_999.99m;

        private static readonly string[] EnOnes =
        {
            "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
        };

        private static readonly string[] EnTens =
        {
            "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };

        private static readonly (long Value, string Name)[] EnScales =
        {
            (1_000_000_000, "Billion"), (1_000_000, "Million"), (1_000, "Thousand")
        };

        private static readonly string[] ArOnes =
        {
            "", "واحد", "إثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة"
        };

        private static readonly string[] ArTens =
        {
            "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"
        };

        private static readonly string[] ArHundreds =
        {
            "", "مئة", "مئتين", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة"
        };

        // Singular, dual, plural (3-10), accusative singular (11-99)
        private static readonly (long Value, string One, string Two, string Plural, string Accusative)[] ArScales =
        {
            (1_000_000_000, "مليار", "مليارين", "مليارات", "ملياراً"),
            (1_000_000, "مليون", "مليونين", "ملايين", "مليوناً"),
            (1_000, "ألف", "ألفين", "آلاف", "ألفاً")
        };

        private const string DefaultCentEn = "Halalah";
        private const string DefaultCentAr = "هللة";

        #endregion

        #region Method
        /// <summary>
        /// Display an amount as words, for example "One Thousand Two Hundred Thirty Four SAR and Fifty Six Halala".
        /// The amount is rounded to 2 decimals (midpoint away from zero) and does not depend on the current culture.
        /// </summary>
        /// <param name="number">amount in double format</param>
        /// <param name="type">words charectures language</param>
        /// <param name="currencyAr">Currency Label in Arabi</param>
        /// <param name="currencyEn">Currency Label in English</param>
        /// <param name="centLabelAr">Partial currency label in Arabic (default "هللة")</param>
        /// <param name="centLabelEn">Partial currency label in English (default "Halalah")</param>
        /// <returns>English text, Arabic text, or "English&lt;br&gt;Arabic" for <see cref="NumberWordType.All"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">The amount is NaN, infinite, or its magnitude exceeds <see cref="MaxWordsAmount"/>.</exception>
        public static string NumberToWords(double number, NumberWordType type, string currencyAr, string currencyEn, string centLabelAr = "", string centLabelEn = "")
        {
            return AmountToWords(ToDecimal(number, nameof(number)), type, currencyAr, currencyEn, centLabelAr, centLabelEn);
        }

        /// <summary>
        /// Display a decimal amount as words. Same output as <see cref="NumberToWords"/>.
        /// </summary>
        /// <param name="amount">amount</param>
        /// <param name="type">words charectures language</param>
        /// <param name="currencyAr">Currency Label in Arabi</param>
        /// <param name="currencyEn">Currency Label in English</param>
        /// <param name="centLabelAr">Partial currency label in Arabic (default "هللة")</param>
        /// <param name="centLabelEn">Partial currency label in English (default "Halalah")</param>
        /// <returns>English text, Arabic text, or "English&lt;br&gt;Arabic" for <see cref="NumberWordType.All"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">The magnitude exceeds <see cref="MaxWordsAmount"/>.</exception>
        public static string AmountToWords(decimal amount, NumberWordType type, string currencyAr, string currencyEn, string centLabelAr = "", string centLabelEn = "")
        {
            var rounded = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
            bool negative = rounded < 0;
            if (negative)
            {
                rounded = -rounded;
            }
            if (rounded > MaxWordsAmount)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, $"The amount must be between -{MaxWordsAmount} and {MaxWordsAmount}.");
            }

            long whole = (long)decimal.Truncate(rounded);
            int cents = (int)((rounded - whole) * 100);

            string centEn = string.IsNullOrEmpty(centLabelEn) ? DefaultCentEn : centLabelEn;
            string centAr = string.IsNullOrEmpty(centLabelAr) ? DefaultCentAr : centLabelAr;

            switch (type)
            {
                case NumberWordType.English:
                    return English(whole, cents, negative, currencyEn, centEn);
                case NumberWordType.Arabic:
                    return Arabic(whole, cents, negative, currencyAr, centAr);
                case NumberWordType.All:
                    return $"{English(whole, cents, negative, currencyEn, centEn)}<br>{Arabic(whole, cents, negative, currencyAr, centAr)}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown NumberWordType.");
            }
        }

        /// <summary>
        /// Round double to spasific number of digits.
        /// NOTE: this is <see cref="Math.Round(double, int)"/>: binary doubles with banker's rounding, so
        /// Round(0.285, 2) = 0.28 and Round(1.005, 2) = 1. Kept unchanged because stored invoices were computed with it.
        /// Use <see cref="RoundMoney"/> for new money calculations.
        /// </summary>
        /// <param name="input">double number</param>
        /// <param name="digits">number of digits</param>
        /// <returns>Number after round</returns>
        public static double Round(double input, int digits)
        {
            return Math.Round(input, digits);
        }

        /// <summary>
        /// Round a money amount half away from zero (0.285 => 0.29, -0.285 => -0.29).
        /// </summary>
        /// <param name="value">amount</param>
        /// <param name="digits">number of decimals (default 2)</param>
        /// <returns>rounded amount</returns>
        public static decimal RoundMoney(decimal value, int digits = 2)
        {
            return Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Round all double properties of an object to spasific number of digits (same rounding as <see cref="Round"/>).
        /// Read-only properties are skipped.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="input">input object</param>
        /// <param name="digits">number of digits</param>
        public static void RoundObject<T>(T input, int digits)
        {
            foreach (PropertyInfo property in WritableProperties(input, typeof(double)))
            {
                object dt = property.GetValue(input);

                if (dt == null)
                {
                    continue;
                }
                property.SetValue(input, Round(Convert.ToDouble(dt), digits));
            }
        }

        /// <summary>
        /// Round all double, double?, decimal and decimal? properties of an object with the given midpoint rule.
        /// Read-only properties are skipped.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="input">input object</param>
        /// <param name="digits">number of digits</param>
        /// <param name="mode">midpoint rule, for money use <see cref="MidpointRounding.AwayFromZero"/></param>
        public static void RoundObject<T>(T input, int digits, MidpointRounding mode)
        {
            foreach (PropertyInfo property in WritableProperties(input, typeof(double), typeof(double?), typeof(decimal), typeof(decimal?)))
            {
                object value = property.GetValue(input);
                switch (value)
                {
                    case null:
                        break;
                    case double d:
                        property.SetValue(input, Math.Round(d, digits, mode));
                        break;
                    case decimal m:
                        property.SetValue(input, Math.Round(m, digits, mode));
                        break;
                }
            }
        }
        #endregion

        #region Helper

        private static IEnumerable<PropertyInfo> WritableProperties(object input, params Type[] types)
        {
            if (input == null)
            {
                return Enumerable.Empty<PropertyInfo>();
            }
            return input.GetType().GetProperties()
                .Where(x => types.Contains(x.PropertyType) && x.CanWrite && x.GetIndexParameters().Length == 0);
        }

        private static decimal ToDecimal(double number, string paramName)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
            {
                throw new ArgumentOutOfRangeException(paramName, number, "The number must be finite.");
            }
            try
            {
                return (decimal)number; // rounds to 15 significant digits, so 0.1 stays 0.1
            }
            catch (OverflowException)
            {
                throw new ArgumentOutOfRangeException(paramName, number, $"The amount must be between -{MaxWordsAmount} and {MaxWordsAmount}.");
            }
        }

        private static string English(long whole, int cents, bool negative, string currency, string centLabel)
        {
            var parts = new List<string>();
            if (negative)
            {
                parts.Add("Minus");
            }
            parts.Add(whole == 0 ? "Zero" : EnglishWhole(whole));
            AddIfAny(parts, currency);
            if (cents > 0)
            {
                parts.Add("and");
                parts.Add(EnglishGroup(cents));
                AddIfAny(parts, centLabel);
            }
            return string.Join(" ", parts);
        }

        private static string EnglishWhole(long number)
        {
            var parts = new List<string>();
            foreach (var (value, name) in EnScales)
            {
                if (number >= value)
                {
                    parts.Add(EnglishGroup((int)(number / value)) + " " + name);
                    number %= value;
                }
            }
            if (number > 0)
            {
                parts.Add(EnglishGroup((int)number));
            }
            return string.Join(" ", parts);
        }

        // 1..999
        private static string EnglishGroup(int number)
        {
            var parts = new List<string>();
            if (number >= 100)
            {
                parts.Add(EnOnes[number / 100] + " Hundred");
                number %= 100;
            }
            if (number >= 20)
            {
                parts.Add(EnTens[number / 10]);
                number %= 10;
            }
            if (number > 0)
            {
                parts.Add(EnOnes[number]);
            }
            return string.Join(" ", parts);
        }

        private static string Arabic(long whole, int cents, bool negative, string currency, string centLabel)
        {
            var text = whole == 0 ? "صفر" : ArabicWhole(whole);
            if (!string.IsNullOrWhiteSpace(currency))
            {
                text += " " + currency.Trim();
            }
            if (cents > 0)
            {
                text += " و" + ArabicGroup(cents);
                if (!string.IsNullOrWhiteSpace(centLabel))
                {
                    text += " " + centLabel.Trim();
                }
            }
            return negative ? "سالب " + text : text;
        }

        private static string ArabicWhole(long number)
        {
            var parts = new List<string>();
            foreach (var scale in ArScales)
            {
                if (number >= scale.Value)
                {
                    parts.Add(ArabicScale((int)(number / scale.Value), scale));
                    number %= scale.Value;
                }
            }
            if (number > 0)
            {
                parts.Add(ArabicGroup((int)number));
            }
            return string.Join(" و", parts);
        }

        // count (1..999) of thousands, millions or billions, with the counted noun in the right form.
        private static string ArabicScale(int count, (long Value, string One, string Two, string Plural, string Accusative) scale)
        {
            if (count == 1)
            {
                return scale.One;
            }
            if (count == 2)
            {
                return scale.Two;
            }
            int lastTwo = count % 100;
            if (count > 100 && (lastTwo == 1 || lastTwo == 2))
            {
                // 501,000 => "خمسمائة ألف وألف", 602,000 => "ستمائة ألف وألفين"
                return ArabicGroup(count - lastTwo) + " " + scale.One + " و" + (lastTwo == 1 ? scale.One : scale.Two);
            }
            string noun = lastTwo >= 3 && lastTwo <= 10 ? scale.Plural
                : lastTwo >= 11 ? scale.Accusative
                : scale.One;
            return ArabicGroup(count) + " " + noun;
        }

        // 1..999
        private static string ArabicGroup(int number)
        {
            var parts = new List<string>();
            if (number >= 100)
            {
                parts.Add(ArHundreds[number / 100]);
                number %= 100;
            }
            if (number > 0)
            {
                parts.Add(ArabicBelowHundred(number));
            }
            return string.Join(" و", parts);
        }

        // 1..99
        private static string ArabicBelowHundred(int number)
        {
            if (number < 10)
            {
                return ArOnes[number];
            }
            if (number == 10)
            {
                return ArTens[1];
            }
            if (number == 11)
            {
                return "أحد عشر";
            }
            if (number == 12)
            {
                return "إثنا عشر";
            }
            if (number < 20)
            {
                return ArOnes[number % 10] + " عشر";
            }
            int ones = number % 10;
            return ones == 0 ? ArTens[number / 10] : ArOnes[ones] + " و" + ArTens[number / 10];
        }

        private static void AddIfAny(List<string> parts, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                parts.Add(value.Trim());
            }
        }
        #endregion
    }
}
