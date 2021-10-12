using Persistence.Models.ReadModels;
using RestAPI.Models;

namespace RestAPI.Extensions
{
    public static class ApiKeysReadModelIMapper
    {
        public static ApiKeyModel MapToApiKey(this ApiKeyReadModel apiKey)
        {
            return new ApiKeyModel
            {
                Id = apiKey.Id,
                ApiKey = apiKey.ApiKey,
                UserId = apiKey.UserId,
                IsActive = apiKey.IsActive,
                DateCreated = apiKey.DateCreated,
                ExpirationDate = apiKey.ExpirationDate
            };
        }
    }
}
