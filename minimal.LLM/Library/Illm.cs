namespace Llm;

public interface Illm <T, C, G, k> : IDisposable
{
    public T Infer(C prompt);
    public G InferParams();

    public k IsDisposed();
}

