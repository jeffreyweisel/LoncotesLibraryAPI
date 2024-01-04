using System.ComponentModel.DataAnnotations;


namespace Library.Models.DTOs;

public class CheckoutDTO
{
    public int Id { get; set; }
    [Required]
    public int MaterialId { get; set; }
    public MaterialDTO Material { get; set; }
    [Required]
    public int PatronId { get; set; }
    public PatronDTO Patron { get; set; }
    public DateTime? CheckoutDate { get; set; }

    public DateTime? ReturnDate { get; set; }
}