using Context;
using Llm;

namespace Run;



public class RunLlmConsole : IRun
{   
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> _llmSharp;
    readonly bool _testMode;

    public RunLlmConsole(Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llmSharp, bool testmode = false)
    {
        _llmSharp = llmSharp;
        _testMode = testmode;
    }

    public async Task<bool> Run()
    {   
        var llmParams = _llmSharp.InferParams();
        var prompt = Constants.InitPrompt;
        using  Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llm = _llmSharp;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The executor has been enabled. In this example, the prompt is printed, the maximum tokens is set to {(llmParams.InferenceParams == null ? 0 : llmParams.InferenceParams.MaxTokens)} (an example for medium scale usage) {(llmParams.ModelParams==null ? 0 : llmParams.ModelParams.ContextSize)} Max context size");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(prompt);
        bool run = true;
        while (run)
        {   
            await foreach (var text in llm.Infer(prompt)) 
            {   
                Console.Write(text);
                
                if(_testMode){
                    run = false;
                    return true;
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            prompt = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Gray;

        }
        return false;
    }
}

public static class Constants
{
    public static string InitPrompt =
@"Transcript of a dialog, where the User interacts with an Assistant named Bob.

User: Hello, Bob.
Bob: Hello. How may I help you today with any questions?
User: Bob, can you make a tell me what is the tallest mountain in the world?
Bob: Sure! That would be Mount Everest.
User: Thanks!
Bob: Do you have another question?
User:";
}