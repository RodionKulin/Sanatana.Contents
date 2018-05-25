using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents
{
    public static class DateTimeExtensions
    {
        public static string ToIso8601(this DateTime? dateTime)
        {
            return dateTime == null
                ? null
                : dateTime.Value.ToIso8601();
        }

        public static string ToIso8601(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Local)
                dateTime = dateTime.ToUniversalTime();

            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.ffff") + "Z";
        }

        public static bool TryParseIso8601(this string input, out DateTime result)
        {
            return DateTime.TryParseExact(input, "yyyy-MM-ddTHH:mm:ss.ffffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
                , out result);
        }

        public static bool TryParseIso8601(this string input, out DateTime? result)
        {
            if(input == null)
            {
                result = null;
                return true;
            }

            DateTime resultParsed;
            bool completed = input.TryParseIso8601(out resultParsed);

            result = completed ? (DateTime?)resultParsed : null;
            return completed;
        }
    }
}
