using System.ComponentModel;
using Context;
using LLama;
using llm;

public class LlamaSharpLlm : Illm<IAsyncEnumerable<string>, string, LlamaInstance>
{ 
    
    readonly IContext<LlamaInstance> _settings;
    readonly LlamaInstance _llamaInstance;
    private LLamaWeights model;
    private LLamaContext context;
    private InteractiveExecutor interactiveExecutor;


    public LlamaSharpLlm (IContext<LlamaInstance> settings)
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

    public LlamaInstance InferParams() => _llamaInstance;
}

