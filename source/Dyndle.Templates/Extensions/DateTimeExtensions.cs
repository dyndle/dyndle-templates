using System;
using System.Collections.Generic;
using System.Linq;

namespace Tridion.ContentManager.ContentManagement.Fields
{
    public static class DateTimeExtensions
    {
        public static string ToRFC822Date(this Nullable<DateTime> inputDate)
        {
            string formattedDate = inputDate.Value.ToString();
            const string RFC822DateFormat = "yyyyMMddHHmmss";
            if (DateTime.TryParse(formattedDate, out DateTime dt))
            {
                formattedDate = dt.ToString(RFC822DateFormat);
            }
            return formattedDate;
        }
    }
}
