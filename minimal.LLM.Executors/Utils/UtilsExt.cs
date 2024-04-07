using Quickenshtein;

namespace UtilsExt;

public static class EnumerableUtils 
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)       
       => self.Select((item, index) => (item, index));
}

public static class LevenshteinUtils
{

    public static List<string> ToFlatCharacterStringMatrix(this string input) 
    {   
        List<string> output = new List<string>();
        input.ToLower().Chunk(1).WithIndex().ToList().ForEach(x =>{
            for(int j = 0; j < input.Length - x.index; j++)
                output.Add(input.Substring(x.index, j+1));
        });
        return output;
    } 

    public static List<string> FilterLevenshteinTolerance(this List<string> strings, string input, double tolerance = 0.65)
    {   
        List<string> filtered = new List<string>();
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