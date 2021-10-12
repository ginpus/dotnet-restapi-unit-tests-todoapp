using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestAPI.Models;

namespace RestAPI.Services
{
    public interface IApikeyService
    {
        Task<ApiKeyModel> CreateApiKey(string username, string password);

        Task<IEnumerable<ApiKeyModel>> GetAllApiKeys(string username, string password);

        Task<ApiKeyModel> UpdateApiKeyState(Guid id, bool newState);
    }
}