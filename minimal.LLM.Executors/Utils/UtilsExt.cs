using Quickenshtein;

namespace UtilsExt;

public static class EnumerableUtils 
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)       
       => self.Select((item, index) => (item, index));
}

public record StringSegment(string Value, int Start)
{
    public static implicit operator string(StringSegment Segment) => Segment.Value;
}

public static class LevenshteinUtils
{

    public static List<StringSegment> ToFlatCharacterStringMatrix(this string input) 
    {   
        List<StringSegment> output = new List<StringSegment>();
        input.ToLower().Chunk(1).WithIndex().ToList().ForEach(x =>{
            for(int j = 0; j < input.Length - x.index; j++)
                output.Add(new(input.Substring(x.index, j+1), x.index));
        });
        return output;
    } 

    public static List<StringSegment> FilterLevenshteinTolerance(this List<StringSegment> strings, string input, double tolerance = 0.65)
    {   
        List<StringSegment> filtered = new List<StringSegment>();
        foreach(var item in strings)
        {
            var distance = Levenshtein.GetDistance(input, item, CalculationOptions.DefaultWithThreading);
            double matchRate = 1 - (double)distance / (double)input.Length;
            if(matchRate > tolerance) filtered.Add(item);
        }
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
}