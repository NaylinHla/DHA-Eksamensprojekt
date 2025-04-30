namespace Application.Models.Dtos.RestDtos.Request.User;

public class CreateUserDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Birthday { get; set; } = null!;
    public string Country { get; set; } = null!;
}