using System.Net.NetworkInformation;

namespace Fina.Core.Common;

public static class DateTimeExtension
{
    public static DateTime GetFirstDay(this DateTime date, int? year = null, int? month = null) 
        => new DateTime(year: (year ?? date.Year),month: (month ?? date.Month), day: 1);

    public static DateTime GetLastDay(this DateTime date, int? year = null, int? month = null)
        => new DateTime(year: (year ?? date.Year), month: (month ?? date.Month), day: 1)
               .AddMonths(1)
               .AddDays(-1);
}
