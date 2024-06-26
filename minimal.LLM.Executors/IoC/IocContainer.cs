using Autofac;
using Newtonsoft.Json;
using Configuration;
using Context;
using Llm;
using Reasoners;
using LLama.Abstractions;
using Factory;
using LLama;

namespace Ioc;
public class IocContainer: IContainer <Config>
{   
    readonly ContainerBuilder _builder;
    readonly IContainer _container;
    readonly Config _configuration;
    public IocContainer(string configPath)
    {
        _builder = new ContainerBuilder();

        //settings
        _configuration = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
        
        //register
        _builder.Register(c => new LlamaSharpContext(_configuration)).As<IContext<LlmContextInstance>>().SingleInstance();

        _builder.Register( c => {
            var context = c.Resolve<IContext<LlmContextInstance>>().Init().Result;
            return new LlmFactory(context);
        }).As<IFactory<ILLamaExecutor>>();

        _builder.Register(c => 
            new LlmInstance(c.Resolve<IContext<LlmContextInstance>>(), c.Resolve<IFactory<ILLamaExecutor>>(), typeof(InteractiveExecutor)))
        .As<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>().SingleInstance();

        _builder.Register(c => {
            var llm = c.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>();
            return new ReasonerFactory(llm);
        }).As<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();

        _builder.Register(c => new LlmReasoner(c.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>())).As<IReasoner<Reasoning, ReasonerTemplate>>();

        //build module container
        _container = _builder.Build();
    }
    public IContainer Container() => _container;
    public Config Configuration() => _configuration;
}