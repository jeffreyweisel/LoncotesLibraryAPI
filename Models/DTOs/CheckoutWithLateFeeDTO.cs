using System.ComponentModel.DataAnnotations;


namespace Library.Models.DTOs;

public class CheckoutWithLateFeeDTO
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
    private static decimal _lateFeePerDay = 0.50M;
    public decimal? LateFee
    {
        get
        {
            DateTime dueDate = CheckoutDate.Value.AddDays(Material.MaterialType.CheckoutDays);
            DateTime returnDate = ReturnDate ?? DateTime.Today;
            int daysLate = (returnDate - dueDate).Days;
            decimal fee = daysLate * _lateFeePerDay;
            return daysLate > 0 ? fee : null;
        }
    }

}