using System.ComponentModel.DataAnnotations;


namespace Library.Models;

public class Checkout
{
    public int Id { get; set; }
    [Required]
    public int MaterialId { get; set; }
    public Material Material { get; set; }
    [Required]
    public int PatronId { get; set; }
    public Patron Patron { get; set; }
    [Required]
    public DateTime? CheckoutDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool Paid { get; set;}
}