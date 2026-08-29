 using System;
using System.ComponentModel.DataAnnotations.Schema;
using API.Extensions;

namespace API.Entities;

public class AppUser
{
public int Id { get; set; }
public required string UserName { get; set; } 
public  byte[] PasswordHash { get; set; } =[];
public  byte[] PasswordSalt { get; set; } =[];

public DateOnly DateOfbirth {get;set;}
public  string? KnownsAs {get;set;}
public DateTime Created {get;set;} = DateTime.UtcNow;
public DateTime LastActive {get;set;} = DateTime.UtcNow;

public required string Gender {get;set;}
public  string? Introduction {get;set;}
public string? Interests {get;set;}
public string?LookingFor {get;set;}

public required string City {get;set;}

public required string Country{get ; set;}
public List<Photo>  Photos{get ; set;} =[];
public List<Like> LikedUsers { get; set; } = [];
public List<Like> LikedByUsers { get; set; } = [];

[InverseProperty(nameof(Message.Sender))]
public List<Message> SentMessages { get; set; } = [];

[InverseProperty(nameof(Message.Recipient))]
public List<Message> ReceivedMessages { get; set; } = [];

[InverseProperty(nameof(Notification.User))]
public List<Notification> Notifications { get; set; } = [];

// public int GetAge(){
//     return DateOfbirth.CalculateAge();
// }


}

