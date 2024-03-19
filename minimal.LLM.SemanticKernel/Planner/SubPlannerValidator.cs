using Microsoft.SemanticKernel;
using Newtonsoft.Json;



namespace Planner.Validators;

public record Validation(bool Valid, KernelParameterMetadata KernelParameter, dynamic Value);
public class SubPlannerValidator : IPlanner<Task<Validation>, KernelParamValidationPlan>
{
    public async Task<Validation> Plan(KernelParamValidationPlan Inputs)
    {   
        var parameterType = Inputs.Parameter.ParameterType;
        try
        {   
            var derserialized = JsonConvert.DeserializeObject(Inputs.Input, parameterType);
            return new(true, Inputs.Parameter, derserialized);
        }
        catch(Exception ex)
        {
            return new Validation(false, Inputs.Parameter, ex.ToErrorReply(parameterType));
        }
    }
}

public static class SubPlannerValidatorExtension
{
    public static string ToErrorReply(this Exception exception, Type type) => $"<|im_start|>Bob\nYour input could not be parsed as {type} and resulted in the following error '{exception.Message}'<|im_end|>Prohibere";
}