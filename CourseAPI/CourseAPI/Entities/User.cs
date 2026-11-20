using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseAPI.Entities
{
    public enum UserRole {Participant, Instructor}

    public class User
    {
        public Guid Id { get; set;  }

        
        [MaxLength(254)]
        public string Email { get; set; } = null!;


        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;


        [MaxLength(120)]
        public string DisplayName { get; set; } = null!;


        public UserRole Role { get; set; }

        public DateTime CreatedAtUtc { get; set; }




    }
}
