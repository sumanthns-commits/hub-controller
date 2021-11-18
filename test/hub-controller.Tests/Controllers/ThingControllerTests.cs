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
        private readonly IThingRepository _mockThingRepo;
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
            _mockThingRepo = Substitute.For<IThingRepository>();
            _mockThingIdGenerator = Substitute.For<IThingIdGenerator>();
            _mockThingIdGenerator.Generate().Returns(_thingId);

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMappingProfile());
            });
            _mapper = mappingConfig.CreateMapper();
            _controller = new ThingsController(
                new ThingService(_mockThingRepo, _mockThingIdGenerator,
                    new HubService(_mockHubRepo, new UserService())),
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
            var createdAt = DateTime.UtcNow;
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(Hub.Create(_userId, hubName)));
            _mockThingRepo.FindAll(hubId).Returns(Task.FromResult(new List<Thing>()));
            _mockThingRepo.Create(hubId, name, description, _thingId).Returns(
                Task.FromResult(new Thing()
                {
                    Name = name,
                    HubId = $"hub_thing_{hubId}",
                    ThingId = _thingId,
                    Description = description,
                    CreatedAt = createdAt,
                    Status = "off"
                }));
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
            actualThingDto.CreatedAt.Should().Be(createdAt);
        }

        [Theory]
        [InlineAutoData]
        public async void CreateThing_Should_Throw_Error_When_Hub_NotExists(string name, string description)
        {
            // Act
            var hubId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            _mockThingRepo.Create(hubId, name, description, _thingId).Returns(
                Task.FromResult(new Thing()
                {
                    Name = name,
                    HubId = $"hub_thing_{hubId}",
                    ThingId = _thingId,
                    Description = description,
                    CreatedAt = createdAt,
                    Status = "off"
                }));
            var thingDao = new ThingDAO() { Name = name, Description = description };

            // Act
            Func<Task> action = () => _controller.Post(hubId, thingDao);

            // Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(action);
            exception.Message.Should().Be($"Hub {hubId} not found.");
        }

        [Theory]
        [InlineAutoData]
        public async void CreateThing_Should_Thing_With_SameName_IfExists_Already(string name, string description, string hubName)
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
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(Hub.Create(_userId, hubName)));
            _mockThingRepo.FindAll(hubId).Returns(
                Task.FromResult(new List<Thing>() { existingThing }));
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
            // Act
            Environment.SetEnvironmentVariable("THINGS_ALLOWED_PER_HUB", "1");
            var hubId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            _mockHubRepo.Find(_userId, hubId).Returns(Task.FromResult(Hub.Create(_userId, hubName)));
            _mockThingRepo.FindAll(hubId).Returns(
                Task.FromResult(new List<Thing>() { Thing.Create(hubId, anotherThingName, description, _thingId) }));
            var thingDao = new ThingDAO() { Name = name, Description = description };

            // Act
            Func<Task> action = () => _controller.Post(hubId, thingDao);

            // Assert
            var exception = await Assert.ThrowsAsync<LimitExceededException>(action);
            exception.Message.Should().Be($"Cannot create more things for Hub {hubId}. Limit reached.");
        }
    }
}
