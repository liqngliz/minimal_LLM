using Microsoft.SemanticKernel;
using Planner.Validators;

namespace Planner.StepPlanner;
public record StepResult(string Output, bool Final, FunctionResult FunctionResult = null);
public record StepInput(string Input, KernelFunction Function, Kernel Kernel);
public class StepPlanner : IPlanner<Task<StepResult>, StepInput>
{
    readonly IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> _parameterPlanner;
    readonly IPlanner<Task<Validation>, KernelParamValidationPlan> _validationPlanner;

    readonly string _success;
    readonly string _failure;
    public StepPlanner(
        IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> parameterPlanner,
        IPlanner<Task<Validation>, KernelParamValidationPlan> validationPlanner,  StepPlannerTemplate stepPlannerTemplate = null)
    {   
        if(stepPlannerTemplate == null) stepPlannerTemplate = new();
        _parameterPlanner = parameterPlanner;
        _validationPlanner = validationPlanner;
        _success = string.IsNullOrEmpty(stepPlannerTemplate.success)? stepPlannerTemplate.success.ToDefaultSuccess() : stepPlannerTemplate.success;
        _failure = string.IsNullOrEmpty(stepPlannerTemplate.failure)? stepPlannerTemplate.failure.ToDefaultFailure() : stepPlannerTemplate.failure;
    }
    private Steps step = Steps.Function;
    private Dictionary<KernelParameterMetadata, string> parameters;
    private KernelParameterMetadata inProgress;
    private List<KernelParameterMetadata> completed = new List<KernelParameterMetadata>();
    private KernelArguments KernelArgs = new KernelArguments();
    public async Task<StepResult> Plan(StepInput inputs)
    {   
        var kernelFunction = inputs.Function;
        var kernel = inputs.Kernel;
        if(step == Steps.Function)
        {
            parameters = await _parameterPlanner.Plan(kernelFunction);
            if(parameters.Count() == 0)
            {
                FunctionResult result = await kernel.InvokeAsync(kernelFunction, new KernelArguments());
                step = Steps.Function;
                return new(_success, true, result);
            }
            step = Steps.Parameters;
        }

        if(step == Steps.Parameters)
        {
            var notProccessed = parameters.Where(x => !completed.Contains(x.Key)).First();
            inProgress = notProccessed.Key;
            var question = notProccessed.Value;
            step = Steps.Validation;
            return new(question, false);
        }

        if(step == Steps.Validation)
        {
            var validation = await _validationPlanner.Plan(new(inProgress, inputs.Input));
            if(!validation.Valid) return new(validation.Value.ToString(), false);

            KernelArgs.Add(validation.KernelParameter.Name, validation.Value);
            completed.Add(inProgress);

            if(!parameters.Select(x => x.Key).All(x => completed.Contains(x)))
            {
                var notProccessed = parameters.Where(x => !completed.Contains(x.Key)).First();
                inProgress = notProccessed.Key;
                var question = notProccessed.Value;
                return new(question, false);
            }
            else
            {   
                try
                {
                    FunctionResult result = await kernel.InvokeAsync(kernelFunction, KernelArgs);
                    step = Steps.Function;
                    reset();
                    return new(_success, true, result);
                }
                catch(Exception ex)
                {   
                    var failedMsg = _failure
                        .Replace("{function}", kernelFunction.Name)
                        .Replace("{inputs}", string.Join(",", KernelArgs.Select(x => x.Value.ToString())))
                        .Replace("{error}", ex.Message);
                    reset();
                    return new(ex.Message, true, null);
                }
            }
        }

        throw new NotImplementedException();
    }

    private void reset()
    {   
        step = Steps.Function;
        inProgress = null;
        completed = new List<KernelParameterMetadata>();
        KernelArgs = new KernelArguments();

    }

    private enum Steps
    {
        Function,
        Parameters,
        Validation
    }
}

public record StepPlannerTemplate(string success = null, string failure = null);

public static class StepPlannerExtesions
{   
    public static string ToDefaultSuccess(this string input) => "<|im_start|>System\nSemantic kernel has successfully invoked a plugin native function<|im_end|>Prohibere";
    public static string ToDefaultFailure(this string input) => "<|im_start|>System\nSemantic kernel has failed invoked a plugin native function {function} for your {inputs} and resulted in error {error}<|im_end|>Prohibere";

}
