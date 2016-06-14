using System.Threading.Tasks;

namespace OpenRCT2.API.AppVeyor
{
    public interface IAppVeyorService
    {
        Task<JBuild> GetLastBuildAsync(string account, string project);
        Task<JBuild> GetLastBuildAsync(string account, string project, string branch);
        Task<string> GetLastBuildJobIdAsync(string account, string project, string branch);
        Task<JMessage[]> GetMessagesAsync(string jobId);
    }
}
