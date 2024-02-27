using System.Reflection;
using Autofac;
using Configuration;
using Context;
using IoC;
using LLama;
using LLama.Common;
using LLamaSharp.SemanticKernel.ChatCompletion;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace minimal.LLM.SemanticKernel;

public class LlmSemanticKernel
{
    readonly IKernelBuilder _builder;
    readonly IModule<Config> _module;
    public LlmSemanticKernel()
    {
        _builder = Kernel.CreateBuilder();
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
    }
    public List<string> transcript = new List<string>();

    public async Task<List<string>> init()
    {   
        var llmContainer = _module.Container();
        var llmContext = llmContainer.Resolve<IContext<LlmContextInstance>>();
        var context = await llmContext.Init();

        var parameters = context.ModelParams;
        using var model = LLamaWeights.LoadFromFile(parameters);
        
        var ex = new StatelessExecutor(model, parameters);

        _builder.Services.AddKeyedSingleton<ITextGenerationService>("local-llama", new LLamaSharpTextCompletion(ex));
        
        var kernel = _builder.Build();

        var prompt = @"{{$input}}
        
        Make a one 150 character summary of the above";

        ChatRequestSettings settings = new() { MaxTokens = 50, StopSequences = new List<string>(){"."}};
        var summarize = kernel.CreateFunctionFromPrompt(prompt, settings);

            string text1 = @"
1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

            string text2 = @"
1. An object at rest remains at rest, and an object in motion remains in motion at constant speed and in a straight line unless acted on by an unbalanced force.
2. The acceleration of an object depends on the mass of the object and the amount of force applied.
3. Whenever one object exerts a force on another object, the second object exerts an equal and opposite on the first.";

        var res1 = (await kernel.InvokeAsync(summarize, new() { ["input"] = text1 })).GetValue<string>();
        var res2 = (await kernel.InvokeAsync(summarize, new() { ["input"] = text2 })).GetValue<string>();
        transcript.Add(res1);
        transcript.Add(res2);
        return transcript;
    }
}
