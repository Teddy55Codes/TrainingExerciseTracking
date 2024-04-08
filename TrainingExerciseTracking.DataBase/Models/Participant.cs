namespace TrainingExerciseTracking.Database.Models;

public class Participant
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string Rank { get; set; }
    public string Country { get; set; }
    public string Information { get; set; }
    
    public virtual  List<Movement> Movements { get; set; }
}