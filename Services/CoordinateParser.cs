using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace map_app.Services
{
    public static class CoordinateParser // {1,32 3,212 5,212} {1,32 3,212 5,212} {1,32 3,212 5,212}
    {
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
            var regexObj = new Regex(@"(?<={)\b[0-9 ]+\b(?=})"); // todo: doesn't work with double values
            var matchResults = regexObj.Match(data);
            while (matchResults.Success)
            {
                yield return matchResults.Value;
                matchResults = matchResults.NextMatch();
            }
        }

        private static (double X, double Y, double Z) ParseIntoDoublePoint(string data) // 1 2 3 => (1, 2, 3)
        {
            var splited = data.Split();
            if (splited.Length != 3)
                throw new ArgumentException("Incorrect number parameters");
            return (double.Parse(splited[0]), double.Parse(splited[1]), double.Parse(splited[2]));
        }        
    }
}