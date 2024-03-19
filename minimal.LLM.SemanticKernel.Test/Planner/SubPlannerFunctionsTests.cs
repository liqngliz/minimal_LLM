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
    readonly IKernelBuilder _builder;
    readonly IModule<Config> _module;
    readonly Kernel _kernel;

    public SubPlannerFunctionsTest()
    {   
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
        _builder = Kernel.CreateBuilder();
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        _builder.Services.AddKeyedSingleton<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>("local-llama-reasoner-factory", reasonerFactory);
        _builder.Plugins.AddFromType<MathPlugin>();
        _kernel = _builder.Build();
    }

    [Theory]
    [InlineData("what is the square root of 9", "Sqrt")]
    [InlineData("what is 5 plus 1982", "Add")]
    [InlineData("876 minus -1", "Subtract")]
    [InlineData("What is -56 added to 78", "Add")]
    [InlineData("57 multiplied by 978", "Multiply")]
    public async void Should_return_functions_for_prompt(string prompt, string function)
    {   
        KernelPlan kPlan = new(_kernel, prompt);
        sut = new Planner.Functions.SubPlannerFunctions();
        var plans = await sut.Plan(kPlan);
        var functionNames = plans.Select(x => x.Name);
        Assert.Equal(function, plans[0].Name);
        Assert.True(functionNames.Count() == 1);
    }

}