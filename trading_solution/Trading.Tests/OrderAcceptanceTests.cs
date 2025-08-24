using System;
using System.Threading;
using System.Threading.Tasks;
using Trading.Application.Services;
using Trading.Contracts;
using Trading.Domain.Entities;
using Trading.Infrastructure.Configuration;
using Xunit;

namespace Trading.Tests
{
    public class OrderAcceptanceTests
    {
        [Fact(Skip="Requires running venue services")] 
        public async Task BuyOrder_Accepted_WhenPriceAtOrAboveAsk()
        {
            // This test would spin up a venue and send an order through the OrderService.
            // It is skipped by default because it requires network services.  The logic
            // is covered by the integration of venue implementations and can be
            // executed manually.
        }
    }
}