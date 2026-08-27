using System;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using API.Data;
using AutoMapper;
using API.DTOs;
using AutoMapper.QueryableExtensions;

namespace API.Data;

public class UserRepository(DataContext context , IMapper mapper) : IUserRepository
{
    public async Task<IEnumerable<MemberDto>> GetMemberAsync()
    {
       return await context.Users
       .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
        .ToListAsync();
    }

    public async Task<MemberDto?> GetMemberAsync(string username)
    {
       return await context.Users
       .Where(x=>x.UserName==username)
       .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
       .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<AppUser?>> GetUserAsync()
    {
        return await context.Users
        .Include(x=>x.Photos)
        .ToListAsync();
    }

    public async Task<AppUser?> GetUserByIDAsync(int id)
    {
        return await context.Users
        .Include(x=>x.Photos)
        .FirstOrDefaultAsync(x => x.Id == id);

    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await context.Users.SingleOrDefaultAsync(x => x.UserName==username);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync()>0 ;
        }

    public void Upadate(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}
