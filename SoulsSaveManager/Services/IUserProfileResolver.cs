using System.Threading;
using System.Threading.Tasks;

namespace SoulsSaveManager.Services;

public interface IUserProfileResolver
{
    IReadOnlyList<string> DiscoverProfiles(string gameAlias, string saveRootOrCustomPath);
    string GetDisplayLabel(string profileId, string gameAlias);
    Task<string> ResolveLabelAsync(string profileId, string gameAlias, CancellationToken cancellationToken = default);
}
