using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spice.Models
{
    public class OrderDetail:EntityBase
    {
       
        public int Count { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Required]
        public double Price { get; set; }


        // Navigation Propertiies
        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual OrderHeader? OrderHeader { get; set; }

        [Required]
        public int MenuItemId { get; set; }
        [ForeignKey("MenuItemId")]
        public virtual MenuItem? MenuItem { get; set; }
    }
}
