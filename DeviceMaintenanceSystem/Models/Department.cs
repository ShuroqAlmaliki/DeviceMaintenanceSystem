using System.ComponentModel.DataAnnotations;

namespace DeviceMaintenanceSystem.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        public string DepartmentName { get; set; } = string.Empty;

        public int? HeadUserID { get; set; }

        public string? HeadUserName { get; set; }

        // Navigation Properties
        public ICollection<Device> Devices { get; set; }
            = new List<Device>();
    }
}