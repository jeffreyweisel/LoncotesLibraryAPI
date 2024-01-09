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
        if (CheckoutDate == null || Material == null || Material.MaterialType == null)
        {
            return null; // Return null if necessary data is not available
        }

        DateTime dueDate = CheckoutDate.Value.AddDays(Material.MaterialType.CheckoutDays);
        DateTime returnDate = ReturnDate ?? DateTime.Today;
        int daysLate = (returnDate - dueDate).Days;

        if (daysLate > 0)
        {
            decimal fee = daysLate * _lateFeePerDay;
            return fee;
        }
        else
        {
            return null;
        }
    }
}
    public bool Paid { get; set; }

}