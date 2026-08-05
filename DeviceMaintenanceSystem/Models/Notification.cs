using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceMaintenanceSystem.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int RequestId { get; set; }

        public string NotificationDescription { get; set; } = string.Empty;

        public DateTime NotificationDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Navigation Property
        [ForeignKey("RequestId")]
        public MaintenanceRequest? MaintenanceRequest { get; set; }
    }
}