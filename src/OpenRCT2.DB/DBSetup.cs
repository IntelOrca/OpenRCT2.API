using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;

namespace OpenRCT2.DB
{
    internal class DBSetup
    {
        private readonly IDBService _dbService;

        public DBSetup(IDBService dbService)
        {
            _dbService = dbService;
        }

        public Task SetupAsync()
        {
            return Task.FromResult(0);
        }
    }
}
