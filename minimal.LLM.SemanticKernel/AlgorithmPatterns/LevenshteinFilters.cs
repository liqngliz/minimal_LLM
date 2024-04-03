using Quickenshtein;
using UtilsExt;

namespace minimal.LLM.SemanticKernel;

public static class LevenshteinFilters
{

    public static List<string> ToFlatCharacterStringMatrix(this string input) 
    {   
        List<string> output = new List<string>();
        var chunks = input.ToLower().Chunk(1).ToList();
        for(int i = 0; i < chunks.Count; i++)
        {
            int remainder = input.Length - i;
            for(int j = 0; j < remainder; j++)
            {
                var stringSet = input.Substring(i, j+1);
                output.Add(stringSet);
            }
        }
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
