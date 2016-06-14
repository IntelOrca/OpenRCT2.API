using System.Threading.Tasks;

namespace OpenRCT2.API.Abstractions
{
    public interface ILocalisationService
    {
        Task<int> GetLanguageProgressAsync(string languageId);
    }
}
