using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class AuthRegisterDto
{
    [MinLength(2)] [Required] public string FirstName { get; set; } = null!;
    [MinLength(2)] [Required] public string LastName { get; set; } = null!;
    [MinLength(3)] [Required] public string Email { get; set; } = null!;
    [Required] public DateTime Birthday { get; set; }
    [Required] public string Country { get; set; } = null!;
    [MinLength(4)] [Required] public string Password { get; set; } = null!;
}