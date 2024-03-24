using Configuration;
using IoC;
using Microsoft.SemanticKernel;

namespace minimal.LLM.SemanticKernel;

public interface ILlmConductor
{
    public Kernel LlmConductor();
}
