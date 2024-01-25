using Autofac;
using Newtonsoft.Json;
using Configuration;
using Context;
using Run;
using Llm;
using Reasoners;

namespace IoC;
public class IoCModule: IModule <Config>
{   
    readonly ContainerBuilder _builder;
    readonly IContainer _container;
    readonly Config _configuration;
    public IoCModule(string configPath)
    {
        _builder = new ContainerBuilder();

        //settings
        _configuration = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
        
        //register
        _builder.Register(c => new LlamaSharpContext(_configuration)).As<IContext<LlamaInstance>>().SingleInstance();
        _builder.Register(c => new LlamaSharpLlm(c.Resolve<IContext<LlamaInstance>>())).As<Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool>>().SingleInstance();
        
        _builder.Register(c => new LlmReasonerRelevance(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool>>())).As<IReasoner<bool, Relevance>>();
        _builder.Register(c => new LlmReasonerSummary(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool>>())).As<IReasoner<string, Summary>>();
        _builder.Register(c => new LlmReasonerClassify(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool>>())).As<IReasoner<string, Classify>>();

        _builder.Register(c => new RunLlama(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool>>())).As<IRun>();

        //build module container
        _container = _builder.Build();
    }
    public IContainer Container() => _container;
    public Config Configuration() => _configuration;
}