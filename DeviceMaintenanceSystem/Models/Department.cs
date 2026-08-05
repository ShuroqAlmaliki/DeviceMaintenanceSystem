using System.ComponentModel.DataAnnotations;

namespace DeviceMaintenanceSystem.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        public string DepartmentName { get; set; } = string.Empty;

        public int? HeadUserId { get; set; }

        // Navigation Properties
        public ICollection<Device> Devices { get; set; }
    }
}