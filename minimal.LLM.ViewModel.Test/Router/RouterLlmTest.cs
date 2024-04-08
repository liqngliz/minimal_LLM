using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using Plugins;
using minimal.LLM.SemanticKernel;
using Reasoners;
using ViewRouter;

namespace RouterTest;

public class RouterLlmTest
{
    readonly ILlmConductorKernel _conductorKernel;
    readonly IModule<Config> _module;

    public RouterLlmTest()
    {
         var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
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

        IRouter<RoutingPayload>  router = new Router(kernel);

        var res = router.route(new (Mode.Interactive, input));

        Assert.Equal(match, res.Text.ToLower().Contains(functionName.ToLower()));
        Assert.True(res.Mode == expectedMode);
    }

    [Fact]
    public void should_route_to_steps_when_function_selected()
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutingPayload>  router = new Router(kernel);
        var res = router.route(new(Mode.Interactive, "Do you have some text files about cats?"));

    }

    [Fact]
    public void should_route_to_result_when_valid_parameters()
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutingPayload>  router = new Router(kernel);

        var res = router.route(new(Mode.Interactive, "I want the content of cats.txt from the file list."));

    }
}
