using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using RethinkDb.Driver;

namespace OpenRCT2.DB.Repositories
{
    internal class AuthTokenRepository : IAuthTokenRepository
    {
        private static readonly RethinkDB R = RethinkDB.R;
        private readonly IDBService _dbService;

        public AuthTokenRepository(IDBService dbService)
        {
            _dbService = dbService;
        }

        public async Task<AuthToken> GetFromTokenAsync(string token)
        {
            var conn = await _dbService.GetConnectionAsync();
            return await R
                .Table(TableNames.AuthTokens)
                .GetAllByIndex(nameof(AuthToken.Token), token)
                .RunFirstOrDefaultAsync<AuthToken>(conn);
        }

        public async Task InsertAsync(AuthToken authToken)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.AuthTokens)
                .Insert(authToken);
            var result = await query.RunWriteAsync(conn);
            if (result.GeneratedKeys != null && result.GeneratedKeys.Length != 0)
            {
                authToken.Id = result.GeneratedKeys[0].ToString();
            }
        }

        public async Task UpdateAsync(AuthToken authToken)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.AuthTokens)
                .Get(authToken.Id)
                .Update(authToken);
            await query.RunWriteAsync(conn);
        }

        public async Task DeleteAsync(string token)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.AuthTokens)
                .GetAllByIndex(nameof(AuthToken.Token), token)
                .Delete();
            await query.RunWriteAsync(conn);
        }

        public async Task DeleteAllAsync(string userId)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.AuthTokens)
                .GetAllByIndex(nameof(AuthToken.UserId), userId)
                .Delete();
            await query.RunWriteAsync(conn);
        }
    }
}
