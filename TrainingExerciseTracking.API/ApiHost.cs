using System.ComponentModel.DataAnnotations;
using TrainingExerciseTracking.API.DTOs;
using TrainingExerciseTracking.Database;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.API;

public delegate void APIAddedMovement(Movement movement);

public class ApiHost
{
    public static event APIAddedMovement OnAPIAddedMovement;
    
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
#if RELEASE            
            .ConfigureLogging(logging =>
            {
                // disable logging to console for performance reasons
                logging.ClearProviders();
                // TODO: add a logger
            })
#endif            
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.Configure(app =>
                {
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapPost("/Movements", async (MovementDTO movementDTO, HttpContext httpContext) =>
                        {
                            if (!httpContext.Request.HasJsonContentType()) return Results.BadRequest("Content type must be application/json.");
                            if (movementDTO is not { ParticipantNumber: { } number, Latitude: { } lat , Longitude: { } lon }) return Results.BadRequest("movement not valid.");
                            
                            await using var db = new TrainingDbContext();
                            Participant? participant = db.Participants.Where(p => p.Number == number).FirstOrDefault();
                            if (participant == null) return Results.BadRequest($"Participant with number {number} doesn't exist.");
                            
                            OnAPIAddedMovement?.Invoke(new Movement()
                            {
                                Longitude = lon,
                                Latitude = lat,
                                ParticipantId = participant.Id,
                                Participant = new Participant()
                                {
                                    Id = participant.Id,
                                    Number = participant.Number
                                }
                            });
                            return Results.Created("Created new movement.", movementDTO);
                        });
                        
                        endpoints.MapPost("/Participants", async (ParticipantDTO participantDTO, HttpContext httpContext) =>
                        {
                            if (!httpContext.Request.HasJsonContentType()) return Results.BadRequest("Content type must be application/json.");
                            if (participantDTO is not { Number: { } number, Country: { } country, Rank: { } rank }) return Results.BadRequest("participant not valid.");
                            
                            await using var db = new TrainingDbContext();
                            var participant = db.Participants.FirstOrDefault(p => p.Number == number);
                            if (participant != null) return Results.Conflict($"Participant with number {number} already exists.");
                                    
                            db.Participants.Add(new Participant()
                            {
                                Number = number,
                                Country = country,
                                Rank = rank,
                                Information = participantDTO.Information
                            });
                            await db.SaveChangesAsync();
                            return Results.Created("Created new participant.", participantDTO);
                        });
                    });
                });
            });
}