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
        Kernel kernel = _conductorKernel.MakeConductorKernel();

        var validation = 
            (IPlanner<Task<Validation>, KernelParamValidationPlan>)kernel.Services.GetService(typeof(IPlanner<Task<Validation>, KernelParamValidationPlan>));
        
        var parameter = 
            (IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>)kernel.Services.GetService(typeof(IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>));

        var steps = 
            (IPlanner<Task<StepResult>, StepInput>)kernel.Services.GetService(typeof(IPlanner<Task<StepResult>, StepInput>));
        
        var function = 
            (IPlanner<Task<List<KernelFunction>>, KernelPlan>)kernel.Services.GetService(typeof(IPlanner<Task<List<KernelFunction>>, KernelPlan>));

        var factory = (IFactory<IReasoner<Reasoning, ReasonerTemplate>>)kernel.Services.GetService(typeof(IFactory<IReasoner<Reasoning, ReasonerTemplate>>));

        IRouter<RoutedResult>  router = new Router.Router(parameter, validation, steps, function, factory);

        var res = router.route(InputMode.Interactive, "can I get some chicken wings?");

        Assert.DoesNotContain("yes", res.Output.ToLower());

        res = router.route(InputMode.Interactive, "Do you have some text files about cats?");
        Assert.Contains("yes", res.Output.ToLower());

        res = router.route(InputMode.Interactive, "Do you have some documents!");
        Assert.Contains("yes", res.Output.ToLower());
    }
}
