namespace UtilsExt;

public static class UtilsExt 
{
    public static List<string> SplitChunk(this string str, int chunkSize)
    {   
        var res = Enumerable.Range(0, str.Length / chunkSize)
            .Select(i => str.Substring(i * chunkSize, chunkSize)).ToList();

        var remainder = str.Count() % chunkSize;

        
        string end = "";

        if(remainder > 0) 
        {   
            end = str.Substring(Math.Max(0, str.Length - remainder));
            res.Add(end);
        }
        
        return res.ToList();
    }
    public static int toInt(this bool b) => b? 1:-1;
    public static List<string> SortByBool (this List<string> strings, Func <string, string, string, bool> function, string question) 
    {
        var res = strings;
        res.Sort((a,b) => function(a, b, question).toInt());
        return res;
    }

}