using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Author
    {
        [Key]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Yazar adı boş bırakılamaz.")]
        [DisplayName("Yazar Adı Soyadı")]
        public string AuthorFullName { get; set; }

        [DisplayName("Doğum Yılı")]
        public int BirthYear { get; set; }

        [DisplayName("Biyaografi")]
        public string? Biography { get; set; }

        //public List<Book>Books? {get; set;
    }
}
