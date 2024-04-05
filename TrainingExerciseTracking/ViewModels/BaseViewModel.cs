using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestingTool.Ui;

public abstract class BaseViewModel : ObservableObject
{
    internal void RaisePropertyChanged([CallerMemberName] string name = null) => OnPropertyChanged(name);
}
