using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class AlertCreate
{
    [Required]
    public string AlertName { get; set; }

    [Required]
    public string AlertDesc { get; set; }

    public Guid? AlertPlant { get; set; }
}