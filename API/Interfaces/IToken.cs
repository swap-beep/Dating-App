using System;
using API.Entities;

namespace API.Interfaces;

public interface IToken
{
    string CreateToken(AppUser user);

}
