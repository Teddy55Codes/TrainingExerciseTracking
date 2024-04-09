using System.ComponentModel.DataAnnotations;
using TrainingExerciseTracking.API.DTOs;
using TrainingExerciseTracking.Database;
using TrainingExerciseTracking.Database.Models;

namespace TrainingExerciseTracking.API;

public delegate void APIAddedMovement(Movement movement);

public class ApiHost
{
    private static ConcurrentDictionary<int, int> _participantNumberCache = new ConcurrentDictionary<int, int>();
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
                            if (movementDTO is not { ParticipantNumber: { } number, Latitude: { } lat and >= -90 and <= 90, Longitude: { } lon and >= -180 and <= 180 }) return Results.BadRequest("movement not valid.");

                            (int, int) participantIdAndNumber;
                            if (!_participantNumberCache.Keys.Contains(number))
                            {
                                await using var db = new TrainingDbContext();
                                var participant = db.Participants.FirstOrDefault(p => p.Number == number);
                                if (participant == null) return Results.BadRequest($"Participant with number {number} doesn't exist.");
                                _participantNumberCache.TryAdd(participant.Number, participant.Id);
                                participantIdAndNumber = (participant.Number, participant.Id);
                            }
                            else
                            {
                                participantIdAndNumber = (number, _participantNumberCache[number]);
                            }
                            
                            OnAPIAddedMovement?.Invoke(new Movement()
                            {
                                Longitude = lon,
                                Latitude = lat,
                                ParticipantId = participantIdAndNumber.Item2,
                                Participant = new Participant()
                                {
                                    Id = participantIdAndNumber.Item2,
                                    Number = participantIdAndNumber.Item1
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