using System.Collections.Generic;
using System.Threading.Tasks;
using OpenRCT2.DB.Abstractions;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace OpenRCT2.DB
{
    internal class DBSetup
    {
        private readonly static RethinkDB R = RethinkDB.R;
        private readonly IDBService _dbService;

        public DBSetup(IDBService dbService)
        {
            _dbService = dbService;
        }

        public async Task SetupAsync()
        {
            IConnection conn = await _dbService.GetConnectionAsync();

            // Create tables and indexes
            var tables = await GetTablesAsync(conn);
            await CreateTableAsync(conn, tables, TableNames.Users, "OpenRCT2orgId");
        }

        private async Task CreateTableAsync(IConnection conn, HashSet<string> existingTables, string table, params string[] indexes)
        {
            if (!existingTables.Contains(TableNames.Users))
            {
                await R
                    .TableCreate(table)
                    .RunResultAsync(conn);
            }

            var existingIndexes = await GetIndexesAsync(conn, table);
            foreach (string index in indexes)
            {
                if (!existingIndexes.Contains(index))
                {
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
