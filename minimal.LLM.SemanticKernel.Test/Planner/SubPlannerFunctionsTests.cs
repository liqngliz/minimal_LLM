using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using Ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Plugins;
using Planner;
using Reasoners;


namespace PlannerTests;

[Collection("Sequential")]
public class SubPlannerFunctionsTest
{
    readonly IKernelBuilder _builder;
    readonly IContainer<Config> _module;
    readonly Kernel _kernel;

    public SubPlannerFunctionsTest()
    {   
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IocContainer(configurationJSON);
        _builder = Kernel.CreateBuilder();
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        _builder.Services.AddSingleton(reasonerFactory);
        //_builder.Plugins.AddFromType<MathPlugin>();
        _builder.Plugins.AddFromObject(new MathPlugin());
        _kernel = _builder.Build();
        sut = new Planner.Functions.SubPlannerFunctions();
    }
    private IPlanner<Task<List<KernelFunction>>, KernelPlan> sut;

    [Theory]
    [InlineData("I want to get the square root", "Sqrt")]
    [InlineData("I want to add two numbers", "Add")]
    [InlineData("I want to subtract two numbers", "Subtract")]
    [InlineData("What is 56 + 78", "Add")]
    [InlineData("57 times 978", "Multiply")]

    public async void Should_return_functions_for_prompt(string prompt, string function)
    {   
        KernelPlan kPlan = new(_kernel, prompt);
        
        var plans = await sut.Plan(kPlan);
        var functionNames = plans.Select(x => x.Name);
        Assert.Contains(function, functionNames);
        Assert.True(functionNames.Count() <= 4);
    }

}