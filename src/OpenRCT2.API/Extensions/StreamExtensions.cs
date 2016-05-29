using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenRCT2.API.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<byte[]> ReadToBytesAsync(this Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                byte[] data = ms.ToArray();
                return data;
            }
        }

        public static async Task<string> ReadToStringAsync(this Stream stream)
        {
            byte[] data = await ReadToBytesAsync(stream);
            string text = Encoding.UTF8.GetString(data);
            return text;
        }
    }
}
