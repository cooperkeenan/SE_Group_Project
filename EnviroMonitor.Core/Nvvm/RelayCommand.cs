// File: Mvvm/RelayCommand.cs
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EnviroMonitor.Core.Mvvm
{
    /// <summary>
    /// A small helper that lets you bind any async method to a UI control.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Typical XAML usage:
    /// <code language="xml">
    /// &lt;Button Content="Save"
    ///         Command="{Binding SaveCommand}" /&gt;
    /// </code>
    /// </para>
    /// </remarks>
    public class RelayCommand : ICommand
    {
        // -----------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------

        /// <summary>The async action to run when the command is executed.</summary>
        private readonly Func<Task> _execute;

        /// <summary>
        /// Optional function that decides whether the command is currently
        /// allowed to run (e.g. “IsFormValid == true”).
        /// </summary>
        private readonly Func<bool>? _canExecute;

        // -----------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">
        /// The async delegate to run.  
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <param name="canExecute">
        /// Optional delegate that returns <see langword="true"/> when the
        /// command should be enabled.  
        /// If omitted, the command is always enabled.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="execute"/> is <see langword="null"/>.
        /// </exception>
        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // -----------------------------------------------------------------
        // ICommand implementation
        // -----------------------------------------------------------------

        /// <summary>
        /// Checks whether the command can run.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        /// <returns>
        /// The result of <see cref="_canExecute"/> when supplied;
        /// otherwise <see langword="true"/>.
        /// </returns>
        public bool CanExecute(object? parameter) =>
            _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Executes the async delegate.  
        /// Any exceptions are ignored; handle them inside your delegate.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        public async void Execute(object? parameter) => await _execute();

        /// <summary>
        /// Event that UI frameworks listen to in order to re‑query
        /// <see cref="CanExecute"/>.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        // -----------------------------------------------------------------
        // Public helpers
        // -----------------------------------------------------------------

        /// <summary>
        /// Forces the UI to ask again whether the command is enabled.
        /// Call this after something that affects <see cref="_canExecute"/>
        /// changes (for example, when form validation state flips).
        /// </summary>
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
