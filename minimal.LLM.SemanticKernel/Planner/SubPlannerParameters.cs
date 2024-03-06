using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Reasoners;

namespace Planner;

public class SubPlannerParameters : IPlanner<Task<List<KernelArguments>>, KernelPlan>
{
    public Task<List<KernelArguments>> Plan(KernelPlan Inputs)
    {
        throw new NotImplementedException();
    }
}