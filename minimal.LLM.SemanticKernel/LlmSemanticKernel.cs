using System.Reflection;
using System.Text;
using Autofac;
using Configuration;
using Factory;
using IoC;
using LLama;
using LLama.Abstractions;
using LLamaSharp.SemanticKernel.ChatCompletion;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;
using Microsoft.SemanticKernel.TextGeneration;
using minimal.LLM.SemanticKernel.Plugins;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Reasoners;


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

    public async Task<string> copilot(string input)
    {
        var ex = (StatelessExecutor)_factory.Make(typeof(StatelessExecutor));
        _builder = Kernel.CreateBuilder();
        _builder.Services.AddKeyedSingleton<IChatCompletionService>("local-llama", new LLamaSharpChatCompletion(ex));
        Kernel kernel = _builder.Build();

        ChatHistory history = new();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        
        var transcript = new StringBuilder();
        transcript.AppendLine($"User: {input}");
        history.AddUserMessage(transcript.ToString());
        
        var res = chatCompletionService.GetStreamingChatMessageContentsAsync(history, kernel: kernel);

        string fullMessage = "";
        var first = true;
        await foreach (var content in res)
        {
            if (content.Role.HasValue && first)
            {
                first = false;
            }
            fullMessage += content.Content;
        }
        return fullMessage;
    }

    public async Task<string> sqrt(int input)
    {
        _builder = Kernel.CreateBuilder();
        _builder.Plugins.AddFromType<MathPlugin>();
        Kernel kernel = _builder.Build();
        double answer = await kernel.InvokeAsync<double>("MathPlugin", "Sqrt", new(){{"number1", input}});
        return $"The sqaure root of {input} is {answer}";
    }

    
    public async Task<string> plannedCopilot(string input)
    {
        var ex = (StatelessExecutor)_factory.Make(typeof(StatelessExecutor));
        _builder = Kernel.CreateBuilder();
        _builder.Services.AddKeyedSingleton<IChatCompletionService>("local-llama", new LLamaSharpChatCompletion(ex));
        var reasoner = _module.Container().Resolve<IReasoner<Reasoning, ReasonerTemplate>>();
        _builder.Services.AddKeyedSingleton<IReasoner<Reasoning, ReasonerTemplate>>("local-llama-reasoner", reasoner);
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        _builder.Services.AddKeyedSingleton<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>("local-llama-reasoner-factory", reasonerFactory);
        _builder.Plugins.AddFromType<MathPlugin>();
        Kernel kernel = _builder.Build();
        var mathPlugins = kernel.Plugins;
        var mathPluginsMeta = kernel.Plugins.GetFunctionsMetadata();

        /*
        #pragma warning disable SKEXP0060
        var options = new HandlebarsPlannerOptions() { AllowLoops = true };
        var planner = new HandlebarsPlanner(options);
        var plan = await planner.CreatePlanAsync(kernel, input);
        var planText = plan.ToString();
        */

        return null;
    }
}
