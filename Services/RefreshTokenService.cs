using LibraryApi.Types;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using LibraryApi.Models;
using LibraryApi.Utilities;
using LibraryApi.Services;


public class RefreshTokenService
{
    private readonly TokenOptions _options;

    public RefreshTokenService(IOptions<TokenOptions> options)
    {
        _options = options.Value;
    }

    public async Task<(string, string)> CreateRefreshToken(MongoDbService mongo, string UserId)
    {
        var token = GenerateToken();
        var refreshToken = new RefreshTokenModel
        {
            UserId = UserId,
            Token = TokenHasher.Hash(token),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays),
        };

        await mongo.RefreshTokens.InsertOneAsync(refreshToken);
        return (token, refreshToken.Id);
    }


    public static string GenerateToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

}
