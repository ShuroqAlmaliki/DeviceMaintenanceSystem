using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceMaintenanceSystem.Models
{
    public class MaintenanceRequest
    {
        [Key]
        public int RequestId { get; set; }


        [Required]
        public string UserId { get; set; } = string.Empty;


        // الجهاز المسجل سابقًا - اختياري
        public string? DeviceId { get; set; }


        // اسم الجهاز الذي يكتبه صاحب الطلب
        [Required]
        public string DeviceName { get; set; } = string.Empty;


        public DateTime RequestDate { get; set; } = DateTime.Now;


        [Required]
        public string RequestDescription { get; set; } = string.Empty;


        public string RequestStatus { get; set; } = "Pending";


        public string? ApprovedByUserId { get; set; }


        public string? ApprovalNote { get; set; }


        // الفني الذي استلم الطلب
        public string? AssignedTechnicianId { get; set; }


        // وقت استلام الفني للطلب
        public DateTime? AssignedDate { get; set; }


        // =========================================
        // APPLICANT INFORMATION
        // =========================================

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Please enter your phone number")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string ApplicantPhone { get; set; } = string.Empty;


        [Display(Name = "Office Number")]
        [Required(ErrorMessage = "Please enter the office number")]
        public string OfficeNumber { get; set; } = string.Empty;


        [Display(Name = "Department")]
        [Required(ErrorMessage = "Please enter the department")]
        public string Department { get; set; } = string.Empty;


        [Display(Name = "Building")]
        [Required(ErrorMessage = "Please enter the building")]
        public string Building { get; set; } = string.Empty;


        // =========================================
        // NAVIGATION PROPERTIES
        // =========================================

        [ForeignKey("DeviceId")]
        public Device? Device { get; set; }


        public ICollection<MaintenanceLog> MaintenanceLogs { get; set; }
            = new List<MaintenanceLog>();


        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();
    }
}