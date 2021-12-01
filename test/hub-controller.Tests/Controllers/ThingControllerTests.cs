using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using HubController.Controllers;
using HubController.Entities;
using HubController.Exceptions;
using HubController.Models.DAO;
using HubController.Models.DTO;
using HubController.Repositories;
using HubController.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace HubController.Tests.Controllers
{
    public class ThingControllerTests
    {
        private readonly IHubRepository _mockHubRepo;
        private readonly ThingsController _controller;
        private readonly IThingIdGenerator _mockThingIdGenerator;
        private readonly IMapper _mapper;
        private readonly string _userId;
        private readonly string _thingId;

        public ThingControllerTests()
        {
            _userId = "someUserId";
            _thingId = "someThingId";
            _mockHubRepo = Substitute.For<IHubRepository>();
            _mockThingIdGenerator = Substitute.For<IThingIdGenerator>();
            _mockThingIdGenerator.Generate().Returns(_thingId);

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMappingProfile());
            });
            _mapper = mappingConfig.CreateMapper();
            _controller = new ThingsController(
                new ThingService(_mockThingIdGenerator,
                    new HubService(_mockHubRepo, new UserService(), new PasswordService())),
                _mapper)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim("sub", _userId)
                    })),
                    }
                }
            };
        }

        [Theory]
        [InlineAutoData]
        public async void CreateThing_Should_Return_Thing_Created(string name, string description, string hubName)
        {
            // Act
            var hubId = Guid.NewGuid();
            var hub = Hub.Create(_userId, hubName, description);
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));
            _mockHubRepo.Save(Arg.Is<Hub>(h => h.Things.Any(t => t.ThingId == _thingId))).Returns(Task.CompletedTask);
            var thingDao = new ThingDAO() { Name = name, Description = description };

            // Act
            var result = await _controller.Post(hubId, thingDao) as CreatedAtActionResult;

            // Assert
            result.StatusCode.Should().Be(201);

            var actualThingDto = result.Value as ThingDTO;
            actualThingDto.Name.Should().Be(name);
            actualThingDto.Description.Should().Be(description);
            actualThingDto.ThingId.Should().Be(_thingId);
            actualThingDto.Status.Should().Be("off");
        }

        [Theory]
        [InlineAutoData]
        public async void CreateThing_Should_Throw_Error_When_Hub_NotExists(string name, string description)
        {
            // Act
            var hubId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var thingDao = new ThingDAO() { Name = name, Description = description };

            // Act
            Func<Task> action = () => _controller.Post(hubId, thingDao);

            // Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
            exception.Message.Should().Be($"Hub {hubId} not found.");
        }

        [Theory]
        [InlineAutoData]
        public async void CreateThing_Should_Return_Thing_With_SameName_IfExists_Already(string name, string description, string hubName)
        {
            // Act
            var hubId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddDays(-10);
            var existingThing = new Thing()
            {
                Name = name,
                HubId = $"hub_thing_{hubId}",
                ThingId = _thingId,
                Description = "existing description",
                CreatedAt = createdAt,
                Status = "off"
            };
            var hub = Hub.Create(_userId, hubName, description);
            hub.Things = new List<Thing>() { existingThing };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));
            
            var thingDao = new ThingDAO() { Name = name, Description = description };

            // Act
            var result = await _controller.Post(hubId, thingDao) as CreatedAtActionResult;

            // Assert
            result.StatusCode.Should().Be(201);

            var actualThingDto = result.Value as ThingDTO;
            actualThingDto.Name.Should().Be(name);
            actualThingDto.Description.Should().Be(existingThing.Description);
            actualThingDto.ThingId.Should().Be(existingThing.ThingId);
            actualThingDto.Status.Should().Be(existingThing.Status);
            actualThingDto.CreatedAt.Should().Be(existingThing.CreatedAt);
        }

        [Theory]
        [InlineAutoData]
        public async void CreateThing_Should_Throw_Error_When_Limit_Exceeded(string name, string description, string hubName, string anotherThingName)
        {
            // Arrange
            Environment.SetEnvironmentVariable("THINGS_ALLOWED_PER_HUB", "1");
            var hubId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var hub = Hub.Create(_userId, hubName, description);
            hub.Things = new List<Thing>() { Thing.Create(anotherThingName, description, _thingId) };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));

            var thingDao = new ThingDAO() { Name = name, Description = description };

            // Act
            Func<Task> action = () => _controller.Post(hubId, thingDao);

            // Assert
            var exception = await Assert.ThrowsAsync<LimitExceededException>(action);
            exception.Message.Should().Be($"Cannot create more things for Hub {hubId}. Limit reached.");
        }

        [Theory]
        [InlineAutoData]
        public async void GetThing_Should_Throw_Exception_If_Hub_Not_Found(Guid hubId, string thingId)
        {
            // Arrange
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult<Hub>(null));

            // Act
            Task action() => _controller.Get(hubId, thingId);

            // Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
            exception.Message.Should().Be($"Thing {thingId} not found.");
        }

        [Theory]
        [InlineAutoData]
        public async void DeleteThing_Should_Work(Guid hubId, string thingId, string hubName, string description, string thingName)
        {
            // Arrange

            var hub = Hub.Create(_userId, hubName, description);
            hub.Things = new List<Thing>() { Thing.Create(thingName, description, thingId) };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));

            // Act
            var result = await _controller.Delete(hubId, thingId) as NoContentResult;

            // Assert
            result.StatusCode.Should().Be(204);
            await _mockHubRepo.Received(1).Save(Arg.Is<Hub>(h => h.Things.Count == 0));
        }

        [Theory]
        [InlineAutoData]
        public async void PatchThing_Should_Throw_Error_For_Empty_Thing_Status(string description, string hubName, string anotherThingName, string thingId, Guid hubId)
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var hub = Hub.Create(_userId, hubName, description);
            hub.Things = new List<Thing>() { Thing.Create(anotherThingName, description, thingId) };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));

            var thingStatusDao = new ThingStatusDAO();

            // Act
            Func<Task> action = () => _controller.Patch(hubId, thingId, thingStatusDao);

            // Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(action);
            exception.Message.Should().Be($"Thing status should be off|on");
        }

        [Theory]
        [InlineAutoData]
        public async void PatchThing_Should_Throw_Error_For_Invalid_Thing_Status(string status, string description, string hubName, string anotherThingName, string thingId, Guid hubId)
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var hub = Hub.Create(_userId, hubName, description);
            hub.Things = new List<Thing>() { Thing.Create(anotherThingName, description, thingId) };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));

            var thingStatusDao = new ThingStatusDAO() { Status = status };

            // Act
            Func<Task> action = () => _controller.Patch(hubId, thingId, thingStatusDao);

            // Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(action);
            exception.Message.Should().Be($"Thing status should be off|on");
        }

        [Theory]
        [InlineAutoData]
        public async void PatchThing_Should_Throw_Error_When_HubId_Is_Not_Found(string description, string hubName, string anotherThingName, string thingId, Guid hubId, Guid nonExistingHubId)
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var hub = Hub.Create(_userId, hubName, description);
            hub.Things = new List<Thing>() { Thing.Create(anotherThingName, description, thingId) };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));

            var thingStatusDao = new ThingStatusDAO() { Status = "off" };

            // Act
            Func<Task> action = () => _controller.Patch(nonExistingHubId, thingId, thingStatusDao);

            // Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
            exception.Message.Should().Be($"Thing {thingId} not found.");
        }

        [Theory]
        [InlineAutoData]
        public async void PatchThing_Should_Throw_Error_When_Thing_Not_Found(string description, string hubName, string anotherThingName, string thingId, Guid hubId, string nonExistingThingId)
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var hub = Hub.Create(_userId, hubName, description);
            hub.Things = new List<Thing>() { Thing.Create(anotherThingName, description, thingId) };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));

            var thingStatusDao = new ThingStatusDAO() { Status = "off" };

            // Act
            Func<Task> action = () => _controller.Patch(hubId, nonExistingThingId, thingStatusDao);

            // Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
            exception.Message.Should().Be($"Thing {nonExistingThingId} not found.");
        }

        [Theory]
        [InlineAutoData]
        public async void PatchThing_Should_update_thing_status(string description, string hubName, string anotherThingName, string thingId, Guid hubId, string anotherThingId, string thingName)
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var hub = Hub.Create(_userId, hubName, description);
            hub.Things = new List<Thing>() { 
                Thing.Create(thingName, description, thingId),
                Thing.Create(anotherThingName, description, anotherThingId) };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));

            var thingStatusDao = new ThingStatusDAO() { Status = Constants.THING_ON };

            // Act
           var result = await _controller.Patch(hubId, thingId, thingStatusDao) as OkObjectResult;

            // Assert
            result.StatusCode.Should().Be(200);
            var actualThing = result.Value as ThingDTO;
            actualThing.ThingId.Should().Be(thingId);
            actualThing.Status.Should().Be(Constants.THING_ON);
            await _mockHubRepo.Received(1).Save(Arg.Is<Hub>(h => h.Things.Any(t => t.ThingId == thingId && t.Status == Constants.THING_ON)));
        }

        [Theory]
        [InlineAutoData]
        public async void PatchThing_Should_not_update_thing_if_status_not_changed(string description, string hubName, string anotherThingName, string thingId, Guid hubId, string anotherThingId, string thingName)
        {
            // Arrange
            var createdAt = DateTime.UtcNow;
            var hub = Hub.Create(_userId, hubName, description);
            Thing thing = Thing.Create(thingName, description, thingId);
            thing.Status = Constants.THING_ON;
            hub.Things = new List<Thing>() {
                thing,
                Thing.Create(anotherThingName, description, anotherThingId) };
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(hub));

            var thingStatusDao = new ThingStatusDAO() { Status = Constants.THING_ON };

            // Act
            var result = await _controller.Patch(hubId, thingId, thingStatusDao) as OkObjectResult;

            // Assert
            result.StatusCode.Should().Be(200);
            var actualThing = result.Value as ThingDTO;
            actualThing.ThingId.Should().Be(thingId);
            actualThing.Status.Should().Be(Constants.THING_ON);
            await _mockHubRepo.DidNotReceive().Save(Arg.Any<Hub>());
        }
    }
}
