using Microsoft.SemanticKernel;
using Planner.Validators;


namespace Planner.StepPlanner;
public record StepResult(string Output, bool Final, FunctionResult FunctionResult = null);
public class StepPlanner : IPlanner<Task<StepResult>, string>
{
    readonly IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> _parameterPlanner;
    readonly IPlanner<Task<Validation>, KernelParamValidationPlan> _validationPlanner;
    readonly KernelFunction _kernelFunction;
    readonly Kernel _kernel;
    readonly string _success;
    public StepPlanner(
        IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> parameterPlanner,
        IPlanner<Task<Validation>, KernelParamValidationPlan> validationPlanner,
        KernelFunction kernelFunction, Kernel kernel, string success = null)
    {
        _parameterPlanner = parameterPlanner;
        _validationPlanner = validationPlanner;
        _kernelFunction = kernelFunction;
        _kernel = kernel;
        _success = string.IsNullOrEmpty(success)? success.ToDefaultSuccess() : success;
    }
    private Steps step = Steps.Function;
    private Dictionary<KernelParameterMetadata, string> parameters;
    private KernelParameterMetadata inProgress;
    private List<KernelParameterMetadata> completed = new List<KernelParameterMetadata>();
    private KernelArguments ouput = new KernelArguments();
    public async Task<StepResult> Plan(string inputs)
    {   
        if(step == Steps.Function) 
        {
            parameters = await _parameterPlanner.Plan(_kernelFunction);
            if(parameters.Count() == 0)
            {
                FunctionResult result = await _kernel.InvokeAsync(_kernelFunction, new KernelArguments());
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
            var validation = await _validationPlanner.Plan(new(inProgress, inputs));
            if(!validation.Valid) return new(validation.Value, false);

            ouput.Add(validation.KernelParameter.Name, validation.Value);
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
                FunctionResult result = await _kernel.InvokeAsync(_kernelFunction, ouput);
                step = Steps.Function;
                return new(_success, true, result);
            }
        }

        throw new NotImplementedException();
    }

    private enum Steps
    {
        Function,
        Parameters,
        Validation
    }
}

public static class StepPlannerExtesions
{
    public static string ToDefaultSuccess(this string input) => "<|im_start|>System\nSemantic kernel has successfully invoked a plugin native function<|im_end|>Prohibere";
}
