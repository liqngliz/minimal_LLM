using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using LLama;
using LLama.Abstractions;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using minimal.LLM.SemanticKernel.Plugins;
using Planner;
using Reasoners;


namespace minimal.LLM.SemanticKernel.Test;

public class SubPlannerFunctionsTest
{
    private Planner.IPlanner<Task<List<KernelFunction>>, KernelPlan> sut;
    private IKernelBuilder _builder;
    readonly IModule<Config> _module;
    readonly IFactory<ILLamaExecutor> _factory;

    public SubPlannerFunctionsTest()
    {   
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
        var llmContainer = _module.Container();
        _factory = llmContainer.Resolve<IFactory<ILLamaExecutor>>();
    }

    [Fact]
    public async void Should_return_functions_for_prompt()
    {   
        _builder = Kernel.CreateBuilder();
        var reasoner = _module.Container().Resolve<IReasoner<Reasoning, ReasonerTemplate>>();
        _builder.Services.AddKeyedSingleton<IReasoner<Reasoning, ReasonerTemplate>>("local-llama-reasoner", reasoner);
        _builder.Plugins.AddFromType<MathPlugin>();
        Kernel kernel = _builder.Build();
        KernelPlan kPlan = new(kernel, "what is the square root of 9");
        sut = new Planner.SubPlannerFunctions();
        var plans = await sut.Plan(kPlan);
        var functionNames = plans.Select(x => x.Name);
        Assert.Equal("Sqrt", plans[0].Name);
        Assert.True(functionNames.Count() == 1);
    }
}