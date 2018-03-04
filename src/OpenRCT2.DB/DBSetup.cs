using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
            var reqEntities = GetRequiredTableNamesAndIndexes();

            // Create tables and indexes
            var conn = await _dbService.GetConnectionAsync();
            var tables = await GetTablesAsync(conn);
            foreach (var (TableName, Indexes) in reqEntities)
            {
                await CreateTableAsync(conn, tables, TableName, Indexes);
            }
        }

        private (string TableName, string[] Indexes)[] GetRequiredTableNamesAndIndexes()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Select(t => (Type: t, TableName: t.GetCustomAttribute<TableAttribute>()?.Name))
                .Where(x => x.TableName != null)
                .Select(x => (x.TableName, GetSecondaryIndexes(x.Type)))
                .ToArray();

            string[] GetSecondaryIndexes(Type modelType)
            {
                return modelType.GetProperties()
                    .Where(x => x.GetCustomAttribute<SecondaryIndexAttribute>() != null)
                    .Select(x => x.Name)
                    .ToArray();
            }
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
