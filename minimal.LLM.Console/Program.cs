﻿using System.Reflection;
using Autofac;
using Context;
using IoC;
using Llm;
using Run;
using Factory;
using Reasoners;
using minimal.LLM.Plugins;
using minimal.LLM.SemanticKernel;
;

//Llm Lib container
Console.WriteLine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
var llmContainer = new IoCModule(configurationJSON).Container();
var llm = llmContainer.Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool>>();

//Orchestration container
var reasonerFactory = llmContainer.Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
List<object> plugins = new List<object>(){ new FilePlugin() };
ILlmConductorKernel llmConductor = new LlmConductorKernel(plugins, reasonerFactory);


//Console Application container
var consoleBuilder = new ContainerBuilder();
consoleBuilder.Register(c => new RunLlmConsole(llm)).As<IRun>();
var consoleContainer = consoleBuilder.Build();
var consoleRunner = consoleContainer.Resolve<IRun>();
await consoleRunner.Run();


