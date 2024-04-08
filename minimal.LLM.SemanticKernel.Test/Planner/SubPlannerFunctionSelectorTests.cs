using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Plugins;
using Planner;
using Reasoners;
using Planner.FunctionSelector;


namespace PlannerTests;

[Collection("Sequential")]
public class SubPlannerFunctionsSelectorTest
{   
    readonly IKernelBuilder _builder;
    readonly IModule<Config> _module;
    readonly Kernel _kernel;

    public SubPlannerFunctionsSelectorTest()
    {   
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
        _builder = Kernel.CreateBuilder();
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        _builder.Services.AddSingleton(reasonerFactory);
        //_builder.Plugins.AddFromType<MathPlugin>();
        _builder.Plugins.AddFromObject(new MathPlugin());
        _kernel = _builder.Build();
    }

    private IPlanner<FunctionSelection, FunctionOptions> sut;

    [Theory]
    [InlineData("I want to get use Sqrt", true,"Sqrt")]
    [InlineData("I want to adds", true, "Add")]
    [InlineData("I want to subtraction", true, "Subtract")]
    [InlineData("What is 56 + 78", false, null)]
    [InlineData("57 times 978", false, null)]
    public void should_find_function_when_near_match(string input, bool expected, string expectedFunctionName)
    {   
        var kernelFunctionsMeta = _kernel.Plugins.GetFunctionsMetadata();
        var kernelFunctions = kernelFunctionsMeta.Select(x => _kernel.Plugins.GetFunction(x.PluginName, x.Name)).ToList();
        sut = new SubPlannerFunctionSelector();
        var res = sut.Plan(new(kernelFunctionsMeta.ToList(), _kernel, input));
        KernelFunction expectedFunction = kernelFunctions.Where(x => x.Name == expectedFunctionName).FirstOrDefault();
        Assert.Equal(res.Valid, expected);
        if(res.Valid)
            Assert.Equal(res.KernelFunctions.Single(), expectedFunction);
        else
            Assert.Null(res.KernelFunctions);
    }

}