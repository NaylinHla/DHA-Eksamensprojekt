using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos.Request;

public class PatchUserPasswordDto
{
    [Required]
    public string OldPassword { get; set; } = null!;
    
    [Required]
    public string NewPassword { get; set; } = null!;
}