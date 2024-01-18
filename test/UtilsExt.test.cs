
using UtilsExt;

namespace UtilsExtTest;

public class UtilsExtTest
{
    [Fact]
    public void should_split_to_chunks()
    {
        string text = "ID8ZSOB2M032";
        var res = text.SplitChunk(2);
        Assert.Equal(res.Count(), 6);

        text = "ID8ZSOB2M03";
        res = text.SplitChunk(2);
        Assert.Equal(res.Count(), 6);

        text = "ID8ZSOB2M0324";
        res = text.SplitChunk(2);
        Assert.Equal(res.Count(), 7);
    }
    [Fact]
    public void should_convert_to_int()
    {
        Assert.Equal(true.toInt(), 1);
        Assert.Equal(false.toInt(), -1);
    }

    [Fact]
    public void should_convert_order_by_bool()
    {
        var text = "ID8ZSOB2M03";
        var res = text.SplitChunk(2);
        Assert.Equal(res.Count(), 6);
        res = res.SortByBool(reverse, "someTxt");
        Assert.Equal(res[0], "3"); 

        var strings = new List<string>(){"asgsag", "aggs", "ID8ZSOB2M03", "relevant", "Relevant"};
        res = strings.SortByBool(relevance, "relevant");
        res.Reverse();
        Assert.Equal(res[0], "relevant"); 
    }

    private bool reverse (string a, string b, string q) => a.Count() > b.Count();

    private bool relevance (string a, string b, string q) => a == q;
}