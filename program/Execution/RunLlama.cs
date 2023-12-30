using Configuration;
using Context;
using IoC;
using LLama;
using llm;

namespace Run;

public class RunLlama : IRun
{   
    readonly Illm<IAsyncEnumerable<string>, string, LlamaInstance> _llamaSharpLlm;

    public RunLlama(Illm<IAsyncEnumerable<string>, string, LlamaInstance> llamaSharpLlm)
    {
        _llamaSharpLlm = llamaSharpLlm;
    }

    public async Task Run()
    {   
        var llama = _llamaSharpLlm.InferParams();
        var prompt = llama.Prompt;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The executor has been enabled. In this example, the prompt is printed, the maximum tokens is set to {llama.InferenceParams.MaxTokens} (an example for medium scale usage) {llama.ModelParams.ContextSize} Max context size");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(prompt);
        
        while (true)
        {   

            await foreach (var text in _llamaSharpLlm.Infer(prompt)) 
            {   
                Console.Write(text);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            prompt = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Gray;

        }

        _llamaSharpLlm.Dispose();

    }


}