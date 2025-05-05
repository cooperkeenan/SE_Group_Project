using EnviroMonitor.Core.Mvvm;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    /// <summary>
    /// Minimal subclass that exposes one property so we can test
    /// <see cref="ObservableObject.SetProperty{T}(ref T,T,string?)"/>.
    /// </summary>
    internal class TestObservable : ObservableObject
    {
        private int _value;

        /// <summary>Integer property whose setter uses <c>SetProperty</c>.</summary>
        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }

    /// <summary>
    /// Tests for the helper base‑class <see cref="ObservableObject"/>.
    /// </summary>
    public class ObservableObjectTests
    {
        /// <summary>
        /// When the value really changes, <see cref="ObservableObject.PropertyChanged"/>
        /// should fire exactly once.
        /// </summary>
        [Fact]
        public void SetProperty_RaisesPropertyChanged()
        {
            // arrange
            var obj   = new TestObservable();
            var fired = false;

            obj.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TestObservable.Value))
                    fired = true;
            };

            // act
            obj.Value = 42;

            // assert
            Assert.True(fired);
            Assert.Equal(42, obj.Value);
        }

        /// <summary>
        /// Setting the same value again should NOT raise
        /// <see cref="ObservableObject.PropertyChanged"/>.
        /// </summary>
        [Fact]
        public void SetProperty_DoesNotRaise_WhenSameValue()
        {
            // arrange
            var obj   = new TestObservable { Value = 5 };
            var fired = false;
            obj.PropertyChanged += (_, __) => fired = true;

            // act
            obj.Value = 5; // no actual change

            // assert
            Assert.False(fired);
        }
    }
}
