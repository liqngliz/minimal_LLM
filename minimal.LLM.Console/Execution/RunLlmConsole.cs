using Context;
using Llm;

namespace Run;

public class RunLlmConsole : IRun
{   
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> _llmSharp;

    public RunLlmConsole(Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llmSharp)
    {
        _llmSharp = llmSharp;
    }

    public async Task Run()
    {   
        var llm = _llmSharp.InferParams();
        var prompt = llm.Prompt;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The executor has been enabled. In this example, the prompt is printed, the maximum tokens is set to {llm.InferenceParams.MaxTokens} (an example for medium scale usage) {llm.ModelParams.ContextSize} Max context size");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(prompt);
        
        while (true)
        {   
            await foreach (var text in _llmSharp.Infer(prompt)) 
            {   
                Console.Write(text);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            prompt = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}