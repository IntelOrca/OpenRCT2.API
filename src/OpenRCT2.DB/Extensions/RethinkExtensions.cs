using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Net;

namespace OpenRCT2.DB
{
    internal static class RethinkExtensions
    {
        public static GetAll GetAllByIndex<T>(this Table expr, string index, T value)
        {
            return expr.GetAll(value)[new { index }];
        }

        public static async Task<T> RunFirstOrDefaultAsync<T>(this ReqlExpr expr, IConnection conn)
        {
            var result = await expr.Limit(1).RunCursorAsync<T>(conn);
            return result.FirstOrDefault();
        }

        public static async Task<T[]> RunArrayAsync<T>(this ReqlExpr expr, IConnection conn)
        {
            var huh = await expr.RunResultAsync(conn);
            var cursor = await expr.RunCursorAsync<T>(conn);
            return cursor.ToArray();
        }
    }
}