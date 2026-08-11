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

        [Required]
        public string DeviceId { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public string RequestDescription { get; set; } = string.Empty;

        public string RequestStatus { get; set; } = "Pending";

        public string? ApprovedByUserId { get; set; }

        public string? ApprovalNote { get; set; }

        // الفني الذي استلم الطلب
        public string? AssignedTechnicianId { get; set; }

        // وقت استلام الفني للطلب
        public DateTime? AssignedDate { get; set; }


        // Navigation Properties

        [ForeignKey("DeviceId")]
        public Device? Device { get; set; }

        public ICollection<MaintenanceLog> MaintenanceLogs { get; set; }
            = new List<MaintenanceLog>();

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();
    }
}