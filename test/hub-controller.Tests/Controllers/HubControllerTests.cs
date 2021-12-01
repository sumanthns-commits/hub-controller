using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using HubController.Controllers;
using HubController.Entities;
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
    public class HubControllerTests
    {
        private readonly IHubRepository _mockHubRepo;
        private readonly HubsController _controller;
        private readonly IMapper _mapper;
        private readonly string _userId;

        public HubControllerTests()
        {
            _userId = "someUserId";
            _mockHubRepo = Substitute.For<IHubRepository>();
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMappingProfile());
            });
            _mapper = mappingConfig.CreateMapper();
            _controller = new HubsController(new HubService(_mockHubRepo, new UserService(), new PasswordService()), _mapper)
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
        public async void CreateHub_Should_Return_Hub_Created(string name, string description, string password)
        {
            // Act
            var hubId = Guid.NewGuid();
            _mockHubRepo.FindAll(_userId).Returns(new List<Hub>());
            _mockHubRepo.Create(_userId, name,
                description,
                Arg.Is<string>(
                    passwordHash => new PasswordService().VerifyPassword(passwordHash, password)))
                .Returns(Task.FromResult(
                    new Hub() {
                        Name = name,
                        HubId = hubId,
                        Description = description
                    }));
            var hubDao = new HubDAO() { Name = name, Description = description, Password = password };

            // Act
            var result = await _controller.Post(hubDao) as CreatedAtActionResult;

            // Assert
            result.StatusCode.Should().Be(201);

            var actualHubDto = result.Value as HubDTO;
            actualHubDto.Name.Should().Be(name);
            actualHubDto.Description.Should().Be(description);
            actualHubDto.HubId.Should().Be(hubId);
        }
    }
}
