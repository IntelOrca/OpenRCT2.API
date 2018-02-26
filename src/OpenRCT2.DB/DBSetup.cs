using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace OpenRCT2.DB
{
    internal class DBSetup
    {
        private readonly static RethinkDB R = RethinkDB.R;
        private readonly IDBService _dbService;
        private readonly ILogger _logger;

        public DBSetup(IDBService dbService, ILogger logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        public async Task SetupAsync()
        {
            _logger.LogInformation("Creating required database tables");

            IConnection conn = await _dbService.GetConnectionAsync();

            // Create tables and indexes
            var tables = await GetTablesAsync(conn);
            await CreateTableAsync(conn, tables, TableNames.Users, nameof(User.Name), nameof(User.Email), nameof(User.OpenRCT2orgId));
            await CreateTableAsync(conn, tables, TableNames.AuthTokens, nameof(AuthToken.Token), nameof(AuthToken.UserId));
        }

        private async Task CreateTableAsync(IConnection conn, HashSet<string> existingTables, string table, params string[] indexes)
        {
            if (!existingTables.Contains(table))
            {
                _logger.LogInformation($"Creating table {table}");
                await R
                    .TableCreate(table)
                    .RunResultAsync(conn);
            }

            var existingIndexes = await GetIndexesAsync(conn, table);
            foreach (string index in indexes)
            {
                if (!existingIndexes.Contains(index))
                {
                    _logger.LogInformation($"Creating secondary index {table}.{index}");
                    await R
                        .Table(table)
                        .IndexCreate(index)
                        .RunResultAsync(conn);
                }
            }
        }

        private async Task<HashSet<string>> GetTablesAsync(IConnection conn)
        {
            string[] tables = await R
                .TableList()
                .RunAtomAsync<string[]>(conn);
            return new HashSet<string>(tables);
        }

        private async Task<HashSet<string>> GetIndexesAsync(IConnection conn, string table)
        {
            string[] indexes = await R
                .Table(table)
                .IndexList()
                .RunAtomAsync<string[]>(conn);
            return new HashSet<string>(indexes);
        }
    }
}
