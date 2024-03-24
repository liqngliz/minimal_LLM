using minimal.LLM.Plugins;

namespace FilePluginTest;
public class FilePluginTest
{   
    public FilePluginTest()
    {
        var files = Directory.GetDirectories(".").ToList();
        if(!files.Contains("files"))
        {
            DirectoryInfo di = Directory.CreateDirectory("./files");
            File.WriteAllText("./files/test_text.txt", test_text);
        }
    }

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

private string test_text = @"5. Tax and legal issues for start-ups
You should choose the right legal structure to suit your circumstances and register it with HM Revenue and Customs. Should you choose the limited company route you are required to register with Companies House.

You may need to seek specialist advice on intellectual property protection to cover copyright, trade marking, design registration or patenting.

It is vital to keep accurate records and pay tax and National Insurance.

6. Business planning for start-ups
You should plan your business carefully before you start up. The headings in a business plan can be thought of as a checklist of questions you need to ask yourself to reassure yourself that your venture will work.

Writing the plan down helps to clarify your thinking and identifies where you intend to get to and how you intend to get there.

Read our guide on how to prepare a business plan and download our business plan template.";
}