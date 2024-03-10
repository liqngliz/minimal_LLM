using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using LLama.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using minimal.LLM.SemanticKernel.Plugins;
using Planner;
using Reasoners;


namespace PlannerTests;

[Collection("Sequential")]
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
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        _builder.Services.AddKeyedSingleton<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>("local-llama-reasoner-factory", reasonerFactory);
        _builder.Plugins.AddFromType<MathPlugin>();
        Kernel kernel = _builder.Build();
        KernelPlan kPlan = new(kernel, "what is the square root of 9");
        sut = new Planner.Functions.SubPlannerFunctions();
        var plans = await sut.Plan(kPlan);
        var functionNames = plans.Select(x => x.Name);
        Assert.Equal("Sqrt", plans[0].Name);
        Assert.True(functionNames.Count() == 1);
    }
}