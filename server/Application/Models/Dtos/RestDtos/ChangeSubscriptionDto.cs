namespace Application.Models.Dtos.RestDtos;

public class ChangeSubscriptionDto
{
    public required string ClientId { get; set; }
    public List<string> TopicIds { get; set; } = null!;
}