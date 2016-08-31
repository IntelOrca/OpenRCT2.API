using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Extensions;

namespace OpenRCT2.API.Implementations
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly ILogger<UserSessionRepository> _logger;
        private readonly Random _random = new Random();
        private readonly ConcurrentDictionary<int, UserSession> _userSessions = new ConcurrentDictionary<int, UserSession>();
        private readonly ConcurrentDictionary<string, int> _tokenToUserIdMap = new ConcurrentDictionary<string, int>();

        public UserSessionRepository(ILogger<UserSessionRepository> logger)
        {
            _logger = logger;
        }

        public Task<string> CreateToken(int userId)
        {
            UserSession userSession = GetUserSession(userId);

            string token = CreateRandomToken();
            if (_tokenToUserIdMap.TryAdd(token, userId))
            {
                userSession.AddToken(token);

                _logger.LogInformation("Creating new token for user id {0}", userId);
                _logger.LogTrace("CreateToken: UserId = {0}, Token = {1}", userId, token);
            }
            return Task.FromResult(token);
        }

        public Task<bool> RevokeToken(string token)
        {
            int userId;
            if (_tokenToUserIdMap.TryRemove(token, out userId))
            {
                UserSession userSession = GetUserSession(userId);
                userSession.RevokeToken(token);

                _logger.LogInformation("Revoking token for user id {0}", userId);
                _logger.LogTrace("RevokeToken: UserId = {0}, Token = {1}", userId, token);

                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<int?> GetUserIdFromToken(string token)
        {
            _logger.LogTrace("GetUserIdFromToken: Token = {0}", token);

            int? result = null;
            int userId;
            if (_tokenToUserIdMap.TryGetValue(token, out userId))
            {
                result = userId;
            }
            return Task.FromResult<int?>(result);
        }

        private UserSession GetUserSession(int userId)
        {
            UserSession userSession = _userSessions.GetOrAdd(userId, key => new UserSession(key));
            return userSession;
        }

        private string CreateRandomToken()
        {
            string token = _random.NextBytes(16).ToHexString();
            return token;
        }

        private class UserSession
        {
            private List<string> _tokens = new List<string>();
            private object _tokensSync = new object();

            public int UserId { get; }
            public string[] Tokens
            {
                get
                {
                    lock (_tokensSync)
                    {
                        return _tokens.ToArray();
                    }
                }
            }

            public UserSession(int userId)
            {
                UserId = userId;
            }

            public void AddToken(string token)
            {
                lock (_tokensSync)
                {
                    _tokens.Add(token);
                }
            }

            public void RevokeToken(string token)
            {
                lock (_tokensSync)
                {
                    _tokens.Remove(token);
                }
            }

            public bool TokenExists(string token)
            {
                lock (_tokensSync)
                {
                    return _tokens.Contains(token);
                }
            }
        }
    }
}
