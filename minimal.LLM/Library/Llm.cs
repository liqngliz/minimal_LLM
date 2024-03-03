using Context;
using Factory;
using LLama;
using LLama.Abstractions;
using Llm;

public class LlmInstance : Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>
{ 
    
    readonly IContext<LlmContextInstance> _settings;
    readonly LlmContextInstance _llamaInstance;
    readonly IFactory<ILLamaExecutor> _factory;
    readonly Type _executorType; 
    private LLamaWeights model;
    private LLamaContext context;
    private ILLamaExecutor executor;

    public LlmInstance (IContext<LlmContextInstance> settings, IFactory<ILLamaExecutor> factory, Type executorType)
    { 
        _settings = settings;
        _factory = factory;
        _llamaInstance = _settings.Init().Result;
        _executorType = executorType;

        model = LLamaWeights.LoadFromFile(_llamaInstance.ModelParams);
        context = model.CreateContext(_llamaInstance.ModelParams);
        executor = _factory.Make(_executorType);
    }

    public void Dispose()
    {
        model.Dispose();
        context.Dispose();
        executor = null;
    }

    public IAsyncEnumerable<string> Infer(string prompt) { 
        if(executor == null)
        {
            model = LLamaWeights.LoadFromFile(_llamaInstance.ModelParams);
            context = model.CreateContext(_llamaInstance.ModelParams);
            executor = _factory.Make(_executorType);
        }
        
        return executor.InferAsync(prompt, _llamaInstance.InferenceParams);
    }

    public LlmContextInstance InferParams() => _llamaInstance;

    public bool IsDisposed() => executor == null;
}