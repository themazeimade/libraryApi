using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using LibraryApi.Models;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryApi.Services;
using LibraryApi.Utilities;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly string _jwtSecret;
    private readonly MongoDbService _mongo;
    private readonly RefreshTokenService _refreshTokenService;

    public AuthController(IConfiguration configuration, MongoDbService mongo, RefreshTokenService refreshTokenService)
    {
        // Read the User Secret
        _mongo = mongo;
        _jwtSecret = configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("JWT SigningKey is missing");
        _refreshTokenService = refreshTokenService;
    }

    [HttpPost]
    [Authorize]
    public IActionResult CheckAuth()
    {
        return Ok("I am authorized");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel user)
    {
        Console.WriteLine("Start...\n");

        var filter = Builders<UserModel>.Filter.Eq(c => c.Username, user.username);
        var User = await _mongo.Users.Find(filter).FirstOrDefaultAsync();

        if (User == null)
            return Unauthorized("Username not found");

        // Console.WriteLine(user);
        Console.WriteLine("Login in...\n");
        var hasher = new PasswordHasher<UserModel>();

        var result = hasher.VerifyHashedPassword(User, User.PasswordHash, user.password);

        if (result == PasswordVerificationResult.Success)
        {
            var (refreshToken, _) = await _refreshTokenService.CreateRefreshToken(_mongo, User.Id);
            Response.Cookies.Append(
                "refresh_token",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30)
                });

            var token = GenerateJwtToken(User);
            return Ok(new { token });
        }
        return Unauthorized();
    }

    private string GenerateJwtToken(UserModel user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (UserRole role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "my-library-api",
            audience: "my-library-api",
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        Console.WriteLine("Refreshed called");
        var refreshToken = Request.Cookies["refresh_token"];
        if (refreshToken == null)
            return Unauthorized("RefreshToken Cookie not found");


        var filter = Builders<RefreshTokenModel>.Filter.And(
            Builders<RefreshTokenModel>.Filter.Eq(c => c.Token, TokenHasher.Hash(refreshToken)),
            Builders<RefreshTokenModel>.Filter.Gte(c => c.ExpiresAt, DateTime.UtcNow),
            Builders<RefreshTokenModel>.Filter.Eq(c => c.ReplacedAt, null)
        );

        var storedToken = await _mongo.RefreshTokens
            .Find(filter)
            .FirstOrDefaultAsync();



        if (storedToken == null)
            return Unauthorized("RefreshToken entry not found");

        var filter_0 = Builders<UserModel>.Filter.Eq(c => c.Id, storedToken.UserId);
        var User = await _mongo.Users.Find(filter_0).FirstOrDefaultAsync();

        if (User == null)
            return Unauthorized("UserId not valid");

        //get new refresh token
        var (newRefreshToken, newRefreshId) = await _refreshTokenService.CreateRefreshToken(_mongo, User.Id);

        filter = Builders<RefreshTokenModel>.Filter.And(
            Builders<RefreshTokenModel>.Filter.Eq(c => c.Id, storedToken.Id),
            Builders<RefreshTokenModel>.Filter.Eq(c => c.ReplacedAt, null)
        );
        var update = Builders<RefreshTokenModel>.Update.Set(c => c.ReplacedAt, DateTime.UtcNow)
          .Set(c => c.ReplacedBy, newRefreshId);

        var res = await _mongo.RefreshTokens.UpdateOneAsync(filter, update);

        // if (res.MatchedCount == 0) return NotFound();
        if (res.ModifiedCount != 1) return Unauthorized();

        Response.Cookies.Append(
            "refresh_token",
            newRefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30)
            });

        var token = GenerateJwtToken(User);


        Console.WriteLine("Refresh successful");
        return Ok(new { token });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogOut()
    {
        Console.WriteLine("In logout");
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Console.WriteLine("this is username: " + username);
        var filter_0 = Builders<UserModel>.Filter.Eq(c => c.Username, username);
        var UserEntry = await _mongo.Users.Find(filter_0).FirstOrDefaultAsync();

        if (UserEntry == null)
            return Unauthorized("UserName not valid");

        var filter = Builders<RefreshTokenModel>.Filter.And(
            Builders<RefreshTokenModel>.Filter.Eq(c => c.UserId, UserEntry.Id),
            Builders<RefreshTokenModel>.Filter.Eq(c => c.ReplacedAt, null)
        );
        var update = Builders<RefreshTokenModel>.Update.Set(c => c.ReplacedAt, DateTime.UtcNow);

        var res = await _mongo.RefreshTokens.UpdateOneAsync(filter, update);

        if (res.ModifiedCount != 1) return Unauthorized();

        Response.Cookies.Append(
            "refresh_token",
            "",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30)
            });

        return Ok("Logged Out succesfully");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult checkIfAdmin()
    {
        return Ok("You are admin!");
    }
}
