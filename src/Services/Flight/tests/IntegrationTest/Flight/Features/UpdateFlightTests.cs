﻿using System.Threading.Tasks;
using BuildingBlocks.Contracts.EventBus.Messages;
using FluentAssertions;
using Integration.Test.Fakes;
using MassTransit;
using MassTransit.Testing;
using Xunit;

namespace Integration.Test.Flight.Features;

[Collection(nameof(TestFixture))]
public class UpdateFlightTests
{
    private readonly TestFixture _fixture;
    private readonly ITestHarness _testHarness;

    public UpdateFlightTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testHarness = _fixture.TestHarness;
    }

    [Fact]
    public async Task should_update_flight_to_db_and_publish_message_to_broker()
    {
        // Arrange
        var fakeCreateCommandFlight = new FakeCreateFlightCommand().Generate();
        var flightEntity = global::Flight.Flights.Models.Flight.Create(fakeCreateCommandFlight.Id, fakeCreateCommandFlight.FlightNumber,
            fakeCreateCommandFlight.AircraftId, fakeCreateCommandFlight.DepartureAirportId, fakeCreateCommandFlight.DepartureDate,
            fakeCreateCommandFlight.ArriveDate, fakeCreateCommandFlight.ArriveAirportId, fakeCreateCommandFlight.DurationMinutes,
            fakeCreateCommandFlight.FlightDate, fakeCreateCommandFlight.Status, fakeCreateCommandFlight.Price);
        await _fixture.InsertAsync(flightEntity);

        var command = new FakeUpdateFlightCommand(flightEntity.Id).Generate();

        // Act
        var response = await _fixture.SendAsync(command);

        // Assert
        response.Should().NotBeNull();
        response?.Id.Should().Be(flightEntity?.Id);
        response?.Price.Should().NotBe(flightEntity?.Price);
        (await _testHarness.Published.Any<Fault<FlightUpdated>>()).Should().BeFalse();
        (await _testHarness.Published.Any<FlightUpdated>()).Should().BeTrue();
    }
}