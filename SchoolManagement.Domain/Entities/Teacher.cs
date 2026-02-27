using SchoolManagement.Domain.Entities.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Qualification { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<ApplicationUsers>? Users { get; set; }
    }
}