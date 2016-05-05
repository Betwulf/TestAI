using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace TestAICore
{
    /// <summary>
    /// Generic Extension Methods used throughout the code
    /// </summary>
    public static class ExtendCalculations
    {
        public static string DocIdFromDate(this DateTime date)
        {
            return date.ToString("yyyy.MM.dd");
        }

        public static DateTime DateFromDocId(this string id)
        {
            return DateTime.ParseExact(id, "yyyy.MM.dd", CultureInfo.CurrentCulture);
        }


        public static DateTime NextBusinessDay(this DateTime currDate)
        {
            var newDate = currDate.AddDays(1);
            if (newDate.DayOfWeek == DayOfWeek.Saturday) { newDate = newDate.AddDays(2); }
            if (newDate.DayOfWeek == DayOfWeek.Sunday) { newDate = newDate.AddDays(1); }
            return newDate;
        }


        public static double StandardDeviation(this IEnumerable<decimal> values)
        {
            decimal avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow((double)(v - avg), 2)));
        }


        /// <summary>
        /// Creates the CSV from a generic list.
        /// </summary>;
        /// <typeparam name="T"></typeparam>;
        /// <param name="list">The list.</param>;
        /// <param name="csvNameWithExt">Name of CSV (w/ path) w/ file ext.</param>;
        public static void CreateCSVFromGenericList<T>(this List<T> list, string csvCompletePath)
        {
            if (list == null || list.Count == 0) return;

            //get type from 0th member
            Type t = list[0].GetType();
            string newLine = Environment.NewLine;

            if (!Directory.Exists(Path.GetDirectoryName(csvCompletePath))) Directory.CreateDirectory(Path.GetDirectoryName(csvCompletePath));

            using (var sw = new StreamWriter(csvCompletePath))
            {
                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();

                //foreach of the properties in class above, write out properties
                //this is the header row
                sw.Write(string.Join(",", props.Select(d => d.Name).ToArray()) + newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    try
                    {
                        var row = string.Join(",", props.Select(d => item.GetType()
                                                                        .GetProperty(d.Name)
                                                                        .GetValue(item, null)
                                                                        .ToString())
                                                                .ToArray());
                        sw.Write(row + newLine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    //this acts as datacolumn

                }
            }
        }
    }
}
