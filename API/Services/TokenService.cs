using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService (IConfiguration config): IToken
{
public string CreateToken(AppUser user){
   var tokenKey = config["TokenKey"] ?? throw new Exception ("cannot excess token from app setting");
   if (tokenKey.Length<64) throw new Exception("You Token key needs to  be longer");
   var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

   var claims = new List<Claim>
   {
    new Claim(ClaimTypes.NameIdentifier , user.UserName)
   };

var cred = new SigningCredentials(key , SecurityAlgorithms.HmacSha512Signature);

   var tokenDescriptor = new SecurityTokenDescriptor
   {
    Subject= new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddDays(7),
    SigningCredentials = cred
   };

   var tokenHandeler = new JwtSecurityTokenHandler();
   var token = tokenHandeler.CreateToken(tokenDescriptor);

   return tokenHandeler.WriteToken(token);
}
}
