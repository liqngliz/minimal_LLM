using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using Microsoft.SemanticKernel;
using minimal.LLM.Console.Router;
using minimal.LLM.Plugins;
using minimal.LLM.SemanticKernel;
using Planner;
using Planner.StepPlanner;
using Planner.Validators;
using Reasoners;

namespace minimal.LLM.Console.Test;

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
    
    [Fact]
    public void should_route_to_files()
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutedResult>  router = new Router.Router(kernel);

        var res = router.route(Mode.Interactive, "can I get some chicken wings?");

        Assert.DoesNotContain("GetFileList", res.Output.ToLower());
        Assert.True(res.Mode == Mode.Interactive);

        res = router.route(Mode.Interactive, "Do you have some text files about cats?");
        Assert.Contains("GetFileList", res.Output);
        Assert.True(res.Mode != Mode.Interactive);

        res = router.route(Mode.Interactive, "Do you have some documents!");
        Assert.Contains("GetFileList", res.Output);
        Assert.True(res.Mode != Mode.Interactive);
    }
}
