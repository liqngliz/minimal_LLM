using Llm;

namespace Reasoners;

public interface IReasoner <T,C> : IDisposable
{
    Task<T> Reason(C Input);
}