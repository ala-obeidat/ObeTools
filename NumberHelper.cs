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
        #endregion

        #region Method
        /// <summary>
        /// Display number as words
        /// </summary>
        /// <param name="number">number in double format</param>
        /// <param name="type">words charectures language</param>
        /// <param name="currencyAr">Currency Label in Arabi</param>
        /// <param name="currencyEn">Currency Label in English</param>
        /// <param name="centLabelAr">Partial currency label in Arabic</param>
        /// <param name="centLabelEn">Partial currency label in English</param>
        /// <returns></returns>
        public static string NumberToWords(double number, NumberWordType type, string currencyAr, string currencyEn, string centLabelAr = "", string centLabelEn = "")
        {
            string negativeAr = string.Empty;
            string negativeEn = string.Empty;
            if (number < 0)
            {
                number = number * -1;
                negativeAr = "سالب";
                negativeEn = "Minus";
            }
            var space = " ";
            if (type == NumberWordType.All)
            {
                var convert = ConvertToWords(number.ToString(), type, centLabelEn, centLabelAr).Split("<br>");
                var englihsConvert = convert[0].Replace(" ", space)
                    .Replace("-", " ").Replace("point", ", and ")
                .Replace("Saudi Riyal", currencyEn);
                space = " و";
                var arabicConvert = convert[1]
                    .Replace("واحد#", "")
                    .Replace(" ", space)
                    .Replace("-", " ")
                    .Replace("#", " ")
                    .Replace("إثنان مئة", "مئتين")
                    .Replace("ة مئة", "مائة")
                    .Replace("يمائة", "مائة")
                    .Replace("إثنان ألف", "ألفين")
                    .Replace("إثنان مليون", "مليونين")
                    .Replace("إثنان مليار", "مليارين")
                    .Replace("ريال سعودي", currencyAr)
                    .Replace("فاصلة  و", "فاصلة ")
                    .Replace("فاصلة ", "، و");
                return $"{negativeEn} {englihsConvert}<br>{negativeAr} {arabicConvert}";

            }

            if (type == NumberWordType.Arabic)
            {
                space = " و";
                return negativeAr + " " + ConvertToWords(number.ToString(), type, centLabelEn, centLabelAr)
                    .Replace("واحد#", "")
                    .Replace(" ", space)
                    .Replace("-", " ")
                    .Replace("#", " ")
                    .Replace("إثنان مئة", "مئتين")
                    .Replace("ة مئة", "مائة")
                    .Replace("يمائة", "مائة")
                    .Replace("إثنان ألف", "ألفين")
                    .Replace("إثنان مليون", "مليونين")
                    .Replace("إثنان مليار", "مليارين")
                    .Replace("ريال سعودي", currencyAr)
                    .Replace("Saudi Riyal", currencyEn)
                    .Replace("فاصلة  و", "فاصلة ")
                    .Replace("فاصلة ", "، و")
                    .Replace("#", " ");
            }

            return negativeEn + " " + ConvertToWords(number.ToString(), type, centLabelEn, centLabelAr)
                .Replace(" ", space).Replace("-", " ").Replace("point", ", and ")
                .Replace("Saudi Riyal", currencyEn)
                .Replace("ريال سعودي", currencyAr);
        }

        /// <summary>
        /// Round double to spasific number of digits
        /// </summary>
        /// <param name="input">double number</param>
        /// <param name="digits">number of digits</param>
        /// <returns>Number after round</returns>
        public static double Round(double input, int digits)
        {
            return Math.Round(Convert.ToDouble(input), digits);
        }

        /// <summary>
        /// Round all double number in object to spasific number of digits
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="input">input object</param>
        /// <param name="digits">number of digits</param>
        public static void RoundObject<T>(T input, int digits)
        {
            IEnumerable<PropertyInfo> properties = input.GetType().GetProperties()
               .Where(x => x.PropertyType == typeof(double));

            foreach (PropertyInfo property in properties)
            {
                object dt = property.GetValue(input);

                if (dt == null)
                {
                    continue;
                }
                property.SetValue(input, Round(Convert.ToDouble(dt), digits));
            }
        }
        #endregion

        #region Helper
        private static string ConvertToWords(string numb, NumberWordType numberWordType, string centLable = "", string centLableAr = "")
        {

            string val = "", wholeNo = numb, points, andStr = "point", andStrAr = "فاصلة",
                pointStr = "", secondPointStr = "", endStr = "Saudi Riyal", endStrAr = "ريال-سعودي",
                words, words2, centAr = "هللة", cent = "Halalah";
            if (!string.IsNullOrEmpty(centLable))
            {
                cent = centLable;
            }
            if (!string.IsNullOrEmpty(centLableAr))
            {
                centAr = centLableAr;
            }
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb[..decimalPlace];
                    points = numb[(decimalPlace + 1)..];
                    if (Convert.ToInt32(points) > 0)
                    {
                        if (numberWordType != NumberWordType.English)
                        {
                            secondPointStr = ConvertWholeNumber(points, NumberWordType.Arabic);
                        }
                        pointStr = ConvertWholeNumber(points, numberWordType);
                    }
                }
                if (numb == "0")
                {
                    words = "Zero";
                    words2 = "صفر";
                }
                else
                {
                    words = ConvertWholeNumber(wholeNo, NumberWordType.English);
                    words2 = ConvertWholeNumber(wholeNo, NumberWordType.Arabic);
                }

                switch (numberWordType)
                {
                    case NumberWordType.English:
                        if (string.IsNullOrEmpty(pointStr))
                        {
                            val = $"{words} {endStr}";
                        }
                        else
                        {
                            val = $"{words} {endStr} {andStr} {pointStr} {cent}";
                        }
                        break;
                    case NumberWordType.Arabic:
                        if (string.IsNullOrEmpty(secondPointStr))
                        {
                            val = $"{words2}-{endStrAr}";
                        }
                        else
                        {
                            val = $"{words2}-{endStrAr}-{andStrAr}-{secondPointStr}-{centAr}";
                        }
                        break;
                    case NumberWordType.All:
                        string valAr = "";
                        string valEn = "";
                        if (string.IsNullOrEmpty(pointStr))
                        {
                            valAr = $"{words} {endStr}";
                        }
                        else
                        {
                            valAr = $"{words} {endStr} {andStr} {pointStr} {cent}";
                        }
                        if (string.IsNullOrEmpty(secondPointStr))
                        {
                            valEn = $"{words2}-{endStrAr}";
                        }
                        else
                        {
                            valEn = $"{words2}-{endStrAr}-{andStrAr}-{secondPointStr}-{centAr}";
                        }
                        val = $"{valAr}<br>{valEn}";
                        break;
                    default:
                        break;
                }
            }
            catch { }
            return val;
        }
        private static string ConvertDecimals(string number)
        {
            string cd = "", digit, engOne;

            for (int i = 0; i < number.Length; i++)
            {
                digit = number[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {

                    engOne = Ones(digit, NumberWordType.English);
                }
                cd += " " + engOne;
            }


            return cd;
        }
        private static string ConvertWholeNumber(string Number, NumberWordType numberWordType)
        {
            bool beginsZero = false;
            string word = "";
            try
            {

                bool isDone = false;
                double dblAmt = (Convert.ToDouble(Number));
                if (dblAmt > 0)
                {
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;
                    string place = "";
                    switch (numDigits)
                    {
                        case 1:
                            word = Ones(Number, numberWordType);
                            isDone = true;
                            break;
                        case 2:
                            word = Tens(Number, numberWordType);
                            isDone = true;
                            break;
                        case 3:
                            pos = (numDigits % 3) + 1;
                            place = numberWordType == NumberWordType.Arabic ? "#مئة " : " Hundred ";
                            break;
                        case 4:
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = numberWordType == NumberWordType.Arabic ? "#ألف " : " Thousand ";
                            break;
                        case 7:
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = numberWordType == NumberWordType.Arabic ? "#مليون " : " Million ";
                            break;
                        case 10:
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = numberWordType == NumberWordType.Arabic ? "#مليار " : " Billion ";
                            break;
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {
                        if (Number[..pos] != "0" && Number[pos..] != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number[..pos], numberWordType) + place + ConvertWholeNumber(Number[pos..], numberWordType);
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number[..pos], numberWordType) + ConvertWholeNumber(Number[pos..], numberWordType);
                        }
                    }
                    if (word.Trim().Equals(place.Trim()))
                    {
                        word = "";
                    }
                }
            }
            catch { }
            if (beginsZero)
            {
                return numberWordType == NumberWordType.Arabic ? $"صفر-{word.Trim()}" : $"Zero {word.Trim()}";
            }
            return word.Trim();
        }
        private static string Ones(string Number, NumberWordType numberWordType)
        {
            int _Number = Convert.ToInt32(Number);
            string name = "";
            switch (_Number)
            {
                case 1:
                    name = numberWordType == NumberWordType.Arabic ? "واحد" : "One";
                    break;
                case 2:
                    name = numberWordType == NumberWordType.Arabic ? "إثنان" : "Two";
                    break;
                case 3:
                    name = numberWordType == NumberWordType.Arabic ? "ثلاثة" : "Three";
                    break;
                case 4:
                    name = numberWordType == NumberWordType.Arabic ? "أربعة" : "Four";
                    break;
                case 5:
                    name = numberWordType == NumberWordType.Arabic ? "خمسة" : "Five";
                    break;
                case 6:
                    name = numberWordType == NumberWordType.Arabic ? "ستة" : "Six";
                    break;
                case 7:
                    name = numberWordType == NumberWordType.Arabic ? "سبعة" : "Seven";
                    break;
                case 8:
                    name = numberWordType == NumberWordType.Arabic ? "ثمانية" : "Eight";
                    break;
                case 9:
                    name = numberWordType == NumberWordType.Arabic ? "تسعة" : "Nine";
                    break;
            }
            return name;
        }
        private static string Tens(string Number, NumberWordType numberWordType)
        {
            int _Number = Convert.ToInt32(Number);
            string name = null;
            switch (_Number)
            {
                case 10:
                    name = numberWordType == NumberWordType.Arabic ? "عشرة" : "Ten";
                    break;
                case 11:
                    name = numberWordType == NumberWordType.Arabic ? "إحدى-عشر" : "Eleven";
                    break;
                case 12:
                    name = numberWordType == NumberWordType.Arabic ? "إثنا-عشر" : "Twelve";
                    break;
                case 13:
                    name = numberWordType == NumberWordType.Arabic ? "ثلاثة-عشر" : "Thirteen";
                    break;
                case 14:
                    name = numberWordType == NumberWordType.Arabic ? "أربعة-عشر" : "Fourteen";
                    break;
                case 15:
                    name = numberWordType == NumberWordType.Arabic ? "خمسة-عشر" : "Fifteen";
                    break;
                case 16:
                    name = numberWordType == NumberWordType.Arabic ? "ستة-عشر" : "Sixteen";
                    break;
                case 17:
                    name = numberWordType == NumberWordType.Arabic ? "سبعة-عشر" : "Seventeen";
                    break;
                case 18:
                    name = numberWordType == NumberWordType.Arabic ? "ثمانية-عشر" : "Eighteen";
                    break;
                case 19:
                    name = numberWordType == NumberWordType.Arabic ? "تسعة-عشر" : "Nineteen";
                    break;
                case 20:
                    name = numberWordType == NumberWordType.Arabic ? "عشرون" : "Twenty";
                    break;
                case 30:
                    name = numberWordType == NumberWordType.Arabic ? "ثلاثون" : "Thirty";
                    break;
                case 40:
                    name = numberWordType == NumberWordType.Arabic ? "أربعون" : "Fourty";
                    break;
                case 50:
                    name = numberWordType == NumberWordType.Arabic ? "خمسون" : "Fifty";
                    break;
                case 60:
                    name = numberWordType == NumberWordType.Arabic ? "ستون" : "Sixty";
                    break;
                case 70:
                    name = numberWordType == NumberWordType.Arabic ? "سبعون" : "Seventy";
                    break;
                case 80:
                    name = numberWordType == NumberWordType.Arabic ? "ثمانون" : "Eighty";
                    break;
                case 90:
                    name = numberWordType == NumberWordType.Arabic ? "تسعون" : "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        if (numberWordType == NumberWordType.Arabic)
                        {
                            name = Ones(Number[1..], numberWordType) + " " + Tens(Number[..1] + "0", numberWordType);
                        }
                        else
                        {
                            name = Tens(Number[..1] + "0", numberWordType) + " " + Ones(Number[1..], numberWordType);
                        }

                    }
                    break;
            }
            return name;
        }
        #endregion
    }
}
