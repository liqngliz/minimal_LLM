using Llm;

namespace Reasoner;

public interface IReasoner <T,C>
{
    Task<T> Reason(C Input);
}