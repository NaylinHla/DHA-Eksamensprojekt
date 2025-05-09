using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos.Request;

public class DeleteUserDto
{
    [Required] [EmailAddress] public string Email { get; set; } = null!;
}