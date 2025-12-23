using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using LibraryApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryApi.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly string _jwtSecret;
    private readonly MongoDbService _mongo;

    public AuthController(IConfiguration configuration, MongoDbService mongo)
    {
        // Read the User Secret
        _mongo = mongo;
        _jwtSecret = configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("JWT SigningKey is missing");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel user)
    {

        var filter = Builders<UserModel>.Filter.Eq(c => c.Username, user.username);
        var User = await _mongo.Users.Find(filter).FirstOrDefaultAsync();

        if (User == null)
            return BadRequest("Username not found");

        var hasher = new PasswordHasher<UserModel>();

        var result = hasher.VerifyHashedPassword(User, User.PasswordHash, user.password);

        if (result == PasswordVerificationResult.Success)
        {
            var token = GenerateJwtToken(User);
            return Ok(new { token });
        }
        return Unauthorized();
    }

    private string GenerateJwtToken(UserModel user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "my-library-api",
            audience: "my-library-api",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
