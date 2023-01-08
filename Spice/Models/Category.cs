using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Spice.Models
{
    public class Category: EntityBase
    {
        [Display(Name = "Category Name")]
        [Required]
        public string? Name { get; set; }


        public IEnumerable<MenuItem>? MenuItems { get; set; }
        public IEnumerable<SubCategory>? SubCategories { get; set; }

    }
}
