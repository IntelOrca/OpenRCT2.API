using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OpenRCT2.DB.Abstractions;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace OpenRCT2.DB
{
    internal class DBService : IDBService
    {
        private readonly DBOptions _options;

        private volatile Connection _sharedConnection;
        private Mutex _mutex;

        public DBService(IOptions<DBOptions> options)
        {
            _options = options.Value;
            _mutex = new Mutex();
        }

        public void Dispose()
        {
            _sharedConnection?.Dispose();
            _mutex.Dispose();
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            Connection connection = _sharedConnection;
            if (connection == null || !connection.Open)
            {
                _mutex.WaitOne();
                if (_sharedConnection == null || _sharedConnection.Open)
                {
                    _sharedConnection?.Dispose();
                    _sharedConnection = null;
                    _sharedConnection = CreateConnection().Result;
                    connection = _sharedConnection;
                }
                _mutex.ReleaseMutex();
            }
            return connection;
        }

        private async Task<Connection> CreateConnection()
        {
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