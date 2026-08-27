using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class Registerdto
{
[Required]
[StringLength(30, MinimumLength = 3)]
public required string Username{get;set;}
[Required]
[StringLength(100, MinimumLength = 5)]
public required string Password{get;set;}
}
