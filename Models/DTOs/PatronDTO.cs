using System.ComponentModel.DataAnnotations;

namespace Library.Models.DTOs;

public class PatronDTO
{
    public int Id { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Address { get; set; }
    [Required]
    public bool IsActive { get; set; }
    public List<CheckoutDTO> Checkouts { get; set; }
    

}