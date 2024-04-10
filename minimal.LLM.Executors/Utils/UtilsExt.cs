using System.Text.RegularExpressions;
using Quickenshtein;

namespace UtilsExt;

public record StringSegment(string Value, int Start, string Origin)
{
    public static implicit operator string(StringSegment Segment) => Segment.Value;
}

public static class EnumerableUtils 
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)       
       => self.Select((item, index) => (item, index));
    public static IEnumerable<Match> GetWordEndings(this string word, string pattern = @"[\W\s(\r\n|\r|\n)]", string exclude = @"[@\-_'$]") => 
        Regex.Matches(word, pattern).Where(x => !Regex.IsMatch(x.Value, exclude));
    
}

public static class LevenshteinUtils
{

    public static List<StringSegment> ToFlatCharacterStringMatrix(this string input) 
    {   
        List<StringSegment> output = new List<StringSegment>();
        input.ToLower().Chunk(1).WithIndex().ToList().ForEach(x =>{
            for(int j = 0; j < input.Length - x.index; j++)
                output.Add(new(input.Substring(x.index, j+1), x.index, input));
        });
        return output;
    } 

    public static bool LevenshteinMatch(this string target, string input, double tolerance = 0.65, bool caseInsentive = true)
    {   
        var caseInput = input;
        var caseTarget = target;
        if(caseInsentive)
        {
            caseInput = input.ToLower();
            caseTarget = target.ToLower();
        }
        var longest = target.Length > input.Length ? target.Length : input.Length;
        var distance = Levenshtein.GetDistance(caseInput, caseTarget, CalculationOptions.DefaultWithThreading);
        var res = (longest - distance) > longest * tolerance;
        return res;
    }

    public static List<StringSegment> FilterLevenshteinMatch(this List<StringSegment> strings, string input, double tolerance = 0.65, bool caseInsentive = true)
    {   
        List<StringSegment> filtered = strings.Where(x => ((string)x).LevenshteinMatch(input, tolerance, caseInsentive)).ToList();
        filtered.Sort((x, y) => CompareLevRank(x,y,input));
        return filtered;
    }
    
    public static int CompareLevRank(string x, string y, string input)
    {
        x = string.IsNullOrEmpty(x)? "": x;
        y = string.IsNullOrEmpty(y)? "": y;
        int xLev = Levenshtein.GetDistance(x, input, CalculationOptions.DefaultWithThreading);
        int yLev = Levenshtein.GetDistance(y, input, CalculationOptions.DefaultWithThreading); 
        return xLev.CompareTo(yLev);
    }

    public static string GetWordFromOrigin(this StringSegment stringSegment)
    {   
        var before = stringSegment.Origin.Substring(0, stringSegment.Start);
        var end = stringSegment.Start + ((string)stringSegment).Length;
        var after = stringSegment.Origin.Substring(end, stringSegment.Origin.ToString().Length - end);
        
        var prefix = "";
        var suffix = "";
        
        var reverse = new string(before.ToCharArray().Reverse().ToArray());
        List<Match> mxBefore = reverse.GetWordEndings().ToList();
        List<Match> mxAfter = after.GetWordEndings().ToList();

        if (mxBefore.Count > 0)
        {
            var remainder = reverse.Substring(0, mxBefore.First().Index);
            prefix = new string(remainder.Reverse().ToArray());
        }
        else
        {
            prefix = before;
        }
        
        if(mxAfter.Count > 0)
        {
            var remainder = after.Substring(0, mxAfter.First().Index);
            suffix = remainder;
        }
        else
        {
            suffix = after;
        }
        var segment = stringSegment.Origin.Substring(stringSegment.Start, stringSegment.Value.Length);
        return prefix + segment + suffix;
    }
}