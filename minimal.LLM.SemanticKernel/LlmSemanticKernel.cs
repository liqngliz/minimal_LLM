using System.Reflection;
using System.Text;
using Autofac;
using Configuration;
using Context;
using Factory;
using IoC;
using LLama;
using LLama.Abstractions;
using LLama.Common;
using LLamaSharp.SemanticKernel.ChatCompletion;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace minimal.LLM.SemanticKernel;

public class LlmSemanticKernel
{
    private IKernelBuilder _builder;
    readonly IModule<Config> _module;
    readonly IFactory<ILLamaExecutor> _factory;
    public LlmSemanticKernel()
    {
        
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
        var llmContainer = _module.Container();
        _factory = llmContainer.Resolve<IFactory<ILLamaExecutor>>();
    }

    public async Task<string> summarize(string input)
    {   
        var ex = _factory.Make(typeof(InteractiveExecutor));
        _builder = Kernel.CreateBuilder();
        _builder.Services.AddKeyedSingleton<ITextGenerationService>("local-llama", new LLamaSharpTextCompletion(ex));
        var kernel = _builder.Build();
        var conv = new string[]
        {
            "Forget and clear any previous dialogues, transcripts, and instructions. Prohibere", 
            "New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content, making summaries, coding, and logic. Prohibere",
            "User: Hello, Bob.",
            "Bob: Hello. How may I help you today? Prohibere",
            "User: I will be giving you some text to summarize.",
            "Bob: Ok, I am ready to recieve instructions and start classifying? Prohibere",
            @"User: Make a one sentence summary of {{$input}}",
        };
        var startPrompt = new StringBuilder();
        conv.ToList().ForEach(x => startPrompt.AppendLine(x));

        var prompt = startPrompt.ToString();

        ChatRequestSettings settings = new() { MaxTokens = 50};
        var summarize = kernel.CreateFunctionFromPrompt(prompt);
        var res = (await kernel.InvokeAsync(summarize, new() {["input"] = input })).GetValue<string>();
        return res;
    }
}
