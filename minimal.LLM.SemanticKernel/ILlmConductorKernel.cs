using System.Dynamic;
using Configuration;
using Factory;
using IoC;
using JetBrains.Annotations;
using Microsoft.SemanticKernel;
using Planner;
using Planner.FunctionSelector;
using Planner.StepPlanner;
using Planner.Validators;
using Reasoners;

namespace minimal.LLM.SemanticKernel;

public interface ILlmConductorKernel
{
    public ConductorKernel MakeConductorKernel();
}

public struct ConductorKernel
{   
    readonly Kernel _kernel;
    readonly IPlanner<Task<Validation>, KernelParamValidationPlan> _validation;
    readonly IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> _parameter;
    readonly IPlanner<Task<StepResult>, StepInput> _steps;
    readonly IPlanner<FunctionSelection, FunctionOptions> _selector;
    readonly IPlanner<Task<List<KernelFunction>>, KernelPlan> _function;
    readonly IFactory<IReasoner<Reasoning, ReasonerTemplate>> _factory;
    public Kernel Kernel {get => _kernel;}
    public IPlanner<Task<Validation>, KernelParamValidationPlan> Validation {get => _validation;}
    public IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> Parameter {get => _parameter;}
    public IPlanner<Task<StepResult>, StepInput> Steps {get => _steps;}
    public IPlanner<FunctionSelection, FunctionOptions> Selector {get => _selector;}
    public IPlanner<Task<List<KernelFunction>>, KernelPlan> Function {get => _function;}
    public IFactory<IReasoner<Reasoning, ReasonerTemplate>> Factory {get => _factory;}

    public ConductorKernel(Kernel kernel)
    {   
        var validation = kernel.Services.GetService(typeof(IPlanner<Task<Validation>, KernelParamValidationPlan>)); 
        var parameter = kernel.Services.GetService(typeof(IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>));
        var steps = kernel.Services.GetService(typeof(IPlanner<Task<StepResult>, StepInput>));
        var selector = kernel.Services.GetService(typeof(IPlanner<FunctionSelection, FunctionOptions>));
        var function = kernel.Services.GetService(typeof(IPlanner<Task<List<KernelFunction>>, KernelPlan>));
        var factory = kernel.Services.GetService(typeof(IFactory<IReasoner<Reasoning, ReasonerTemplate>>));
        
        if(validation == null || parameter == null || steps == null || selector == null || function == null || factory == null)
            throw new InvalidOperationException();

        _kernel = kernel;
        _validation = (IPlanner<Task<Validation>, KernelParamValidationPlan>)validation;
        _parameter = (IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>)parameter;
        _steps = (IPlanner<Task<StepResult>, StepInput>) steps;
        _selector = (IPlanner<FunctionSelection, FunctionOptions>) selector;
        _function = (IPlanner<Task<List<KernelFunction>>, KernelPlan>) function;
        _factory = (IFactory<IReasoner<Reasoning, ReasonerTemplate>>) factory;

    }
    public static implicit operator Kernel(ConductorKernel conductor) => conductor.Kernel;
    public static implicit operator ConductorKernel(Kernel kernel) => new ConductorKernel(kernel);
}

