using System.Linq;
using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using RethinkDb.Driver;

namespace OpenRCT2.DB.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private static readonly RethinkDB R = RethinkDB.R;
        private readonly IDBService _dbService;

        public UserRepository(IDBService dbService)
        {
            _dbService = dbService;
        }

        public async Task<User[]> GetAll()
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Users);
            var result = await query.RunCursorAsync<User>(conn);
            return result.ToArray();
        }

        public async Task<User> GetUserFromIdAsync(string id)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Users)
                .Get(id);
            var result = await query.RunAtomAsync<User>(conn);
            return result;
        }

        public async Task<User> GetUserFromOpenRCT2orgIdAsync(int id)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Users)
                .GetAll(id)[new { index = "OpenRCT2orgId" }]
                .Filter(1);
            var result = await query.RunCursorAsync<User>(conn);
            return result.FirstOrDefault();
        }

        public async Task InsertUserAsync(User user)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Users)
                .Insert(user);
            var result = await query.RunResultAsync(conn);
            user.Id = result.GeneratedKeys[0].ToString();
        }

        public async Task UpdateUserAsync(User user)
        {
            var conn = await _dbService.GetConnectionAsync();
            var query = R
                .Table(TableNames.Users)
                .Get(user.Id)
                .Update(user);
            await query.RunResultAsync(conn);
        }
    }
}
