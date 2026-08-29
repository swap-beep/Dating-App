using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Hubs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet.Actions;

namespace API.Controllers
{
    [Authorize]
    public class UserController(IUserRepository userRepository, IMapper mapper,
    IPhotoService photoService, DataContext context, IHubContext<NotificationHub> hubContext) : BaseApiController
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

        [HttpGet("matches")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMutualMatches()
        {
            var currentUserName = User.GetUsername();
            var currentUser = await context.Users
                .SingleOrDefaultAsync(x => x.UserName == currentUserName);
            if (currentUser == null) return Unauthorized();

            var mutualLikeIds = await context.Likes
                .Where(like => like.SourceUserId == currentUser.Id)
                .Select(like => like.TargetUserId)
                .Intersect(
                    context.Likes
                        .Where(like => like.TargetUserId == currentUser.Id)
                        .Select(like => like.SourceUserId))
                .ToListAsync();

            var matches = await context.Users
                .Include(user => user.Photos)
                .Where(user => mutualLikeIds.Contains(user.Id))
                .ToListAsync();

            return Ok(mapper.Map<IEnumerable<MemberDto>>(matches));
        }

        [HttpGet("notifications")]
        public async Task<ActionResult<IEnumerable<object>>> GetNotifications()
        {
            var currentUser = await context.Users.SingleOrDefaultAsync(x => x.UserName == User.GetUsername());
            if (currentUser == null) return Unauthorized();

            var notifications = await context.Notifications
                .Where(n => n.UserId == currentUser.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    id = n.Id,
                    message = n.Message,
                    type = n.Type,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
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
            if (likeExists)
            {
                var alreadyMatched = await context.Likes.AnyAsync(like =>
                    like.SourceUserId == target.Id && like.TargetUserId == source.Id);

                return Ok(new
                {
                    matched = alreadyMatched,
                    message = alreadyMatched
                        ? $"It's a match! You and {target.UserName} can now message each other."
                        : $"You already liked {target.UserName}."
                });
            }

            context.Likes.Add(new Like { SourceUserId = source.Id, TargetUserId = target.Id });
            await context.SaveChangesAsync();

            var isNowMatched = await context.Likes.AnyAsync(like =>
                like.SourceUserId == target.Id && like.TargetUserId == source.Id);

            if (isNowMatched)
            {
                var matchNotificationForTarget = new Notification
                {
                    UserId = target.Id,
                    Type = "match",
                    Message = $"You and {source.UserName} have matched! You can now message each other.",
                    IsRead = false
                };

                var matchNotificationForSource = new Notification
                {
                    UserId = source.Id,
                    Type = "match",
                    Message = $"You and {target.UserName} have matched! You can now message each other.",
                    IsRead = false
                };

                context.Notifications.Add(matchNotificationForTarget);
                context.Notifications.Add(matchNotificationForSource);

                await context.SaveChangesAsync();

                await hubContext.Clients.User(target.UserName).SendAsync("ReceiveNotification", new
                {
                    message = matchNotificationForTarget.Message,
                    type = matchNotificationForTarget.Type
                });

                await hubContext.Clients.User(source.UserName).SendAsync("ReceiveNotification", new
                {
                    message = matchNotificationForSource.Message,
                    type = matchNotificationForSource.Type
                });
            }
            else
            {
                var likeNotification = new Notification
                {
                    UserId = target.Id,
                    Type = "like",
                    Message = $"{source.UserName} liked you.",
                    IsRead = false
                };

                context.Notifications.Add(likeNotification);
                await context.SaveChangesAsync();

                await hubContext.Clients.User(target.UserName).SendAsync("ReceiveNotification", new
                {
                    message = likeNotification.Message,
                    type = likeNotification.Type
                });
            }

            return Ok(new
            {
                matched = isNowMatched,
                message = isNowMatched
                    ? $"It's a match! You and {target.UserName} can now message each other."
                    : $"You liked {target.UserName}."
            });
        }

        [HttpGet("{username}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(string username)
        {
            var currentUser = await context.Users.SingleOrDefaultAsync(x => x.UserName == User.GetUsername());
            var otherUser = await context.Users.SingleOrDefaultAsync(x => x.UserName == username.ToLower());
            if (currentUser == null || otherUser == null) return NotFound();

            var isMutualMatch = await context.Likes.AnyAsync(like =>
                like.SourceUserId == currentUser.Id && like.TargetUserId == otherUser.Id)
                && await context.Likes.AnyAsync(like =>
                like.SourceUserId == otherUser.Id && like.TargetUserId == currentUser.Id);

            if (!isMutualMatch)
            {
                return BadRequest("You can only message someone you matched with.");
            }

            var messages = await context.Messages
                .Where(message =>
                    (message.SenderUserId == currentUser.Id && message.RecipientUserId == otherUser.Id) ||
                    (message.SenderUserId == otherUser.Id && message.RecipientUserId == currentUser.Id))
                .OrderBy(message => message.CreatedAt)
                .Select(message => new MessageDto
                {
                    Id = message.Id,
                    Content = message.Content,
                    SenderUserId = message.SenderUserId,
                    RecipientUserId = message.RecipientUserId,
                    SenderUsername = message.Sender.UserName,
                    RecipientUsername = message.Recipient.UserName,
                    CreatedAt = message.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("{username}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessage(string username, SendMessageDto sendMessageDto)
        {
            var currentUser = await context.Users.SingleOrDefaultAsync(x => x.UserName == User.GetUsername());
            var otherUser = await context.Users.SingleOrDefaultAsync(x => x.UserName == username.ToLower());
            if (currentUser == null || otherUser == null) return NotFound();

            var isMutualMatch = await context.Likes.AnyAsync(like =>
                like.SourceUserId == currentUser.Id && like.TargetUserId == otherUser.Id)
                && await context.Likes.AnyAsync(like =>
                like.SourceUserId == otherUser.Id && like.TargetUserId == currentUser.Id);

            if (!isMutualMatch)
            {
                return BadRequest("You can only message someone you matched with.");
            }

            var message = new Message
            {
                Content = sendMessageDto.Content.Trim(),
                SenderUserId = currentUser.Id,
                RecipientUserId = otherUser.Id,
            };

            context.Messages.Add(message);
            await context.SaveChangesAsync();

            return Ok(new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SenderUserId = message.SenderUserId,
                RecipientUserId = message.RecipientUserId,
                SenderUsername = currentUser.UserName,
                RecipientUsername = otherUser.UserName,
                CreatedAt = message.CreatedAt
            });
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
