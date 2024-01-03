using System.ComponentModel.DataAnnotations;

namespace Library.Models.DTOs;

public class GenreDTO
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }

}