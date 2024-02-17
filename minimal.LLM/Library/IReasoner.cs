using Llm;

namespace Reasoners;

public interface IReasoner <T,C>
{
    Task<T> Reason(C Input);
}