using System.ComponentModel.DataAnnotations;

namespace Spice.Models
{
    public class Coupon:EntityBase
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? CouponType { get; set; }
        public enum ECouponType { Percent = 0, Dollar = 1 }
        [Required]
        public double Discount { get; set; }
        [Required]
        [Display(Name=("Minimum Amount"))]
        public double MinimumAmount { get; set; }
        public byte[]? Picture { get; set; }
        [Display(Name=("Is Active"))]
        public bool IsActive { get; set; }
    }
}
