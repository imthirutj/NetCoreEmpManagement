using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Model
{
    public class Employee
    {
        [Key] // Specify the [Key] attribute to define the primary key
        public int Id { get; set; }

        // Other properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string username { get; set; }

        public string password { get; set; }
    }

}
