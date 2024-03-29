﻿using System.Reflection;
using Autofac;
using Configuration;
using Factory;
using IoC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using minimal.LLM.SemanticKernel;
using Planner;
using Planner.Functions;
using Planner.Parameters;
using Planner.StepPlanner;
using Planner.Validators;
using Reasoners;

namespace LlmKernelTest;

public class LlmKernelTest
{
    readonly ILlmConductorKernel _sut;
    readonly IModule<Config> _module;
    public LlmKernelTest()
    {
        var configurationJSON = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" );
        _module = new IoCModule(configurationJSON);
        var reasonerFactory = _module.Container().Resolve<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        List<object> plugins = new List<object>(){};
        _sut = new LlmConductorKernel(plugins, reasonerFactory);
    }

    [Theory]
    [InlineData(typeof(IFactory<IReasoner<Reasoning, ReasonerTemplate>>),typeof(ReasonerFactory))]
    [InlineData(typeof(IPlanner<Task<Validation>, KernelParamValidationPlan>),typeof(SubPlannerValidator))]
    [InlineData(typeof(IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>),typeof(SubPlannerParameter))]
    [InlineData(typeof(IPlanner<Task<StepResult>, StepInput>),typeof(StepPlanner))]
    [InlineData(typeof(IPlanner<Task<List<KernelFunction>>, KernelPlan>),typeof(SubPlannerFunctions))]
    public void should_resolve_types(Type serviceType, Type expected)
    {
        Kernel conductor = _sut.MakeConductorKernel();
        var services = conductor.Services;
        var types = services.GetService(serviceType);
        Assert.IsType(expected, types);
    }

    [Fact]
    public void should_throw_with_incompatible_kernel_services()
    {
        IKernelBuilder badConductorBuilder = Kernel.CreateBuilder();
        Kernel conductor = _sut.MakeConductorKernel();
        var validation = (IPlanner<Task<Validation>, KernelParamValidationPlan>?)conductor.Services.GetService(typeof(IPlanner<Task<Validation>, KernelParamValidationPlan>)); 
        var parameter = (IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>?)conductor.Services.GetService(typeof(IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>));
        _ = badConductorBuilder.Services.AddSingleton(validation);
        _ = badConductorBuilder.Services.AddSingleton(parameter);
        Kernel badConductor = badConductorBuilder.Build();
        Exception exception = null;
        try
        {
            ConductorKernel conductorKernel = badConductor;
        } 
        catch(Exception ex)
        {
            exception = ex;
        }

        Assert.True(exception != null);
    }
}