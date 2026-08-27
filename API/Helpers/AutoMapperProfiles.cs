using System;
using API.Entities;
using AutoMapper;
using API.DTOs;
using API.Extensions;

namespace API.Helpers;

public class AutoMapperProfiles:Profile
{
public AutoMapperProfiles()
{


    CreateMap<AppUser,MemberDto>()
    .ForMember(d=>d.Age,o=>o.MapFrom(s=>s.DateOfbirth.CalculateAge()))
.ForMember(d=>d.PhotoUrl , o=>o.MapFrom(s=> s.Photos.Any(x=>x.IsMain) ? s.Photos.First(x=>x.IsMain).Url : null))
    .ForMember(d => d.Photos, o => o.MapFrom(s => s.Photos));
        CreateMap<Photo, PhotoDto>();

     CreateMap<MemberUpdateDto,AppUser>();
}
}
