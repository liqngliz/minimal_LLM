using System.ComponentModel;
using Microsoft.SemanticKernel;


namespace minimal.LLM.SemanticKernel.Plugins;

public sealed class FilePlugin
{
    [KernelFunction, Description("Gets the names of files available to us")]
    public static string GetFileList() 
    {   
        var files = Directory.GetFiles("./files").ToList();
        files = files.Select(x => x.Replace("./files/", "")).ToList();
        var fileNames = string.Join(", ", files);
        return fileNames;
    }

    [KernelFunction, Description("Reads the content of a text file")]
    public static string GetContent(
        [Description("Full name of file with extensions to be read")] string fileName) 
    {   
        var fileText = File.ReadAllText("./files/" + fileName); 
        return fileText;
    }
}
