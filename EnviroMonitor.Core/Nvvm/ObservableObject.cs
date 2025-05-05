using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnviroMonitor.Core.Mvvm
{
    /// <summary>
    /// Base class that implements <see cref="INotifyPropertyChanged"/> for
    /// simple MVVM view‑models and DTOs.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the supplied
        /// <paramref name="name"/> (defaults to the caller member).
        /// </summary>
        /// <param name="name">Property name.  Automatically supplied by
        /// <see cref="CallerMemberNameAttribute"/> when omitted.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Assigns <paramref name="value"/> to <paramref name="field"/>
        /// if it is different and raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <typeparam name="T">Type of the backing field.</typeparam>
        /// <param name="field">Reference to the backing field.</param>
        /// <param name="value">New value requested by the caller.</param>
        /// <param name="name">Property name (inferred automatically).</param>
        /// <returns>
        /// <see langword="true"/> if the value actually changed;  
        /// <see langword="false"/> if the incoming value equals the
        /// current one (no event is raised in that case).
        /// </returns>
        protected bool SetProperty<T>(ref T field, T value,
            [CallerMemberName] string? name = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
