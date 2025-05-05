// File: Nvvm/RelayCommand.cs
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EnviroMonitor.Core.Mvvm
{
    public class RelayCommand : ICommand
    {
        readonly Func<Task> _execute;
        readonly Func<bool>? _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) 
            => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter) 
            => await _execute();

        // MAUI/NET 8 doesn’t have WPF’s CommandManager, so we raise manually:
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Call this if your can‐execute logic changes at runtime.
        /// </summary>
        public void RaiseCanExecuteChanged() 
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
