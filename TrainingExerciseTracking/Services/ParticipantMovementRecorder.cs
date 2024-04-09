﻿using Microsoft.EntityFrameworkCore;
using TrainingExerciseTracking.Database;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.Services;

public class ParticipantMovementRecorder : IParticipantMovementRecorder
{
    public ParticipantMovementRecorder(IParticipantMovementService participantMovementService)
    {
        participantMovementService.OnBatchCollected += SaveBadgeToDB;
    }

    private void SaveBadgeToDB(Movement[] movements)
    {
        using var db = new TrainingDbContext();
        var actualMovements = movements.Where(m => m.Id == 0);
        var participantNumbers = actualMovements.Select(m => m.Participant.Number);
        var participants = db.Participants.Where(p => participantNumbers.Contains(p.Number)).ToList();
        
        foreach (var movement in actualMovements)
        {
            var participantId = participants.FirstOrDefault(p => p.Number == movement.Participant.Number)?.Id;
            if (participantId != null)
            {
                movement.ParticipantId = (int)participantId;
                movement.Participant = null;
            }
        }
        db.Movements.AddRange(actualMovements);
        db.SaveChanges();
    }
}