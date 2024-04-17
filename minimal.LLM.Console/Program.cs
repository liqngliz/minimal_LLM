using System.Reflection;
using Autofac;
using Context;
using Ioc;
using Llm;
using Run;
using Factory;
using Reasoners;
using Plugins;
using minimal.LLM.SemanticKernel;
using Router;
;

//Llm Lib container
Console.WriteLine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
var llmContainer = new IocContainer(configurationJSON).Container();
var llm = llmContainer.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>();

//Orchestration container
var reasonerFactory = llmContainer.Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
List<object> plugins = new List<object>(){ new FilePlugin() };
ILlmConductorKernel llmConductor = new LlmConductorKernel(plugins, reasonerFactory);

//Semantic Router
IRouter<RoutingPayload> router = new Router.Router(llmConductor.MakeConductorKernel());
IModeSingleton modeSingleton = ModeSingleton.Instance;

//Console Application container
var consoleBuilder = new ContainerBuilder();
consoleBuilder.Register(c => new RunLlmConsole(llm, router, modeSingleton)).As<IRun>();
var consoleContainer = consoleBuilder.Build();
var consoleRunner = consoleContainer.Resolve<IRun>();
await consoleRunner.Run();


