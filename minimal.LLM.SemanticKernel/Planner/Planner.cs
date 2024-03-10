using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Reasoners;

namespace Planner;

public class Planner : IPlanner<Task<List<Tuple<KernelFunction, KernelArguments>>>, KernelPlan>
{   
    readonly IPlanner<Task<List<KernelFunction>>, KernelPlan> _subPlannerForFunctions;
    readonly IPlanner<Task<KernelArguments>, KernelFunctionPlan> _subPlannerForParameters;

    public Planner(IPlanner<Task<List<KernelFunction>>, KernelPlan> subPlannerFunctions, IPlanner<Task<KernelArguments>, KernelFunctionPlan> subPlannerForParameters)
    {
        _subPlannerForFunctions = subPlannerFunctions;
        _subPlannerForParameters = subPlannerForParameters;

    }

    public async Task<List<Tuple<KernelFunction, KernelArguments>>> Plan(KernelPlan Inputs)
    {   
        var functionsPlan = _subPlannerForFunctions.Plan(Inputs).Result;

        var pluginFuncTuples= new List<Tuple<KernelFunction, KernelArguments>>();

        foreach(var function in functionsPlan)
        {   
            var parametersPlan = await _subPlannerForParameters.Plan(new(function, Inputs.Kernel, Inputs.Prompt));
            
            pluginFuncTuples.Add(new(function, parametersPlan));
        }
        
        return pluginFuncTuples;
    }
}

public static class PlannerExtensions
{
    public static T ConvertObject<T>(this object input) {
        return (T) Convert.ChangeType(input, typeof(T));
    }
}