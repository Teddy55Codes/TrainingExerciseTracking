using System.Windows;
using Mapsui;
using Prism.Events;
using TrainingExerciseTracking.Services;
using TrainingExerciseTracking.ViewModels;

namespace TrainingExerciseTracking;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(IEventAggregator eventAggregator, IParticipantMovementService participantMovementService)
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(eventAggregator, participantMovementService, MyMapControl);
    }

    private void MyMapControl_Info(object? sender, MapInfoEventArgs e) => ((MainWindowViewModel)DataContext).MyMapControl_Info(sender, e);
}