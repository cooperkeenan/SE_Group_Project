using EnviroMonitor.Core.Services;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class EnvironmentalDataServiceTests
    {
        [Fact]
        public void DoNothing_DoesNotThrow()
        {
            var svc = new EnvironmentalDataService(new ExcelReaderService());
            svc.DoNothing();   // simply should not throw
        }
    }
}
