using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceMaintenanceSystem.Models
{
    public class MaintenanceLog
    {
        [Key]
        public int LogId { get; set; }

        public int RequestId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string RepairDetails { get; set; } = string.Empty;

        // قيمة افتراضية
        public DateTime RepairStartDate { get; set; } = DateTime.Now;

        public DateTime RepairEndDate { get; set; }

        public string DeviceStatusAfterRepair { get; set; } = string.Empty;

        // Navigation Property
        [ForeignKey("RequestId")]
        public MaintenanceRequest MaintenanceRequest { get; set; }
    }
}