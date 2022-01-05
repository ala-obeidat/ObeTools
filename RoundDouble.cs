using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObeTools
{
    public static class RoundDouble
    {
        #region Variable 
        #endregion

        #region Method
        public static double Round(double input, int digits)
        {
            return Math.Round(Convert.ToDouble(input), digits);
        }
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
