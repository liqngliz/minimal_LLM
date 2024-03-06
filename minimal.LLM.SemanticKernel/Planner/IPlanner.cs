using Microsoft.SemanticKernel;

namespace Planner;

public interface IPlanner<T, C>
{
    T Plan(C Inputs);
}

public record KernelPlan(Kernel Kernel, string Prompt);
public record KernelFunctionPlan(KernelFunction KernelFunction, string Prompt);