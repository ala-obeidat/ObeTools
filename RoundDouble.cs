using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObeTools
{
    /// <summary>
    /// Round double ex: if x=2.55455444; so round(x,2) =>2.55
    /// </summary>
    public static class RoundDouble
    {
        #region Variable 
        #endregion

        #region Method
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

        #endregion
    }
}
