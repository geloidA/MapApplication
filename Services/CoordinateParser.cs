using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace map_app.Services
{
    public static class CoordinateParser // {1,32 3,212 5,212} {1,32 3,212 5,212} {1,32 3,212 5,212}
    {
        private static readonly string _pattern = @"(?<={).*?(?=})"; // {sd} {as} {} => sd as @Empty

        public static bool CanDataParsed(string? data, Func<int, bool> lengthPredicate)
        {            
            if (data is null)
                throw new NullReferenceException();

            var len = Regex.Matches(data, _pattern).Count();
            return lengthPredicate(len);
        }

        public static bool TryParseData<T>(string? data,
            Func<(double X, double Y, double Z), T> init, out IEnumerable<T> result)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<T> ParseData<T>(string? data, 
            Func<(double X, double Y, double Z), T> init) where T : new()
        {
            if (data is null)
                throw new NullReferenceException();

            return ParseIntoPointPieces(data)
                    .Select(ParseIntoDoublePoint)
                    .Select(init);
        }

        private static IEnumerable<string> ParseIntoPointPieces(string data) // {1 2 3} {1 3 5} => 1 2 3 | 1 3 5
        {
            var regexObj = new Regex(_pattern);
            var matchResults = regexObj.Match(data);
            while (matchResults.Success)
            {
                yield return matchResults.Value;
                matchResults = matchResults.NextMatch();
            }
        }

        private static (double X, double Y, double Z) ParseIntoDoublePoint(string data) // "1 2 3" => (1, 2, 3)
        {
            var splited = data.Split();
            if (splited.Length != 3)
                throw new ArgumentException("Неверное количество параметров точки");
            var values = ParseIntoDouble(splited).ToArray();
            return (values[0], values[1], values[2]);
        }

        private static IEnumerable<double> ParseIntoDouble(IEnumerable<string> data)
        {
            return data.Select(x => 
            {
                double result;
                if (double.TryParse(x, out result))
                    return result;
                throw new ArgumentException($"{x} не число");
            });
        }
    }
}