using System.ComponentModel.DataAnnotations;

namespace TestTools.Models
{
    public class ContactFormModel
    {
        public class ContactFormInput
        {
            [Required]
            [StringLength(100)]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100)]
            public string Subject { get; set; }

            [Required]
            [StringLength(1000)]
            public string Message { get; set; }
        }
    }
}
