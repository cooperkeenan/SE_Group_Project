// ViewModels/BaseViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract class BaseViewModel : INotifyPropertyChanged
{
  public event PropertyChangedEventHandler PropertyChanged;
  protected void Raise([CallerMemberName] string name = null)
    => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
