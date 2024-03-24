using Microsoft.SemanticKernel;
using Newtonsoft.Json;



namespace Planner.Validators;

public record Validation(bool Valid, KernelParameterMetadata KernelParameter, object Value);
public class SubPlannerValidator : IPlanner<Task<Validation>, KernelParamValidationPlan>
{   
    readonly string _errorMessage;

    public SubPlannerValidator(string errorMessage = null)
    {
        _errorMessage = string.IsNullOrEmpty(errorMessage) ? errorMessage.ToDefaultErrorReply() : errorMessage;
    }

    public async Task<Validation> Plan(KernelParamValidationPlan Inputs)
    {   
        var parameterType = Inputs.Parameter.ParameterType;
        try
        {   
            var output = Convert.ChangeType(Inputs.Input, parameterType);
            return new(true, Inputs.Parameter, output);
        }
        catch(Exception ex)
        {
            return new Validation(false, Inputs.Parameter, _errorMessage.Replace("{exception}", ex.Message).Replace("{type}", parameterType.FullName));
        }
    }
}

public static class SubPlannerValidatorExtension
{
    public static string ToDefaultErrorReply(this string exception) => "<|im_start|>Bob\nYour input could not be parsed as {type} and resulted in the following error '{exception}'<|im_end|>Prohibere";
    
}