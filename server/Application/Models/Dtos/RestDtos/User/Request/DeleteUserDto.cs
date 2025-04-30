using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos.Request;

public class DeleteUserDto
{
    public string UserId { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}