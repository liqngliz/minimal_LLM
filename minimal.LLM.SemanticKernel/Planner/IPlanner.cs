using Microsoft.SemanticKernel;

namespace Planner;

public interface IPlanner<T, C>
{
    T Plan(C inputs);
}

public record KernelPlan(Kernel Kernel, string Prompt);
public record KernelParamValidationPlan(KernelParameterMetadata Parameter, string Input);