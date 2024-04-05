using System.Configuration;
using System.Data;
using System.Windows;
using Prism.Ioc;
using Prism.Unity;
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
        Task.Run(() =>
        {
            Container.Resolve<IParticipantActivityGenerator>().Start();
        });

    }

    /// <inheritdoc />
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
}