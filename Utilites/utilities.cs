
namespace LibraryApi.Utilities
{
    public static class DateUtils
    {

        public static DateTime AddBusinessDays(DateTime start, int businessDays)
        {
            if (businessDays < 0)
                throw new ArgumentException("businessDays must be positive");

            DateTime current = start;
            int addedDays = 0;

            while (addedDays < businessDays)
            {
                current = current.AddDays(1);
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                    addedDays++;
            }

            return current;
        }
        public static bool IsValidTimeStep(DateTime time)
        {
            return time.Minute % 30 == 0
                && time.Second == 0
                && time.Millisecond == 0;
        }

        public static DateTime FloorTo30Minutes(DateTime time)
        {
            int minutes = (time.Minute / 30) * 30;

            return new DateTime(
                time.Year,
                time.Month,
                time.Day,
                time.Hour,
                minutes,
                0,
                0,
                time.Kind
            );
        }
        public static List<(DateTime Start, DateTime End)> GetAvailableIntervals(
            DateTime queryStart,
            DateTime queryEnd,
            List<Models.RoomReservationModel> reservations)
        {
            var result = new List<(DateTime, DateTime)>();
            var cursor = queryStart;

            foreach (var r in reservations)
            {
                if (r.StartTime > cursor)
                {
                    result.Add((cursor, r.StartTime));
                }

                // cursor = DateTime.Max(cursor, r.EndTime);
                cursor = new[] { cursor, r.EndTime }.Max();
            }

            if (cursor < queryEnd)
            {
                result.Add((cursor, queryEnd));
            }

            return result;
        }
    };
}


