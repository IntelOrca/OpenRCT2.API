using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Threading;
using OpenRCT2.DB.Abstractions;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace OpenRCT2.DB
{
    internal class DBService : IDBService
    {
        private readonly DBOptions _options;
        private readonly ILogger _logger;

        private volatile Connection _sharedConnection;
        private AsyncSemaphore _mutex = new AsyncSemaphore(1);

        public DBService(IOptions<DBOptions> options, ILogger<DBService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public void Dispose()
        {
            _sharedConnection?.Dispose();
            _mutex.Dispose();
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            var connection = _sharedConnection;
            if (connection == null || !connection.Open)
            {
                using (await _mutex.EnterAsync())
                {
                    connection = _sharedConnection;
                    if (connection == null || connection.Open)
                    {
                        _sharedConnection = null;
                        try
                        {
                            connection?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Disposing rethinkdb connection failed");
                        }
                        _sharedConnection = connection = await CreateConnectionAsync();
                    }
                }
            }
            return connection;
        }

        private async Task<Connection> CreateConnectionAsync()
        {
            if (_options.Host == null)
            {
                throw new Exception("Database has not been configured");
            }

            var R = RethinkDB.R;
            var connBuilder = R.Connection()
                               .Hostname(_options.Host)
                               .Port(RethinkDBConstants.DefaultPort)
                               .User(_options.User, _options.Password)
                               .Db(_options.Name);
            Connection connection = await connBuilder.ConnectAsync();
            return connection;
        }

        public Task SetupAsync()
        {
            var dbSetup = new DBSetup(this);
            return dbSetup.SetupAsync();
        }
    }
}
