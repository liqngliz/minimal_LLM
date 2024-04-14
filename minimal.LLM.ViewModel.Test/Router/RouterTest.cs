using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using Ioc;
using Plugins;
using minimal.LLM.SemanticKernel;
using Reasoners;
using Router;
using Microsoft.SemanticKernel;
using FilePluginTest;
using Planner.StepPlanner;

namespace RouterTest;

public class RouterTest
{
    readonly ILlmConductorKernel _conductorKernel;
    readonly IContainer<Config> _module;

    public RouterTest()
    {   
        FilePluginTestUtils.CheckTestFiles();

         var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IocContainer(configurationJSON);
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        List<object> plugins = new List<object>(){ new FilePlugin()};
        _conductorKernel = new LlmConductorKernel(plugins, reasonerFactory);
    }
    
    [Theory]
    [InlineData("can I get some chicken wings?", "GetFileList", false, Mode.Interactive)]
    [InlineData("Do you have some text files about cats?", "GetFileList", true, Mode.FunctionPlan)]
    [InlineData("Do you have some documents!", "GetFileList", true, Mode.FunctionPlan)]
    public void should_route_to_function_when_native_function_match(string input, string functionName, bool match, Mode expectedMode)
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutingPayload> router = new Router.Router(kernel);

        var res = router.route(new (Mode.Interactive, input));

        Assert.Equal(match, res.Text.ToLower().Contains(functionName.ToLower()));
        Assert.True(res.Mode == expectedMode);
    }

    [Theory]
    [InlineData("I want to use GetFileList", Mode.Result)]
    [InlineData("I want to use GetContent", Mode.StepsPlan)]
    [InlineData("I want to use GetFileList and GetContent", Mode.FunctionPlan)]
    [InlineData("I want to use the function buyCountry", Mode.FunctionPlan)]
    [InlineData("I want to eat some fried chicken", Mode.Interactive)]
    public void should_route_to_steps_when_function_selected(string text, Mode expectedMode)
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutingPayload> router = new Router.Router(kernel);
        List<KernelFunction> kernelFunctions = kernel.Kernel.Plugins.GetFunctionsMetadata()
            .Select(x => kernel.Kernel.Plugins.GetFunction(x.PluginName, x.Name)).ToList();

        RoutingPayload next = new RoutingPayload(Mode.FunctionPlan, text, kernelFunctions);
        var sut = router.route(next);
        Assert.True(sut.Mode == expectedMode);
    }

    [Theory]
    [InlineData("test_text.txt", "GetContent", Mode.StepsPlan, Mode.Result)]
    public void should_route_to_result_when_valid_parameters(string input, string functionName, Mode mode, Mode expectedMode)
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutingPayload> router = new Router.Router(kernel);
        List<KernelFunction> kernelFunctions = new List<KernelFunction>(){kernel.Kernel.Plugins.GetFunction("FilePlugin", functionName)};
        StepResult stepResult = new StepResult("some output", false);

        RoutingPayload next = new RoutingPayload(mode, input, kernelFunctions, stepResult);
        var sut = router.route(next);
        sut = router.route(next);
        Assert.True(sut.Mode == expectedMode);
    }
}
