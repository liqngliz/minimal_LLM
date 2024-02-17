using Context;
using LLama;
using Llm;

public class LlmInteractiveInstance : Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>
{ 
    
    readonly IContext<LlmContextInstance> _settings;
    readonly LlmContextInstance _llamaInstance;
    private LLamaWeights model;
    private LLamaContext context;
    private InteractiveExecutor interactiveExecutor;

    public LlmInteractiveInstance (IContext<LlmContextInstance> settings)
    { 
        _settings = settings;
        _llamaInstance = _settings.Init().Result;
        
        model = LLamaWeights.LoadFromFile(_llamaInstance.ModelParams);
        context = model.CreateContext(_llamaInstance.ModelParams);
        interactiveExecutor = new InteractiveExecutor(context);
    }

    public void Dispose()
    {
        model.Dispose();
        context.Dispose();
        interactiveExecutor = null;
    }

    public IAsyncEnumerable<string> Infer(string prompt) { 
        if(interactiveExecutor == null)
        {
            model = LLamaWeights.LoadFromFile(_llamaInstance.ModelParams);
            context = model.CreateContext(_llamaInstance.ModelParams);
            interactiveExecutor = new InteractiveExecutor(context);
        }
        
        return interactiveExecutor.InferAsync(prompt, _llamaInstance.InferenceParams);
    }

    public LlmContextInstance InferParams() => _llamaInstance;

    public bool IsDisposed() => interactiveExecutor == null;
}

