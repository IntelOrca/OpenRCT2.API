using System.Threading.Tasks;

namespace OpenRCT2.API.AppVeyor
{
    public interface IAppVeyorService
    {
        Task<JBuild> GetLastBuild(string account, string project);
        Task<JBuild> GetLastBuild(string account, string project, string branch);
    }
}
