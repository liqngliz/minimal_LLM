
using Microsoft.SemanticKernel;
using UtilsExt;

namespace Planner.FunctionSelector;
public record FunctionSelection(bool Valid, string output = null, KernelFunction KernelFunction = null);

public class SubPlannerFunctionSelector : IPlanner<FunctionSelection, KernelPlan>
{   
    readonly string _error;
    readonly string _many;
    readonly string _none;
    readonly string _reply;
    public SubPlannerFunctionSelector(string reply, string error = null, string many = null, string none = null)
    {
        _error = string.IsNullOrEmpty(error)? error.ToDefaultErrorReply(): error;
        _many = string.IsNullOrEmpty(many)? many.ToDefaultManyReply(): many;
        _none = string.IsNullOrEmpty(none)? none.ToDefaultNoneReply(): none;
        _reply = string.IsNullOrEmpty(reply)? reply.ToDefaultReply() : reply;
    }

    public FunctionSelection Plan(KernelPlan inputs)
    {   
        var userInput = inputs.Prompt;
        var functionNames = inputs.Kernel.Plugins.GetFunctionsMetadata().Select(x => x.Name).WithIndex().ToList();
        var functionDescriptions = inputs.Kernel.Plugins.GetFunctionsMetadata().Select(x => x.Description).WithIndex().ToList();

        

        throw new NotImplementedException();
    }
}

public static class SubPlannerFunctionSelectorExtensions
{
    public static string ToDefaultErrorReply(this string exception) => "<|im_start|>Bob\nYour input could not be parsed and resulted in the following error '{exception}'<|im_end|>Prohibere";
    public static string ToDefaultManyReply(this string exception) => "<|im_start|>Bob\nYour input matches multiple functions {functions} please choose one.<|im_end|>Prohibere";
    public static string ToDefaultNoneReply(this string exception) => "<|im_start|>Bob\nYour input matches no functions.<|im_end|>Prohibere";
    public static string ToDefaultReply(this string exception) => "<|im_start|>Bob\nYou have selected '{function}' : '{description}' that expects {parameters}.<|im_end|>Prohibere";
    
}