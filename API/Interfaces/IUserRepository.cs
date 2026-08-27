using System;
using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
void Upadate (AppUser user);
Task <bool> SaveAllAsync();
Task<IEnumerable<AppUser>> GetUserAsync();

Task<AppUser?> GetUserByIDAsync( int id);
Task<AppUser?> GetUserByUsernameAsync( string username);

Task <IEnumerable<MemberDto>> GetMemberAsync();
Task <MemberDto> GetMemberAsync( string username);


}
