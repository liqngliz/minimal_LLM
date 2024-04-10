using Autofac;
using Context;
using Llm;
using Reasoners;

namespace Factory;

public class ReasonerFactory : IFactory<IReasoner<Reasoning, ReasonerTemplate>>
{   
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> _llm;

    public ReasonerFactory(Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llm)
    {
        _llm = llm;
    }

    public IReasoner<Reasoning, ReasonerTemplate> Make(Type type)
    {   
        ContainerBuilder builder = new ContainerBuilder();

         builder.Register(c => 
        {   
            return new LlmReasoner(_llm);
        })
        .As<IReasoner<Reasoning, ReasonerTemplate>>().Keyed<IReasoner<Reasoning, ReasonerTemplate>>(nameof(LlmReasoner));
        
        var container = builder.Build();
        var name = type.Name;
        var res = container.ResolveKeyed<IReasoner<Reasoning, ReasonerTemplate>>(name);
        container.Dispose();
        
        return res;
    }

}