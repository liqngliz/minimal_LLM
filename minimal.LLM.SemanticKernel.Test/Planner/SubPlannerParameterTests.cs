using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using LLama.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using minimal.LLM.SemanticKernel.Plugins;
using NSubstitute.Core;
using Planner;
using Reasoners;


namespace PlannerTests;

[Collection("Sequential")]
public class SubPlannerParametersTest
{
    private Planner.IPlanner<Task<KernelArguments>, KernelFunctionPlan> sut;
    private IKernelBuilder _builder;
    readonly IModule<Config> _module;
    readonly IFactory<ILLamaExecutor> _factory;

    public SubPlannerParametersTest()
    {   
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
        var llmContainer = _module.Container();
        _factory = llmContainer.Resolve<IFactory<ILLamaExecutor>>();
    }

    [Fact]
    public async void Should_return_values_for_function_from_prompt()
    {   
        _builder = Kernel.CreateBuilder();
         var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        _builder.Services.AddKeyedSingleton<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>("local-llama-reasoner-factory", reasonerFactory);
        _builder.Plugins.AddFromType<MathPlugin>();
        Kernel kernel = _builder.Build();
        KernelFunction function = kernel.Plugins.GetFunction("MathPlugin", "Sqrt");
        KernelFunctionPlan kPlan = new(function ,kernel, "what is the square root of 9");

        sut = new Planner.Parameters.SubPlannerParameters();
        var parameters = await sut.Plan(kPlan);
        var value = parameters.First().Value.ToString();
        Assert.Equal("9", value);
        Assert.Equal("number1", parameters.First().Key);

        var res = await kernel.InvokeAsync(function, parameters);
        Assert.Equal("3", res.ToString());
    }
}