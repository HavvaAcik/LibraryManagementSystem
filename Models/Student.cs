using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Öğrenci adı boş bırakılamaz.")]
        [Display(Name = "Öğrenci Adı")]
        public string StudentFullName { get; set; }

        [Required(ErrorMessage = "Öğrenci numarası boş bırakılamaz.")]
        [Display(Name = "Öğreci Numarası")]
        public string StudentNumber { get; set; }

        [Display(Name = "Bölüm")]
        public string Department { get; set; }

        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; }

        public ICollection<Loan>? Loans { get; set; }

    }
}
