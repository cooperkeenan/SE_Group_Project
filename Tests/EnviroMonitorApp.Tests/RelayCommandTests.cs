using System.Threading.Tasks;
using EnviroMonitor.Core.Mvvm;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    /// <summary>
    /// Tests for <see cref="RelayCommand"/>.
    /// </summary>
    public class RelayCommandTests
    {
        /// <summary>
        /// When no <c>canExecute</c> predicate is supplied,
        /// <see cref="RelayCommand.CanExecute"/> should return <c>true</c>.
        /// </summary>
        [Fact]
        public void CanExecute_DefaultsTrue()
        {
            var cmd = new RelayCommand(() => Task.CompletedTask);

            Assert.True(cmd.CanExecute(null));
        }

        /// <summary>
        /// If a predicate is provided, its boolean value must be respected.
        /// </summary>
        [Fact]
        public void CanExecute_HonorsProvidedFunc()
        {
            var cmd = new RelayCommand(() => Task.CompletedTask, () => false);

            Assert.False(cmd.CanExecute(null));
        }

        /// <summary>
        /// Executing the command should run the async delegate exactly once.
        /// </summary>
        [Fact]
        public async Task Execute_InvokesAsyncDelegate()
        {
            // use TaskCompletionSource to wait deterministically
            var tcs = new TaskCompletionSource();

            var cmd = new RelayCommand(async () =>
            {
                await Task.Delay(1);
                tcs.SetResult();
            });

            cmd.Execute(null);
            await tcs.Task;

            Assert.True(tcs.Task.IsCompletedSuccessfully);
        }

        /// <summary>
        /// Calling <see cref="RelayCommand.RaiseCanExecuteChanged"/> should
        /// fire the event so that UI frameworks re‑query <c>CanExecute</c>.
        /// </summary>
        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var cmd   = new RelayCommand(() => Task.CompletedTask);
            var fired = false;

            cmd.CanExecuteChanged += (_, __) => fired = true;

            cmd.RaiseCanExecuteChanged();

            Assert.True(fired);
        }
    }
}
