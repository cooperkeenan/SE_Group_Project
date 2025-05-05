using System.Threading.Tasks;
using EnviroMonitor.Core.Mvvm;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class RelayCommandTests
    {
        [Fact]
        public void CanExecute_DefaultsTrue()
        {
            var cmd = new RelayCommand(() => Task.CompletedTask);
            Assert.True(cmd.CanExecute(null));
        }

        [Fact]
        public void CanExecute_HonorsProvidedFunc()
        {
            var cmd = new RelayCommand(() => Task.CompletedTask, () => false);
            Assert.False(cmd.CanExecute(null));
        }

        [Fact]
        public async Task Execute_InvokesAsyncDelegate()
        {
            // Use a TaskCompletionSource so the test waits precisely
            var tcs = new TaskCompletionSource();

            var cmd = new RelayCommand(async () =>
            {
                await Task.Delay(1);
                tcs.SetResult();
            });

            cmd.Execute(null);
            await tcs.Task;                         // wait until delegate finishes

            Assert.True(tcs.Task.IsCompletedSuccessfully);
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var cmd = new RelayCommand(() => Task.CompletedTask);
            var fired = false;

            cmd.CanExecuteChanged += (_, __) => fired = true;
            cmd.RaiseCanExecuteChanged();

            Assert.True(fired);
        }
    }
}
