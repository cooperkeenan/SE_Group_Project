// EnviroMonitorApp/Services/ChartTransformers/LogBinningTransformer.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microcharts;
using SkiaSharp;

namespace EnviroMonitorApp.Services.ChartTransformers
{
    public class LogBinningTransformer : IChartTransformer
    {
        /// <summary>
        /// Approximate maximum number of X‐axis labels to draw.
        /// </summary>
        public int MaxLabels { get; set; } = 12;

        public IList<ChartEntry> Transform(
            IEnumerable<(DateTime timestamp, double value)> raw,
            DateTime start,
            DateTime end)
        {
            // 1️⃣ Determine span
            var spanDays = (end.Date - start.Date).TotalDays;

            // 2️⃣ Bin the raw points based on span
            IEnumerable<(DateTime timestamp, double value)> binned;
            if (spanDays <= 30)
            {
                // no binning
                binned = raw.OrderBy(x => x.timestamp);
            }
            else if (spanDays <= 90)
            {
                // daily average
                binned = raw
                    .GroupBy(x => x.timestamp.Date)
                    .Select(g => (
                        timestamp: g.Key,
                        value:     g.Average(x => x.value)))
                    .OrderBy(x => x.timestamp);
            }
            else if (spanDays <= 365)
            {
                // weekly average (group by ISO week number)
                binned = raw
                    .GroupBy(x => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        x.timestamp,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday))
                    .Select(g =>
                    {
                        // pick the earliest date in the week as label
                        var weekStart = g.Min(x => x.timestamp).Date;
                        return (timestamp: weekStart,
                                value:     g.Average(x => x.value));
                    })
                    .OrderBy(x => x.timestamp);
            }
            else
            {
                // monthly average
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

            // 3️⃣ Compute label step so we only show ~MaxLabels labels
            int step = Math.Max(1, list.Count / MaxLabels);

            // 4️⃣ Build entries, log‐transforming Y but showing real value in the label
            var entries = new List<ChartEntry>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var (timestamp, value) = list[i];
                float y = (float)Math.Log10(value + 1.0);

                entries.Add(new ChartEntry(y)
                {
                    // display the real average, not the log
                    ValueLabel = value.ToString("F1", CultureInfo.InvariantCulture),
                    // only show every Nth X-label
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
