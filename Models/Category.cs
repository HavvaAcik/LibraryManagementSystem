using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kategori Adı")]
        public string CategoryName { get; set; }

        [StringLength(200, ErrorMessage = "Kategori adı en fazla 200 karakter olabilir.")]
        [Display(Name = "Kategori Açıklaması")]
        public string? CategoryDescription { get; set; }
    }
}
