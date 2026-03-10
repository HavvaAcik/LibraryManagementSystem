using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Loan
    {
        // kitap ödünç alma bölümü 

        [Key]
        public int LoanId { get; set; }

        // kitap 
        [Required]
        public int BookId { get; set; }
        public Book? Book { get; set; }

        //öğrenci 
        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Display(Name = "Kitap Alış Tarihi")]
        public DateTime LoanDate { get; set; } = DateTime.Now;

        [Display(Name = "Kitap Teslim Tarihi")]
        public DateTime ReturnDate { get; set; }

        [Display(Name = "Kitap Durumu")]
        public bool IsReturned { get; set; } = false;
    }
}
