using System.ComponentModel.DataAnnotations;

namespace Application.Models.Enums;

public static class Constants
{
    [Required] public const string UserRole = "user";

    [Required] public const string AdminRole = "admin";
}