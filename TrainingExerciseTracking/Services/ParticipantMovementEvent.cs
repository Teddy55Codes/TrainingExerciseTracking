using Prism.Events;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.Services;

public class ParticipantMovementEvent : PubSubEvent<Movement>
{
    
}