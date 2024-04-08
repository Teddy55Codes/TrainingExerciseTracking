﻿using System.ComponentModel.DataAnnotations;

namespace TrainingExerciseTracking.API.DTOs;

public class MovementDTO
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public virtual int ParticipantNumber { get; set; }
}