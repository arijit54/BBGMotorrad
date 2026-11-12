
using System.Net;
using System.Net.Http.Json;
using BBG.RideService.Api.Contracts.Rides;
using BBG.RideService.Domain.Enums;
using BBG.RideService.Application.Features.Rides.CreateRide;
using System.Text.Json;
using System.Text.Json.Serialization;
using BBG.RideService.Application.Abstractions.Persistence;
using Microsoft.Extensions.DependencyInjection;
using BBG.RideService.Application.Features.Rides.UpdateRide;

namespace BBG.RideService.IntegrationTests.Api;

public sealed class RideApiTests : IAsyncLifetime
{
    private RideApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new RideApiFactory();
        await _factory.InitializeDatabaseAsync();

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CreateRide_WithValidRequest_ReturnsCreated()
    {
        var request = CreateValidRequest();

        var response = await _client.PostAsJsonAsync(
            "/api/rides",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var result = await response.Content.ReadFromJsonAsync<CreateRideResult>(
            jsonOptions);

        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(RideStatus.Draft, result.Status);
    }

    [Fact]
    public async Task GetRide_WithNonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/rides/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateRide_WithInvalidSchedule_ReturnsBadRequest()
    {
        var request = CreateValidRequest() with
        {
            StartDate = new DateTimeOffset(2027, 1, 20, 10, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2027, 1, 19, 10, 0, 0, TimeSpan.Zero)
        };

        var response = await _client.PostAsJsonAsync(
            "/api/rides",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateRideRequest CreateValidRequest()
    {
        return new CreateRideRequest
        {
            Title = "Integration Test Ride",
            Description = "Ride created through an API integration test.",
            Type = RideType.WeekendRide,
            StartLocationName = "Kolkata",
            StartLocationAddress = "Kolkata, West Bengal",
            DestinationName = "Charkhole",
            DestinationAddress = "Charkhole, West Bengal",
            StartDate = new DateTimeOffset(2027, 1, 20, 10, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2027, 1, 22, 10, 0, 0, TimeSpan.Zero),
            MaximumCapacity = 15,
            CapacityPolicy = RideCapacityPolicy.Waitlist
        };
    }


    [Fact]
    public async Task UpdateRide_WithDraftRide_ReturnsUpdatedRide()
    {
        var createdRide = await CreateRideAsync();

        var request = CreateValidUpdateRequest() with
        {
            Title = "Updated Eastern Himalayan Trail",
            MaximumCapacity = 20
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/rides/{createdRide.Id}",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UpdateRideResult>(
            CreateJsonOptions());

        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(20, result.MaximumCapacity);
        Assert.Equal(RideStatus.Draft, result.Status);
    }

    [Fact]
    public async Task UpdateRide_WithPublishedRide_ReturnsUpdatedRide()
    {
        var createdRide = await CreateRideAsync();

        await PublishRideAsync(createdRide.Id);

        var request = CreateValidUpdateRequest() with
        {
            Title = "Updated Published Ride"
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/rides/{createdRide.Id}",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UpdateRideResult>(
            CreateJsonOptions());

        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(RideStatus.Published, result.Status);
    }

    [Fact]
    public async Task UpdateRide_WithNonExistingId_ReturnsNotFound()
    {
        var request = CreateValidUpdateRequest();

        var response = await _client.PutAsJsonAsync(
            $"/api/rides/{Guid.NewGuid()}",
            request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRide_WhenCapacityIsReducedBelowReservations_PreservesReservations()
    {
        var createdRide = await CreateRideAsync();

        await PublishRideAsync(createdRide.Id);

        using (var scope = _factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IRideRepository>();

            // Simulate two existing reservations.
            Assert.True(await repository.TryReserveCapacityAsync(createdRide.Id));
            Assert.True(await repository.TryReserveCapacityAsync(createdRide.Id));
        }

        var request = CreateValidUpdateRequest() with
        {
            MaximumCapacity = 1
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/rides/{createdRide.Id}",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UpdateRideResult>(
            CreateJsonOptions());

        Assert.NotNull(result);
        Assert.Equal(1, result.MaximumCapacity);
        Assert.Equal(2, result.ActiveReservations);
        Assert.Equal(0, result.AvailableCapacity);
        Assert.Equal(RideStatus.Published, result.Status);
    }

    private async Task<CreateRideResult> CreateRideAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/rides",
            CreateValidRequest());

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateRideResult>(
            CreateJsonOptions());

        Assert.NotNull(result);
        return result;
    }

    private async Task PublishRideAsync(Guid rideId)
    {
        using var scope = _factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IRideRepository>();

        var ride = await repository.GetByIdAsync(rideId);

        Assert.NotNull(ride);

        ride.Publish(DateTimeOffset.UtcNow);

        await repository.UpdateAsync(ride);
    }

    private static UpdateRideRequest CreateValidUpdateRequest()
    {
        return new UpdateRideRequest
        {
            Title = "Updated Integration Test Ride",
            Description = "Updated through an API integration test.",
            Type = RideType.WeekendRide,
            StartLocationName = "Kolkata",
            StartLocationAddress = "Kolkata, West Bengal",
            DestinationName = "Charkhole",
            DestinationAddress = "Charkhole, West Bengal",
            StartDate = new DateTimeOffset(2027, 1, 20, 10, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2027, 1, 22, 10, 0, 0, TimeSpan.Zero),
            MaximumCapacity = 15,
            CapacityPolicy = RideCapacityPolicy.Waitlist
        };
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
    [Fact]
    public async Task UpdateRide_WithInProgressRide_ReturnsBadRequest()
    {
        var createdRide = await CreateRideAsync();
    
        using (var scope = _factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IRideRepository>();
    
            var ride = await repository.GetByIdAsync(createdRide.Id);
    
            Assert.NotNull(ride);
    
            ride.Publish(DateTimeOffset.UtcNow);
            ride.Start();
    
            await repository.UpdateAsync(ride);
        }
    
        var request = CreateValidUpdateRequest() with
        {
            Title = "Should Not Be Updated"
        };
    
        var response = await _client.PutAsJsonAsync(
            $"/api/rides/{createdRide.Id}",
            request);
    
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

}
