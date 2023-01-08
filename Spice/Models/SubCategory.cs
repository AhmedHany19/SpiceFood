using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Spice.Models
{
    public class SubCategory:EntityBase
    {
        [Display(Name = "SubCategory Name")]
        [Required]
        public string? Name { get; set; }



        [Required]
        [Display(Name = "Category Name")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public IEnumerable<MenuItem>? MenuItems { get; set; }


    }
}
