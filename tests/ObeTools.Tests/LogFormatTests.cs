using System;

using ObeTools.Model;

using Xunit;

namespace ObeTools.Tests
{
    /// <summary>
    /// The log line is parsed by scanners and runbooks: pin it byte for byte.
    /// </summary>
    public class LogFormatTests
    {
        [Fact]
        public void LogModel_ToString_IsTheGoldenFormat()
        {
            var model = new LogModel(new DateTime(2026, 10, 9, 13, 5, 9, 70))
            {
                Caption = "Order failed",
                Data = "Timeout",
                Method = "Checkout",
                FileName = "Back => Api => OrdersController.cs",
                LineNumber = 42
            };

            Assert.Equal(
                "\nStringTime: 13:05:09:07 |  Caption: Order failed |  Message: Timeout |  Method: Checkout |  FileName: Back => Api => OrdersController.cs |  LineNumber: 42",
                model.ToString());
        }

        [Fact]
        public void LogModel_ToString_WithStackTrace_AppendsAtTheEnd()
        {
            var model = new LogModel(new DateTime(2026, 10, 9, 0, 0, 0)) { StackTrace = "at X()" };

            Assert.EndsWith(" |  LineNumber: 0 |  StackTrace: at X()", model.ToString());
        }

        [Fact]
        public void StringTime_IsCultureInvariant()
        {
            var previous = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fi-FI"); // '.' time separator
                Assert.Equal("23:59:58:99", new LogModel(new DateTime(2026, 1, 1, 23, 59, 58, 990)).StringTime);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = previous;
            }
        }
    }
}
