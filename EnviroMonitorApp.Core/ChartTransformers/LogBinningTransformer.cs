using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microcharts;
using SkiaSharp;

namespace EnviroMonitorApp.Services.ChartTransformers
{
    /// <summary>
    /// Approximate max number of X‐axis labels to draw.
    /// Bins points by day/week/month and log‐scales Y.
    /// </summary>
    public class LogBinningTransformer : IChartTransformer
    {
        public int MaxLabels { get; set; } = 12;

        public IList<ChartEntry> Transform(
            IEnumerable<(DateTime timestamp, double value)> raw,
            DateTime start,
            DateTime end)
        {
            // 1️⃣ span in days
            var spanDays = (end.Date - start.Date).TotalDays;

            // 2️⃣ bin by span
            IEnumerable<(DateTime timestamp, double value)> binned;
            if (spanDays <= 30)
            {
                binned = raw.OrderBy(x => x.timestamp);
            }
            else if (spanDays <= 90)
            {
                binned = raw
                    .GroupBy(x => x.timestamp.Date)
                    .Select(g => (
                        timestamp: g.Key,
                        value:     g.Average(x => x.value)))
                    .OrderBy(x => x.timestamp);
            }
            else if (spanDays <= 365)
            {
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

            // 3️⃣ compute label step
            int step = Math.Max(1, list.Count / MaxLabels);

            // 4️⃣ build entries with log‐scaled Y but real labels
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
