using Configuration;
using IoC;
using Microsoft.SemanticKernel;

namespace minimal.LLM.SemanticKernel;

public interface ILlmConductor
{
    public LlmConductor LlmConductor();
}

public record LlmConductor(Kernel OrchestrationKernel);