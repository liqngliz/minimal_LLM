using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Reasoners;

namespace Planner;

public class SubPlannerFunctions : IPlanner<Task<List<KernelFunction>>, KernelFunctionPlan>
{
    public Task<List<KernelFunction>> Plan(KernelFunctionPlan Inputs)
    {
        throw new NotImplementedException();
    }
}