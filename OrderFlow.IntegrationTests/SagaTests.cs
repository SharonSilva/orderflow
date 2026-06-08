using System.Net.Http.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Xunit;

namespace OrderFlow.IntegrationTests;

public class SagaTests
{
    [Fact]
    public async Task Happy_Path_Order_Ends_Confirmed_And_Stock_Decreases()
    {
        // 1. Build the test host using the AppHost project.
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.OrderFlow_AppHost>();

        // 2. Start everything (containers + services).
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // 3. Wait until each service is reporting healthy.
        // CancellationToken with a 2-minute timeout - shared across all three waits.
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        await app.ResourceNotifications.WaitForResourceHealthyAsync("orders", cts.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("inventory", cts.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("payments", cts.Token);

        // 4. Get HTTP clients for the services we will talk to.
        var ordersClient = app.CreateHttpClient("orders");
        var inventoryClient = app.CreateHttpClient("inventory");

        // 5. Capture stock BEFORE placing the order.
        var stockBefore = await GetFirstStockItem(inventoryClient);
        var availableBefore = stockBefore.AvailableQuantity;
        var reservedBefore = stockBefore.ReservedQuantity;

        // 6. POST an order with a non-whole amount (payment should succeed).
        var orderRequest = new
        {
            customerId = "test-cust",
            productId = "prod-1",
            quantity = 2,
            amount = 29.99m
        };

        var postResponse = await ordersClient.PostAsJsonAsync("/orders", orderRequest);
        postResponse.EnsureSuccessStatusCode();

        var created = await postResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(created);
        Assert.Equal(0, created!.Status); // Pending immediately after POST

        // 7. Poll until the order becomes Confirmed (status = 1) or timeout.
        var confirmedOrder = await WaitForStatus(
            ordersClient,
            created.Id,
            expectedStatus: 1,
            timeout: TimeSpan.FromSeconds(30));

        Assert.Equal(1, confirmedOrder.Status); // 1 = Confirmed

        // 8. Stock should have moved: available down by 2, reserved up by 2.
        var stockAfter = await GetFirstStockItem(inventoryClient);
        Assert.Equal(availableBefore - 2, stockAfter.AvailableQuantity);
        Assert.Equal(reservedBefore + 2, stockAfter.ReservedQuantity);
    }

    private static async Task<StockItemDto> GetFirstStockItem(HttpClient client)
    {
        var stocks = await client.GetFromJsonAsync<StockItemDto[]>("/stock");
        Assert.NotNull(stocks);
        Assert.NotEmpty(stocks!);
        return stocks![0];
    }

    private static async Task<OrderDto> WaitForStatus(
        HttpClient client,
        Guid orderId,
        int expectedStatus,
        TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var orders = await client.GetFromJsonAsync<OrderDto[]>("/orders");
            var match = orders?.FirstOrDefault(o => o.Id == orderId);
            if (match is not null && match.Status == expectedStatus)
            {
                return match;
            }
            await Task.Delay(500);
        }
        throw new TimeoutException(
            $"Order {orderId} did not reach status {expectedStatus} within {timeout}.");
    }

    private record OrderDto(
        Guid Id,
        string CustomerId,
        string ProductId,
        int Quantity,
        decimal Amount,
        int Status,
        DateTime CreatedAt);

    private record StockItemDto(
        Guid Id,
        string ProductId,
        int AvailableQuantity,
        int ReservedQuantity);
}

