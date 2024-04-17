using Json.Schema;
using UtilsExt;

namespace Router;

public sealed class ModeSingleton : IModeSingleton
{
    private static readonly ModeSingleton instance = new ModeSingleton();
    private bool useAgent;
    public bool UseAgent{get => useAgent;}
    private string key = null;
    public string Key{get => key;}
    private string message;

    static ModeSingleton()
    {
    }
    private ModeSingleton()
    {
    }
    public static ModeSingleton Instance
    {
        get
        {   
            return instance;
        }
    }
    public void Init(string inputKey, string inputMessage) 
    { 
        if(string.IsNullOrEmpty(key))
            key = inputKey;
        else
            throw new InvalidOperationException("Agent Key is already set");

        message = inputMessage;
    }
    public string CheckMode(string input) 
    {   
        if(string.IsNullOrEmpty(key)) 
            throw new InvalidOperationException("Agent Key is not set please set key before mode can be set");
        
        string output = null;
        var inputMatrix = input.ToFlatCharacterStringMatrix();
        var matches = inputMatrix.FilterLevenshteinMatch(key, 0.8);
        string best = "";

        if(matches.Any())
            best = matches.First().GetWordFromOrigin();   

        if(!string.IsNullOrEmpty(best) && best.LevenshteinMatch(key, 0.9))
        {
            output = message + " " + !useAgent;
            useAgent = !useAgent;
        }

        return output;
    }

    public bool UseRouting() => useAgent;
}