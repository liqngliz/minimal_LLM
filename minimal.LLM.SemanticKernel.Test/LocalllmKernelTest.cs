using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using Microsoft.SemanticKernel;
using minimal.LLM.SemanticKernel;
using Planner;
using Planner.Functions;
using Planner.Parameters;
using Planner.StepPlanner;
using Planner.Validators;
using Reasoners;

namespace LlmKernelTest;

public class LlmKernelTest
{
    readonly ILlmConductor _sut;
    readonly IModule<Config> _module;
    public LlmKernelTest()
    {
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        List<object> plugins = new List<object>(){};
        _sut = new LocalllmKernel(plugins, reasonerFactory);
    }

    [Theory]
    [InlineData(typeof(IFactory<IReasoner<Reasoning, ReasonerTemplate>>),typeof(ReasonerFactory))]
    [InlineData(typeof(IPlanner<Task<Validation>, KernelParamValidationPlan>),typeof(SubPlannerValidator))]
    [InlineData(typeof(IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>),typeof(SubPlannerParameter))]
    [InlineData(typeof(IPlanner<Task<StepResult>, StepInput>),typeof(StepPlanner))]
    [InlineData(typeof(IPlanner<Task<List<KernelFunction>>, KernelPlan>),typeof(SubPlannerFunctions))]
    public void should_resolve_types(Type serviceType, Type expected)
    {
        LlmConductor conductor = _sut.LlmConductor();
        var services = conductor.OrchestrationKernel.Services;
        var types = services.GetService(serviceType);
        Assert.IsType(expected, types);
    }
}
