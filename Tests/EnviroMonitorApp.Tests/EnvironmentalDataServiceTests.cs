using EnviroMonitor.Core.Services;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    /// <summary>
    /// Unit tests for <see cref="EnvironmentalDataService"/>.
    /// </summary>
    public class EnvironmentalDataServiceTests
    {
        /// <summary>
        /// Makes sure that calling <see cref="EnvironmentalDataService.DoNothing"/>
        /// really does nothing and—most importantly—does not throw.
        /// </summary>
        [Fact]
        public void DoNothing_DoesNotThrow()
        {
            // arrange
            var svc = new EnvironmentalDataService(new ExcelReaderService());

            // act + assert
            svc.DoNothing();  // if this line throws, the test will fail
        }
    }
}
