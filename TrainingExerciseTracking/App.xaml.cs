using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using TrainingExerciseTracking.API;
using TrainingExerciseTracking.Database.Models;
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
        containerRegistry.Register<MainWindow>();
        containerRegistry.RegisterSingleton<IParticipantMovementService, ParticipantMovementService>();
        containerRegistry.RegisterSingleton<IParticipantActivityGenerator, ParticipantActivityGenerator>();
        containerRegistry.RegisterSingleton<IParticipantMovementRecorder, ParticipantMovementRecorder>();
        Task.Run(() =>
        {
            Container.Resolve<IParticipantMovementRecorder>();
            Container.Resolve<IParticipantActivityGenerator>().Start();
        });
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