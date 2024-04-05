using Microsoft.EntityFrameworkCore;
using TrainingExerciseTracking.Database;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.Services;

public class ParticipantMovementRecorder : IParticipantMovementRecorder
{
    public ParticipantMovementRecorder(IParticipantMovementService participantMovementService)
    {
        using var db = new TrainingDbContext();
        db.Database.Migrate();
        participantMovementService.OnBatchCollected += SaveBadgeToDB;
    }

    private void SaveBadgeToDB(Movement[] movements)
    {
        using var db = new TrainingDbContext();
        var participantNumbers = movements.Select(m => m.Participant.Number);
        var participants = db.Participants.Where(p => participantNumbers.Contains(p.Number)).ToList();
        
        foreach (var movement in movements)
        {
            var participantId = participants.FirstOrDefault(p => p.Number == movement.Participant.Number)?.Id;
            if (participantId != null)
            {
                movement.ParticipantId = (int)participantId;
                movement.Participant = null;
            }
        }
        db.Movements.AddRange(movements);
        db.SaveChanges();
    }
}