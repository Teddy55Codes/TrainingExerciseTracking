using System.Collections.Concurrent;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.Services;

public interface IParticipantMovementService
{
    public ConcurrentBag<Movement> Movements { get; }
    
    public event BatchCollected OnBatchCollected;
}