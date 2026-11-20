using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseAPI.Entities
{
    public enum CourseStatus { Draft, Published, Cancelled }

    public class Course
    {
        public Guid Id { get; set; }


        [MaxLength(180)]
        public string Title { get; set; } = null!;

        
        [Column(TypeName = "text")]
        public string Description { get; set; } = null!;

        public DateTime StartsAtUtc { get; set; }
        public DateTime EndsAtUtc { get; set; }


        [MaxLength(180)]
        public string Location { get; set; } = null!;


        // Uint betyr at kapasiteten ikke kan bli negativ
        public uint Capacity { get; set; }

        public CourseStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }


        // Fremmednøkkel kurseier/bruker
        public Guid OwnerUserId { get; set; }
        public User Owner { get; set; } = null!;



    }
}
