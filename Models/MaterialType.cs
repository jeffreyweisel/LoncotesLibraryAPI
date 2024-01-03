using System.ComponentModel.DataAnnotations;

namespace Library.Models;

public class MaterialType
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public int CheckoutDays { get; set; }
    
}