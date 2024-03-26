using Configuration;
using IoC;
using Microsoft.SemanticKernel;

namespace minimal.LLM.SemanticKernel;

public interface ILlmConductorKernel
{
    public Kernel MakeConductorKernel();
}

