using System.Threading.Tasks;
using TerrainMap.Models;

namespace TerrainMap.Services.Interface;

public interface ITerrainAuthService
{
    Task<LoginApiResponse> AttemptLoginWithCredentials(Branch branch, int memberNumber, string password);

    Task<LoginApiResponse> AttemptLoginWithRefreshToken(string refreshToken);
}
