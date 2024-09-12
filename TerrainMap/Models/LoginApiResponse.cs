using System;

namespace TerrainMap.Models;

public class LoginApiResponse
{
    public string? ErrorMessage;

    public string? AccessToken;

    public string? RefreshToken;

    public string? IdToken;

    public DateTime? AccessTokenExpires;

    public bool Success => ErrorMessage is null;
}
