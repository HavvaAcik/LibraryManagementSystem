using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = " Kitap adı zorunludur. ")]
        [DisplayName("Kitap Adı")]
        public string Title { get; set; }

        [DisplayName("ISBN")]
        public string? ISBN { get; set; }

        [DisplayName("Yayın Yılı")]
        public int PublishedYear { get; set; }

        // Navigation 
        [DisplayName("Kategori")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [DisplayName("Kategori")]
        public Category? Category { get; set; }

        [DisplayName("Yazar")]
        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        [DisplayName("Yazar")]
        public Author? Author { get; set; }


    }
}
