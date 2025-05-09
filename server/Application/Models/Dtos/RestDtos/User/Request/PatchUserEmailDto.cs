using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos.Request;

public class PatchUserEmailDto
{
    [Required] [EmailAddress] public string OldEmail { get; set; } = null!;

    [Required] [EmailAddress] public string NewEmail { get; set; } = null!;
}