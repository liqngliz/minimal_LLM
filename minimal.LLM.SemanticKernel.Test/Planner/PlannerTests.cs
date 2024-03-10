﻿using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using LLama;
using LLama.Abstractions;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using minimal.LLM.SemanticKernel.Plugins;
using Planner;
using Reasoners;
using Planner.Functions;
using Planner.Parameters;

namespace PlannerTests;

[Collection("Sequential")]
public class PlannerTests
{   
    private Planner.IPlanner<Task<List<Tuple<KernelFunction, KernelArguments>>>, KernelPlan> sut;
    private IKernelBuilder _builder;
    readonly IModule<Config> _module;
    readonly IFactory<ILLamaExecutor> _factory;

    public PlannerTests()
    {   
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
        var llmContainer = _module.Container();
        _factory = llmContainer.Resolve<IFactory<ILLamaExecutor>>();
    }
    [Fact]
    public async void Should_return_plan_for_prompt()
    {   
        var ex = (StatelessExecutor)_factory.Make(typeof(StatelessExecutor));
        _builder = Kernel.CreateBuilder();
        _builder.Services.AddKeyedSingleton<IChatCompletionService>("local-llama", new LLamaSharpChatCompletion(ex));
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        _builder.Services.AddKeyedSingleton<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>("local-llama-reasoner-factory", reasonerFactory);
        _builder.Plugins.AddFromType<MathPlugin>();
        Kernel kernel = _builder.Build();
        KernelPlan kPlan = new(kernel, "what is the square root of 9");
        sut = new Planner.Planner(new SubPlannerFunctions(), new SubPlannerParameters());
        var plans = await sut.Plan(kPlan);
        Assert.Equal("Sqrt", plans[0].Item1.Name);
        List<string> results = new List<string>();
        foreach(var plan in plans)
        {   
            var function = plan.Item1;
            var args = plan.Item2;
            var res = (await kernel.InvokeAsync(function, args)).ToString();
            results.Add(res);
        }
        Assert.Contains("3", results);
    }
}