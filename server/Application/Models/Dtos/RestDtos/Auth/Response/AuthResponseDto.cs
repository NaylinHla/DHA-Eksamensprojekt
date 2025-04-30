using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class AuthResponseDto
{
    [Required] public string Jwt { get; set; } = null!;
}