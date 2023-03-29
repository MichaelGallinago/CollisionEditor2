using ReactiveUI;
using System.ComponentModel;

namespace CollisionEditor2.ViewModels;

public class ViewModelBase : ReactiveObject
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
