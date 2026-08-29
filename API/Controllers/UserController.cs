using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet.Actions;

namespace API.Controllers
{
    [Authorize]
    public class UserController(IUserRepository userRepository, IMapper mapper,
    IPhotoService photoService, DataContext context) : BaseApiController
    {
        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await userRepository.GetMemberAsync(); // Fetch users from database
                                                               // var usersToReturn = mapper.Map<IEnumerable<MemberDto>>(users);

            return Ok(users);  // Return the list of users
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {

            var user = await userRepository.GetMemberAsync(username);
            if (user == null) return NotFound();



            return (user);
        }

        [HttpGet("likes")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetLikedUsers()
        {
            var username = User.GetUsername();
            var user = await context.Users.SingleOrDefaultAsync(x => x.UserName == username);
            if (user == null) return Unauthorized();

            var likedTargetIds = context.Likes
                .Where(like => like.SourceUserId == user.Id)
                .Select(like => like.TargetUserId);

            var likedUsers = await context.Users
                .Include(target => target.Photos)
                .Where(target => likedTargetIds.Contains(target.Id))
                .ToListAsync();

            return Ok(mapper.Map<IEnumerable<MemberDto>>(likedUsers));
        }

        [HttpPost("{username}/like")]
        public async Task<ActionResult> LikeUser(string username)
        {
            var source = await context.Users.SingleOrDefaultAsync(x => x.UserName == User.GetUsername());
            var target = await context.Users.SingleOrDefaultAsync(x => x.UserName == username.ToLower());
            if (source == null || target == null) return NotFound();
            if (source.Id == target.Id) return BadRequest("You cannot like yourself");

            var likeExists = await context.Likes.AnyAsync(like =>
                like.SourceUserId == source.Id && like.TargetUserId == target.Id);
            if (likeExists) return NoContent();

            context.Likes.Add(new Like { SourceUserId = source.Id, TargetUserId = target.Id });
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{username}/like")]
        public async Task<ActionResult> UnlikeUser(string username)
        {
            var source = await context.Users.SingleOrDefaultAsync(x => x.UserName == User.GetUsername());
            var target = await context.Users.SingleOrDefaultAsync(x => x.UserName == username.ToLower());
            if (source == null || target == null) return NotFound();

            var like = await context.Likes.SingleOrDefaultAsync(item =>
                item.SourceUserId == source.Id && item.TargetUserId == target.Id);
            if (like == null) return NoContent();

            context.Likes.Remove(like);
            await context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {

            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("Could not find user");
            mapper.Map(memberUpdateDto, user);
            if (await userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed t update user");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Please select an image to upload");
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only image files are supported");
            if (file.Length > 10 * 1024 * 1024) return BadRequest("Image must be smaller than 10 MB");

            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("cannot update user");
            ImageUploadResult result;
            try
            {
                result = await photoService.AddPhotoAsync(file);
            }
            catch (Exception)
            {
                return BadRequest("Photo storage is unavailable. Check the Cloudinary configuration.");
            }
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            user.Photos.Add(photo);
            if (await userRepository.SaveAllAsync())
                return CreatedAtAction(nameof(GetUser),
                new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
            return BadRequest("Problem adding Photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {

            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("could not find user");
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null || photo.IsMain) return BadRequest("cannot use this as main photo");
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting main photo");
        }


        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("User not found");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null || photo.IsMain) return BadRequest("This photo can not be deleted");

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photo");
        }

    }
}
