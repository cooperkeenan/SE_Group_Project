using System;
using System.Collections.Generic;
using Microcharts;

namespace EnviroMonitorApp.Services.ChartTransformers
{
    /// <summary>
    /// Transforms a series of timestamped values into a list of ChartEntry
    /// (handles binning, scaling, labels, etc).
    /// </summary>
    public interface IChartTransformer
    {
        /// <summary>
        /// Transforms raw timestamped data into chart entries suitable for visualization.
        /// </summary>
        /// <param name="raw">A sequence of (timestamp, value) pairs.</param>
        /// <param name="start">Chart start date.</param>
        /// <param name="end">Chart end date.</param>
        /// <returns>Ready-to-plot ChartEntry list.</returns>
        IList<ChartEntry> Transform(
            IEnumerable<(DateTime timestamp, double value)> raw,
            DateTime start,
            DateTime end);
    }
}