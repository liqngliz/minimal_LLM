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
        _builder.Register(c => new LlamaSharpContext(_configuration)).As<IContext<LlmContextInstance>>().SingleInstance();
        _builder.Register(c => new LlmInstance(c.Resolve<IContext<LlmContextInstance>>())).As<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>().SingleInstance();
        
        _builder.Register(c => new LlmReasonerRelevance(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>())).As<IReasoner<bool, Relevance>>();
        _builder.Register(c => new LlmReasonerSummary(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>())).As<IReasoner<string, Summary>>();

        _builder.Register(c => new LlmClassifier(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>())).As<IReasoner<Classification, ClassificationTemplate>>();
        
        _builder.Register(c => new LlmReasonerClassifyL1(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>())).As<IReasoner<List<string>, ClassifyL1>>();
        _builder.Register(c => new LlmReasonerClassifyL2(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>())).As<IReasoner<List<string>, ClassifyL2>>();

        _builder.Register(c => new RunLlama(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>())).As<IRun>();

        //build module container
        _container = _builder.Build();
    }
    public IContainer Container() => _container;
    public Config Configuration() => _configuration;
}