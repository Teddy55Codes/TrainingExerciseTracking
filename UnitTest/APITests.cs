using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using TrainingExerciseTracking.API.DTOs;
using Xunit.Abstractions;

namespace UnitTest;

[CollectionDefinition(nameof(APITests), DisableParallelization = true)]
public class APITests
{
    private const string BaseUrl = "http://localhost:5000";
    private const int SoldierCount = 100;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClient = new();
    private readonly Random _random = new();
    
    public APITests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

#region Setup
    [Fact]
    public async Task CreateParticipantsIfNotExists()
    {
        for (int i = 0; i < SoldierCount; i++)
        {
            var result = await _httpClient.PostAsync(
                BaseUrl + "/participants", 
                new StringContent(
                    JsonSerializer.Serialize(new ParticipantDTO()
                    {
                        Number = i+1, 
                        Country = "TestCountry", 
                        Rank = "TestRank", 
                        Information = "TestInformation"
                    }), 
                    Encoding.UTF8, "application/json"));
            Assert.True(result.StatusCode is HttpStatusCode.Created or HttpStatusCode.Conflict);
        }
        
    }
#endregion
    
#region Performance
    [Theory]
    [InlineData(1, 100)]
    [InlineData(50, 200)]
    [InlineData(100, 500)]
    public async Task MovementsStressTest(int simultaneousConnections, int numberOfRequestsPerConnection)
    {
        const double latitudeMin = -85;
        const double latitudeMax = 85;
        const double longitudeMin = -180;
        const double longitudeMax = 180;
        const string requestUrl = BaseUrl + "/movements";
        
        var sw = new Stopwatch();
        var tasks = new List<Task>();
        sw.Start();
        for (int i = 0; i < simultaneousConnections; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < numberOfRequestsPerConnection; j++)
                {
                    var response = await _httpClient.PostAsync(requestUrl, 
                        new StringContent(
                            JsonSerializer.Serialize(new MovementDTO()
                            {
                                ParticipantNumber = _random.Next(1, SoldierCount+1), 
                                Latitude = _random.NextDouble() * (latitudeMax - latitudeMin) + latitudeMin, 
                                Longitude = _random.NextDouble() * (longitudeMax - longitudeMin) + longitudeMin
                            }), Encoding.UTF8, "application/json"));
                    Assert.True(response.IsSuccessStatusCode, $"One of the requests in {nameof(MovementsStressTest)} did not succeed.");
                }
            }));
        }
        await Task.WhenAll(tasks);
        _testOutputHelper.WriteLine(sw.Elapsed.ToString());
    }
#endregion

#region Validation

    

#endregion
}