using Microsoft.SemanticKernel;
using Reasoners;
using Planner;
using Planner.Validators;
using Planner.StepPlanner;
using Planner.Functions;
using Factory;
using Planner.Parameters;
using Microsoft.Extensions.DependencyInjection;

namespace minimal.LLM.SemanticKernel;

public class LocalllmKernel :ILlmConductor
{  
    readonly List<object> _plugins;
    readonly IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> _parameterPlanner;
    readonly IPlanner<Task<Validation>, KernelParamValidationPlan> _validationPlanner;
    readonly IPlanner<Task<StepResult>, StepInput> _stepsPlanner;
    readonly IPlanner<Task<List<KernelFunction>>, KernelPlan> _functionsPlanner;
    IFactory<IReasoner<Reasoning, ReasonerTemplate>> _llmFactory;
    public LocalllmKernel(List<object>  plugins, IFactory<IReasoner<Reasoning, ReasonerTemplate>> llmFactory, SubPlannerFunctionsTemplate functionsTemplate = null, StepPlannerTemplate stepPlannerTemplate = null, string parameterPlannerQuery = null, string validationErrorMessage = null)
    { 
        _plugins = plugins;
        _llmFactory = llmFactory;
        _functionsPlanner = new SubPlannerFunctions(functionsTemplate);
        _parameterPlanner = new SubPlannerParameter(parameterPlannerQuery);
        _validationPlanner = new SubPlannerValidator(validationErrorMessage);
        _stepsPlanner = new StepPlanner(_parameterPlanner, _validationPlanner, stepPlannerTemplate);
    }

    public LlmConductor LlmConductor()
    {   
        var builder = Kernel.CreateBuilder();
        
        _plugins.ForEach(x =>{
            var obj = x;
            builder.Plugins.AddFromObject(obj);
        });

        builder.Services.AddSingleton(_llmFactory);
        builder.Services.AddSingleton(_functionsPlanner);
        builder.Services.AddSingleton(_stepsPlanner);
        builder.Services.AddSingleton(_parameterPlanner);
        builder.Services.AddSingleton(_validationPlanner);

        Kernel kernel = builder.Build();
        return new(kernel);
        
    }
}
