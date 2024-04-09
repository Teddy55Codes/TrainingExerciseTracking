using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using TrainingExerciseTracking.API;
using TrainingExerciseTracking.Database;
using TrainingExerciseTracking.Services;

namespace TrainingExerciseTracking;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    /// <inheritdoc />
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        var args = Environment.GetCommandLineArgs().Select(arg => arg.ToLower());
        containerRegistry.RegisterSingleton<IParticipantMovementService, ParticipantMovementService>();
        containerRegistry.RegisterSingleton<IParticipantActivityGenerator, ParticipantActivityGenerator>();
        containerRegistry.RegisterSingleton<IParticipantMovementRecorder, ParticipantMovementRecorder>();
        
        // run database migrations
        using var db = new TrainingDbContext();
        db.Database.Migrate();
        
        if (args.Any(arg => arg is "--sample-data" or "-s"))
        {
            Task.Run(() =>
            {
                Container.Resolve<IParticipantMovementRecorder>();
                Container.Resolve<IParticipantActivityGenerator>().Start();
            });
        }
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var eventAggrigator = Container.Resolve<IEventAggregator>();
        ApiHost.OnAPIAddedMovement += dto =>
        {
            eventAggrigator.GetEvent<ParticipantMovementEvent>().Publish(dto);
        };

        // Start the minimal API in a background thread
        var _apiHost = ApiHost.CreateHostBuilder(null)
            .Build();
        
        _apiHost.Start();
        
        
        
        
    }

    /// <inheritdoc />
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
}