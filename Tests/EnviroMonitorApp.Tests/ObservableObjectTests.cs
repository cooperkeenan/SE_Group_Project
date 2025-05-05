using EnviroMonitor.Core.Mvvm;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    // Simple subclass to exercise SetProperty
    class TestObservable : ObservableObject
    {
        private int _value;
        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }

    public class ObservableObjectTests
    {
        [Fact]
        public void SetProperty_RaisesPropertyChanged()
        {
            var obj = new TestObservable();
            var fired = false;

            obj.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TestObservable.Value))
                    fired = true;
            };

            obj.Value = 42;

            Assert.True(fired);
            Assert.Equal(42, obj.Value);
        }

        [Fact]
        public void SetProperty_DoesNotRaise_WhenSameValue()
        {
            var obj = new TestObservable { Value = 5 };
            var fired = false;

            obj.PropertyChanged += (_, __) => fired = true;
            obj.Value = 5;

            Assert.False(fired);
        }
    }
}
