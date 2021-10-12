using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Options;
using RestAPI.Services;
using RestAPI.UnitTests.Attributes;
using Xunit;

namespace RestAPI.UnitTests.Services
{

    public class ApikeyService_Should
    {
        //-------------------------CreateApiKey------------------------------------
        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_UserIsNull(
            string username,
            string password,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {

            //Arrange 
            userRepositoryMock
                .Setup(mock => mock.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserReadModel)null); // when GetAsync method will be called with any string, we want for the method to return null UserReadModel

            // Act & Assert
            var result = await sut
                .Invoking(sut => sut.CreateApiKey(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"User with Username: '{username}' does not exists!");

            result.Which.StatusCode.Should().Be(404);

            // Assert 
            userRepositoryMock
                .Verify(mock => mock
                .GetAsync(It.Is<string>(value => value.Equals(username))), Times.Once);

        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_WrongPassword(
            string username,
            string password,
            UserReadModel userReadModel,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {
            //Arrange 
            userRepositoryMock
                .Setup(mock => mock.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(userReadModel); // when GetAsync method will be called with any string, we want for the method to return UserReadModel

            // Act & Assert
            var result = await sut
                .Invoking(sut => sut.CreateApiKey(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Wrong password for user: '{userReadModel.Username}'");

            result.Which.StatusCode.Should().Be(400);

            // Assert 
            userRepositoryMock
                .Verify(mock => mock
                .GetAsync(It.Is<string>(value => value.Equals(username))), Times.Once);

        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_ReturnsBadHttpException_When_ApiKeyLimit_Is_Reached(
            UserReadModel userReadModel,
            ApiKeySettings apiKeySettings,
            IEnumerable<ApiKeyReadModel> apiKeys,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            [Frozen] Mock<IApiKeysRepository> apiKeyRepositoryMock,
            [Frozen] Mock<IOptions<ApiKeySettings>> apiKeySettingsMock,
            Fixture fixture) // required due to bug with AutoMoq
        {
            //Arrange 

            userRepositoryMock
                .Setup(mock => mock.GetAsync(userReadModel.Username))
                .ReturnsAsync(userReadModel);

            apiKeyRepositoryMock
                .Setup(mock => mock.GetByUserIdAsync(userReadModel.Id))
                .ReturnsAsync(apiKeys); // when GetByUserIdAsync method will be called with ID from 'userReadModel', we want for the method to return IEnumerable<ApiKeyReadModel> (of any size, most probably 3)

            apiKeySettings.ApiKeyLimit = apiKeys.Count();

            apiKeySettingsMock
                .Setup(mock => mock.Value)
                .Returns(apiKeySettings);

            // Act & Assert

            var sut = fixture.Create<ApikeyService>();

            var result = await sut
                .Invoking(sut => sut.CreateApiKey(userReadModel.Username, userReadModel.Password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Api key limit is reached");

            result.Which.StatusCode.Should().Be(400);

            // Assert 
            userRepositoryMock
                .Verify(mock => mock
                .GetAsync(It.IsAny<string>()), Times.Once);

            apiKeyRepositoryMock
                .Verify(mock => mock
                .GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public async Task CreateApiKey_When_AllChecks_Pass(
                UserReadModel userReadModel,
                ApiKeySettings apiKeySettings,
                IEnumerable<ApiKeyReadModel> apiKeys,
                [Frozen] Mock<IUserRepository> userRepositoryMock,
                [Frozen] Mock<IApiKeysRepository> apiKeyRepositoryMock,
                [Frozen] Mock<IOptions<ApiKeySettings>> apiKeySettingsMock,
                Fixture fixture) // required due to bug with AutoMoq
        {
            //Arrange 

            userRepositoryMock
                .Setup(mock => mock.GetAsync(userReadModel.Username))
                .ReturnsAsync(userReadModel);

            apiKeyRepositoryMock
                .Setup(mock => mock.GetByUserIdAsync(userReadModel.Id))
                .ReturnsAsync(apiKeys); // when GetByUserIdAsync method will be called with ID from 'userReadModel', we want for the method to return IEnumerable<ApiKeyReadModel> (of any size, most probably 3)

            apiKeySettings.ApiKeyLimit = apiKeys.Count() + 1;

            apiKeySettingsMock
                .Setup(mock => mock.Value)
                .Returns(apiKeySettings);

            // Act & Assert

            var sut = fixture.Create<ApikeyService>(); // required due to bug with AutoMoq

            var result = await sut.CreateApiKey(userReadModel.Username, userReadModel.Password);

            // Assert 
            userRepositoryMock
                .Verify(mock => mock
                .GetAsync(It.IsAny<string>()), Times.Once);

            apiKeyRepositoryMock
                .Verify(mock => mock
                .GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);

            apiKeyRepositoryMock
                .Verify(mock => mock
                .SaveAsync(It.Is<ApiKeyReadModel>(model =>
                model.UserId.Equals(userReadModel.Id) &&
                model.IsActive.Equals(true))));

            result.UserId.Should().Be(userReadModel.Id);
            result.IsActive.Should().BeTrue();
        }
        //-------------------------GetAllApiKeys------------------------------------
        [Theory, AutoMoqData]
        public async Task GetAllApiKeys_ReturnsBadHttpException_When_UserIsNull(
            string username,
            string password,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {

            //Arrange 
            userRepositoryMock
                .Setup(mock => mock.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserReadModel)null); // when GetAsync method will be called with any string, we want for the method to return null UserReadModel

            // Act & Assert
            var result = await sut
                .Invoking(sut => sut.GetAllApiKeys(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"User with Username: '{username}' does not exists!");

            result.Which.StatusCode.Should().Be(404);

            // Assert 
            userRepositoryMock
                .Verify(mock => mock
                .GetAsync(It.Is<string>(value => value.Equals(username))), Times.Once);

        }

        [Theory, AutoMoqData]
        public async Task GetAllApiKeys_ReturnsBadHttpException_When_WrongPassword(
            string username,
            string password,
            UserReadModel userReadModel,
            [Frozen] Mock<IUserRepository> userRepositoryMock,
            ApikeyService sut)
        {
            //Arrange 
            userRepositoryMock
                .Setup(mock => mock.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(userReadModel); // when GetAsync method will be called with any string, we want for the method to return UserReadModel

            // Act & Assert
            var result = await sut
                .Invoking(sut => sut.GetAllApiKeys(username, password))
                .Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage($"Wrong password for user: '{userReadModel.Username}'");

            result.Which.StatusCode.Should().Be(400);

            // Assert 
            userRepositoryMock
                .Verify(mock => mock
                .GetAsync(It.Is<string>(value => value.Equals(username))), Times.Once);

        }

        [Theory, AutoMoqData]
        public async Task GetAllApiKeys_When_AllChecks_Pass(
        UserReadModel userReadModel,
        IEnumerable<ApiKeyReadModel> apiKeys,
        [Frozen] Mock<IUserRepository> userRepositoryMock,
        [Frozen] Mock<IApiKeysRepository> apiKeyRepositoryMock,
        ApikeyService sut) // required due to bug with AutoMoq
        {
            //Arrange 

            userRepositoryMock
                .Setup(mock => mock.GetAsync(userReadModel.Username))
                .ReturnsAsync(userReadModel);

            apiKeyRepositoryMock
                .Setup(mock => mock.GetByUserIdAsync(userReadModel.Id))
                .ReturnsAsync(apiKeys); // when GetByUserIdAsync method will be called with ID from 'userReadModel', we want for the method to return IEnumerable<ApiKeyReadModel> (of any size, most probably 3)

            // Act & Assert

            var result = await sut.GetAllApiKeys(userReadModel.Username, userReadModel.Password);

            // Assert 
            userRepositoryMock
                .Verify(mock => mock
                .GetAsync(It.IsAny<string>()), Times.Once);

            apiKeyRepositoryMock
                .Verify(mock => mock
                .GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);

            result.Should().BeEquivalentTo(apiKeys, options => options.ComparingByMembers<ApiKeyReadModel>());

        }
    }
}