using LibraryData.Models;
using System;
using System.Collections.Generic;

namespace LibraryServices
{
    class DataHelpers
    {
        public static IEnumerable<string> HumanizeBizHours(IEnumerable<BranchHours> branchHours)
        {
            var hours = new List<string>();
            foreach (var  time in branchHours)
            {
                var day = HumanizeDay(time.DayOfWeek);
                var openTime = HumanizeTime(time.OpenTime);
                var closeTime = HumanizeTime(time.CloseTime);

                var timeEntry = $"{day} {openTime} to {closeTime}";
                hours.Add(timeEntry);
            }

            return hours;
        }

        public static string HumanizeDay( int number)
        {
            // our data corretlates 1 to Sunday, so substract 1
            return Enum.GetName(typeof(DayOfWeek), number-1);
        }

        public static string HumanizeTime(int time)
        {
            return TimeSpan.FromHours(time).ToString("hh' : 'mm");
        }


    }
}
