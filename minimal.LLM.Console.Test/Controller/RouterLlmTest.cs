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
    public void should_route_to_function()
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutingPayload>  router = new Router.Router(kernel);

        var res = router.route(new (Mode.Interactive, "can I get some chicken wings?"));

        Assert.DoesNotContain("GetFileList", res.Text.ToLower());
        Assert.True(res.Mode == Mode.Interactive);

        res = router.route(new(Mode.Interactive, "Do you have some text files about cats?"));
        Assert.Contains("GetFileList", res.Text);
        Assert.True(res.Mode == Mode.FunctionPlan);

        res = router.route(new(Mode.Interactive, "Do you have some documents!"));
        Assert.Contains("GetFileList", res.Text);
        Assert.True(res.Mode == Mode.FunctionPlan);
    }

    [Fact]
    public void should_route_to_result()
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutingPayload>  router = new Router.Router(kernel);
        var res = router.route(new(Mode.Interactive, "Do you have some text files about cats?"));
        Assert.Contains("GetFileList", res.Text);
        Assert.True(res.Mode == Mode.FunctionPlan);

        res = router.route(res with {Mode = Mode.FunctionPlan, Text = "Yes please use GetFileList and get me the list of files"});
        Assert.True(res.Mode == Mode.Result);

        res = router.route(new(Mode.Interactive, "I want the content of cats.txt"));
        Assert.Contains("GetContent", res.Text);
        Assert.True(res.Mode == Mode.FunctionPlan);

        res = router.route(res with {Mode = Mode.FunctionPlan, Text = "Yes please use GetContent and get me content of cats.txt"});
        Assert.True(res.Mode == Mode.StepsPlan);

    }

    [Fact]
    public void should_route_to_steps()
    {
        ConductorKernel kernel = _conductorKernel.MakeConductorKernel();

        IRouter<RoutingPayload>  router = new Router.Router(kernel);

        var res = router.route(new(Mode.Interactive, "I want the content of cats.txt from the file list."));
        Assert.Contains("GetContent", res.Text);
        Assert.True(res.Mode == Mode.FunctionPlan);

        res = router.route(res with {Mode = Mode.FunctionPlan, Text = "Yes please use GetContent and get me content of cats.txt"});
        Assert.True(res.Mode == Mode.StepsPlan);


    }
}
