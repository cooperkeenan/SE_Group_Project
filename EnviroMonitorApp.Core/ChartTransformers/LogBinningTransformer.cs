using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microcharts;
using SkiaSharp;

namespace EnviroMonitorApp.Services.ChartTransformers
{
    /// <summary>
    /// Chart transformer that bins points by day/week/month based on date range,
    /// applying logarithmic scaling to the Y-axis values.
    /// Limits the number of X-axis labels to improve readability.
    /// </summary>
    public class LogBinningTransformer : IChartTransformer
    {
        /// <summary>
        /// Maximum number of X-axis labels to display on the chart.
        /// </summary>
        public int MaxLabels { get; set; } = 12;

        /// <summary>
        /// Transforms raw timestamped data into chart entries with logarithmic Y-axis scaling
        /// and appropriate time-based binning based on the date range.
        /// </summary>
        /// <param name="raw">Raw timestamped data points to transform.</param>
        /// <param name="start">Start date of the chart range.</param>
        /// <param name="end">End date of the chart range.</param>
        /// <returns>A list of chart entries ready for visualization.</returns>
        public IList<ChartEntry> Transform(
            IEnumerable<(DateTime timestamp, double value)> raw,
            DateTime start,
            DateTime end)
        {
            // 1️⃣ Calculate span in days between start and end dates
            var spanDays = (end.Date - start.Date).TotalDays;

            // 2️⃣ Bin data points based on the time span
            IEnumerable<(DateTime timestamp, double value)> binned;
            if (spanDays <= 30)
            {
                // For short spans (≤30 days), use raw data points with no binning
                binned = raw.OrderBy(x => x.timestamp);
            }
            else if (spanDays <= 90)
            {
                // For medium spans (31-90 days), bin by day
                binned = raw
                    .GroupBy(x => x.timestamp.Date)
                    .Select(g => (
                        timestamp: g.Key,
                        value:     g.Average(x => x.value)))
                    .OrderBy(x => x.timestamp);
            }
            else if (spanDays <= 365)
            {
                // For large spans (91-365 days), bin by week
                binned = raw
                    .GroupBy(x => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        x.timestamp,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday))
                    .Select(g =>
                    {
                        var weekStart = g.Min(x => x.timestamp).Date;
                        return (timestamp: weekStart,
                                value:     g.Average(x => x.value));
                    })
                    .OrderBy(x => x.timestamp);
            }
            else
            {
                // For very large spans (>365 days), bin by month
                binned = raw
                    .GroupBy(x => new DateTime(x.timestamp.Year, x.timestamp.Month, 1))
                    .Select(g => (
                        timestamp: g.Key,
                        value:     g.Average(x => x.value)))
                    .OrderBy(x => x.timestamp);
            }

            var list = binned.ToList();
            if (list.Count == 0)
                return Array.Empty<ChartEntry>();

            // 3️⃣ Compute label step to avoid overcrowding
            int step = Math.Max(1, list.Count / MaxLabels);

            // 4️⃣ Build entries with log‐scaled Y values but real labels
            var entries = new List<ChartEntry>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var (timestamp, value) = list[i];
                float y = (float)Math.Log10(value + 1.0);

                entries.Add(new ChartEntry(y)
                {
                    ValueLabel = value.ToString("F1", CultureInfo.InvariantCulture),
                    Label      = (i % step == 0)
                                 ? timestamp.ToString(
                                     spanDays > 90 ? "MM/yy" : "MM/dd",
                                     CultureInfo.InvariantCulture)
                                 : string.Empty,
                    Color      = SKColor.Parse("#FF6200EE")
                });
            }

            return entries;
        }
    }
}