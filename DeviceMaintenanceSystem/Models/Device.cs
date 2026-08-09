using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceMaintenanceSystem.Models
{
    public class Device
    {
        [Key]
        public string DeviceId { get; set; } = string.Empty;

        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string DeviceStatus { get; set; } = string.Empty;
        public string BarcodeValue { get; set; } = string.Empty;
        public int DepartmentId { get; set; }

        // Navigation Properties
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; }
    }
}