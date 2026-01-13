using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Models;

[Table("Patient", Schema = "Healthcare")]
public class Patient
{
    [Key]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z]+([ '-][a-zA-Z]+)*$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z]+([ '-][a-zA-Z]+)*$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [MaxLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
    public string? Gender { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Navigation property
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
