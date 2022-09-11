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
        /// <returns></returns>
        public static string NumberToWords(double number, NumberWordType type)
        {
            var space = " ";
            if (type != NumberWordType.English)
            {
                space = " و";
                return ConvertToWords(number.ToString(), type)
                    .Replace("واحد#", "")
                    .Replace(" ", space)
                    .Replace("-", " ")
                    .Replace("#", " ")
                    .Replace("إثنان مئة", "مئتين")
                    .Replace("إثنان ألف", "ألفين")
                    .Replace("إثنان مليون", "مليونين")
                    .Replace("إثنان مليار", "مليارين")
                    .Replace("فاصلة ريال سعودي", "ريال سعودي")
                    .Replace("point Saudi Riyal", "point Saudi Riyal");
            }

            return ConvertToWords(number.ToString(), type)
                .Replace(" ", space).Replace("-", " ")
                .Replace("point Saudi Riyal", "point Saudi Riyal");
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
        private static string ConvertToWords(string numb, NumberWordType numberWordType)
        {
            string val = "", wholeNo = numb, points, andStr = "point", andStrAr = "فاصلة", pointStr = "", secondPointStr = "", endStr = "Saudi Riyal", endStrAr = "ريال-سعودي";
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb[..decimalPlace];
                    points = numb[(decimalPlace + 1)..];
                    if (Convert.ToInt32(points) > 0)
                    {
                        if (numberWordType == NumberWordType.All)
                        {
                            pointStr = ConvertDecimals(points, NumberWordType.English);
                            secondPointStr = ConvertDecimals(points, NumberWordType.Arabic);
                        }
                        else
                        {
                            pointStr = ConvertDecimals(points, numberWordType);
                        }
                    }
                }
                switch (numberWordType)
                {
                    case NumberWordType.English:
                        val = $"{ConvertWholeNumber(wholeNo, NumberWordType.English)} {andStr}{pointStr} {endStr}";
                        break;
                    case NumberWordType.Arabic:
                        val = $"{ConvertWholeNumber(wholeNo, NumberWordType.Arabic)}-{andStrAr}-{pointStr}-{endStrAr}";
                        break;
                    case NumberWordType.All:
                        val = $"{ConvertWholeNumber(wholeNo, NumberWordType.English)} {andStr}{pointStr} {endStr} <br> {ConvertWholeNumber(wholeNo, NumberWordType.Arabic)}-{andStrAr}-{secondPointStr}-{endStrAr}";
                        break;
                    default:
                        break;
                }
            }
            catch { }
            return val;
        }
        private static string ConvertDecimals(string number, NumberWordType numberWordType)
        {
            string cd = "", digit, engOne;
            if (numberWordType == NumberWordType.Arabic)
            {
                return ConvertWholeNumber(number, numberWordType);
            }
            else
            {
                for (int i = 0; i < number.Length; i++)
                {
                    digit = number[i].ToString();
                    if (digit.Equals("0"))
                    {
                        engOne = "Zero";
                    }
                    else
                    {
                        engOne = Ones(digit, numberWordType);
                    }
                    cd += " " + engOne;
                }
            }

            return cd;
        }
        private static string ConvertWholeNumber(string Number, NumberWordType numberWordType)
        {
            string word = "";
            try
            {
                bool beginsZero = false;
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
                    name = numberWordType == NumberWordType.Arabic ? "أحدى عشر" : "Eleven";
                    break;
                case 12:
                    name = numberWordType == NumberWordType.Arabic ? "إثنا عشر" : "Twelve";
                    break;
                case 13:
                    name = numberWordType == NumberWordType.Arabic ? "ثلاثة عشر" : "Thirteen";
                    break;
                case 14:
                    name = numberWordType == NumberWordType.Arabic ? "أربعة عشر" : "Fourteen";
                    break;
                case 15:
                    name = numberWordType == NumberWordType.Arabic ? "خمسة عشر" : "Fifteen";
                    break;
                case 16:
                    name = numberWordType == NumberWordType.Arabic ? "ستة عشر" : "Sixteen";
                    break;
                case 17:
                    name = numberWordType == NumberWordType.Arabic ? "سبعة عشر" : "Seventeen";
                    break;
                case 18:
                    name = numberWordType == NumberWordType.Arabic ? "ثمانية عشر" : "Eighteen";
                    break;
                case 19:
                    name = numberWordType == NumberWordType.Arabic ? "تسعة عشر" : "Nineteen";
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
