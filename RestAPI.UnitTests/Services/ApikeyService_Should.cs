using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Persistence.Models.ReadModels;
using Persistence.Repositories;
using RestAPI.Options;
using RestAPI.Services;

namespace RestAPI.UnitTests.Services
{
    
    public class ApikeyService_Should
    {
        // public async Task TestCreate()
        // {
        //     var apiKeySettingsMock = new Mock<IOptions<ApiKeySettings>>();
        //
        //     var apiKeySettings = new ApiKeySettings
        //     {
        //         ExpirationTimeInMinutes = 455,
        //         ApiKeyLimit = 54
        //     };
        //
        //     apiKeySettingsMock
        //         .SetupGet(apiKeySettings => apiKeySettings.Value)
        //         .Returns(apiKeySettings);
        //
        //     var apiKeyService = new ApikeyService(new UsersRepository(), new ApiKeysRepository(), apiKeySettingsMock.Object);
        // }

        public async Task CreateApiKey_ReturnsBadHttpException_When_UserIsNull()
        {
            var userRepositoryMock = new Mock<IUserRepository>();

            userRepositoryMock
                .Setup(mock => mock.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserReadModel) null);
            
            // Act
            // var result = 
                
            // Assert 
                
        }
        
        public async Task CreateApiKey_ReturnsBadHttpException_When_WrongPassword()
        {
            var userRepositoryMock = new Mock<IUserRepository>();

            userRepositoryMock
                .Setup(mock => mock.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((UserReadModel) null);
            
            // Act
            // var result = 
                
            // Assert 
                
        }
    }
}