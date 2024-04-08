using System.ComponentModel.DataAnnotations;

namespace TrainingExerciseTracking.API.DTOs;

public class ParticipantDTO
{
    public int Number { get; set; }
    public string Rank { get; set; }
    public string Country { get; set; }
    public string Information { get; set; }
}