using Autofac;
using Context;
using LLama;
using LLama.Abstractions;
using LLama.Common;

namespace Factory;

public class LlmFactory : IFactory<ILLamaExecutor>
{   
    readonly LlmContextInstance _llmContext;
    private ContainerBuilder builder;
    public LlmFactory(LlmContextInstance llmContext)
    {
        _llmContext = llmContext;
    }
    public ILLamaExecutor Make(Type type)
    {   
        builder = new ContainerBuilder();  

        var keys = new List<string>(){nameof(InteractiveExecutor), nameof(InstructExecutor), nameof(StatelessExecutor)};

        builder.Register(c => 
        {   
            ModelParams parameters = _llmContext.ModelParams;
            var model = LLamaWeights.LoadFromFile(parameters);
            var context = model.CreateContext(parameters);
            return new InteractiveExecutor(context);
        })
        .As<ILLamaExecutor>().Keyed<ILLamaExecutor>(keys[0]);

        builder.Register(c => {   
            ModelParams parameters = _llmContext.ModelParams;
            var model = LLamaWeights.LoadFromFile(parameters);
            var context = model.CreateContext(parameters);
            return new InstructExecutor(context);
        })
        .As<ILLamaExecutor>().Keyed<ILLamaExecutor>(keys[1]);

        builder.Register(c => {   
            ModelParams parameters = _llmContext.ModelParams;
            var model = LLamaWeights.LoadFromFile(parameters);
            var context = model.CreateContext(parameters);
            return new StatelessExecutor(model, parameters);
        })
        .As<ILLamaExecutor>().Keyed<ILLamaExecutor>(keys[2]); 

        var container = builder.Build();
        var name = type.Name;
        var res = container.ResolveKeyed<ILLamaExecutor>(name);
        container.Dispose();
        return res;
    }
}
