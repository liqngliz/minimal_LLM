using Configuration;
using Context;
using IoC;
using LLama;

namespace Run;

public class RunLlama : IRun
{   
    readonly IContext<LlamaInstance> _settings;

    public RunLlama(IContext<LlamaInstance> settings)
    {
        _settings = settings;
    }

    public async Task Run()
    {   
        var llama = await _settings.Init();
        var prompt = llama.Prompt;
        
        using var model = LLamaWeights.LoadFromFile(llama.ModelParams);
        using var context = model.CreateContext(llama.ModelParams);
        var interactiveExecutor = new InteractiveExecutor(context);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The executor has been enabled. In this example, the prompt is printed, the maximum tokens is set to {llama.InferenceParams.MaxTokens} (an example for medium scale usage) {llama.ModelParams.ContextSize} Max context size");
        Console.ForegroundColor = ConsoleColor.White;

        Console.Write(prompt);
        
        while (true)
        {   

            await foreach (var text in interactiveExecutor.InferAsync(prompt, llama.InferenceParams)) 
            {   
                Console.Write(text);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            prompt = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;

        }

    }


}