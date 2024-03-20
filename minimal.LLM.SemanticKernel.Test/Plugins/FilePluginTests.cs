using minimal.LLM.SemanticKernel.Plugins;

namespace PluginTests;
public class FilePluginTest
{   

    [Fact]
    public void should_get_files()
    {
        var fileNames = FilePlugin.GetFileList();
        Assert.True(fileNames.Contains("test_text.txt"));
    }

     [Fact]
    public void should_read_file()
    {
        var fileContent = FilePlugin.GetContent("test_text.txt");
        Assert.False(string.IsNullOrEmpty(fileContent));
    }
}