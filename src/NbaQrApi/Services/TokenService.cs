using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NbaQrApi.Domain;

namespace NbaQrApi.Services;

public interface ITokenService
{
    string CreateAccessToken(Terminal terminal);
}

public sealed class TokenService : ITokenService
{
    private readonly byte[] _key;

    public TokenService(IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is missing.");
        _key = Encoding.ASCII.GetBytes(secret);
    }

    public string CreateAccessToken(Terminal terminal)
    {
        var handler = new JwtSecurityTokenHandler();
        var claims = new List<Claim>
        {
            new("serialNumber", terminal.SerialNumber)
        };

        if (terminal.RegisterTsmId.HasValue)
        {
            claims.Add(new Claim("RegisterTsmId", terminal.RegisterTsmId.Value.ToString()));
        }

        if (terminal.RegisterId.HasValue)
        {
            claims.Add(new Claim("registerId", terminal.RegisterId.Value.ToString()));
        }

        if (terminal.CompanyId.HasValue)
        {
            claims.Add(new Claim("companyId", terminal.CompanyId.Value.ToString()));
        }

        if (terminal.MerchantId.HasValue)
        {
            claims.Add(new Claim("merchantId", terminal.MerchantId.Value.ToString()));
        }

        claims.Add(new Claim("companyCode", terminal.CompanyCode));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
