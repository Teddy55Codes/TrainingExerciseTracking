namespace TrainingExerciseTracking.Database.Models;

public class Movement
{
    public long Id { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    
    public virtual int ParticipantId { get; set; }
    public virtual Participant Participant { get; set; }
}