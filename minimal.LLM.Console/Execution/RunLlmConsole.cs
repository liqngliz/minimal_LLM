using Context;
using LLama.Common;
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
        var llm = _llmSharp.InferParams();
        var prompt = llm.Prompt;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The executor has been enabled. In this example, the prompt is printed, the maximum tokens is set to {(llm.InferenceParams == null ? 0 : llm.InferenceParams.MaxTokens)} (an example for medium scale usage) {(llm.ModelParams==null ? 0 : llm.ModelParams.ContextSize)} Max context size");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(prompt);
        bool run = true;
        while (run)
        {   
            await foreach (var text in _llmSharp.Infer(prompt)) 
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