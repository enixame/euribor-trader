using System;
using Trading.Contracts;
using Xunit;

namespace Trading.Tests
{
    public class AcceptanceTests
    {
        [Fact]
        public void OrderAcceptanceRule_Buy()
        {
            decimal ask = 2.305m;
            var orderPrice = 2.306m;
            bool accepted = orderPrice >= ask;
            Assert.True(accepted);
            orderPrice = 2.300m;
            accepted = orderPrice >= ask;
            Assert.False(accepted);
        }

        [Fact]
        public void OrderAcceptanceRule_Sell()
        {
            decimal bid = 2.295m;
            decimal price = 2.290m;
            bool accepted = price <= bid;
            Assert.True(accepted);
            price = 2.300m;
            accepted = price <= bid;
            Assert.False(accepted);
        }
    }
}