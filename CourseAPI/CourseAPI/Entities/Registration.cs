

using System.ComponentModel.DataAnnotations.Schema;

namespace CourseAPI.Entities
{
    public class Registration
    {
        public Guid Id { get; set; }
        public DateTime RegisteredAtUtc { get; set; }


        // CancelledAtUtc må være nullable
        public DateTime? CancelledAtUtc { get; set; }


        // En deltaker kan ikke ha to aktive påmeldinger til samme kurs
        public Guid? ActiveParticipantUserId { get; set; }


        // Fremmednøkkel deltaker/bruker
        public Guid ParticipantUserId { get; set; }
        public User Participant { get; set; } = null!;


        // Fremmednøkkel kurs
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;


    }
}
