﻿
using Microsoft.SemanticKernel;
using UtilsExt;

namespace Planner.FunctionSelector;
public record FunctionOptions(List<KernelFunctionMetadata> KernelFunctions, Kernel Kernel, string Prompt);
public record FunctionSelection(bool Valid, string output = null, List<KernelFunction> KernelFunctions = null);

public record FunctionSelectorTemplate(string Success, string Error, string Many, string None);

public class SubPlannerFunctionSelector : IPlanner<FunctionSelection, FunctionOptions>
{   
    readonly string _error;
    readonly string _many;
    readonly string _none;
    readonly string _success;
    public SubPlannerFunctionSelector(FunctionSelectorTemplate functionSelectorTemplate = null)
    {   
        if (functionSelectorTemplate == null){
            _error = "".ToDefaultErrorReply();
            _many = "".ToDefaultManyReply();
            _none = "".ToDefaultNoneReply();
            _success = "".ToDefaultReply();
        }
        else
        {
            _error = functionSelectorTemplate.Error;
            _many = functionSelectorTemplate.Many;
            _none = functionSelectorTemplate.None;
            _success = functionSelectorTemplate.Success;
        }
    }

    public FunctionSelection Plan(FunctionOptions inputs)
    {   
        var userInput = inputs.Prompt;
        var pluginNames = inputs.KernelFunctions.Select(x => x.PluginName).WithIndex().ToList();
        var functionNames = inputs.KernelFunctions.Select(x => x.Name).WithIndex().ToList();
        var functionDescriptions = inputs.KernelFunctions.Select(x => x.Description).WithIndex().ToList();
        var matrix = inputs.Prompt.ToFlatCharacterStringMatrix().ToList();

        List<KernelFunction> results = new List<KernelFunction>();
        FunctionSelection result = null;
        functionNames.ForEach(x =>{
            try
            {
            string name = x.item;
            int index = x.index;
            string description = functionDescriptions[index].item;
            string pluginName = pluginNames[index].item;
            var matches = matrix.FilterLevenshteinMatch(name);
            
            string best = "";

            if(matches.Any())
                best = matches.First().GetWordFromOrigin();   
            
            if(!string.IsNullOrEmpty(best) && best.LevenshteinMatch(name))
                results.Add(inputs.Kernel.Plugins.GetFunction(pluginName, name));
            }
            catch (Exception e)
            {
                result = new (false, _error.Replace("{exception}", e.Message));
            }
        });

        if(result != null)
            return result;

        string output = _success;

        switch(results.Count)
        {
            case 0:
                output = _none;
                result = new (false, output);
                break;
            case 1:
                var function = results.Single();
                var parameters = function.Metadata.Parameters.Select(x => $"\n'{x.ParameterType} - {x.Name} : {x.Description}'").ToList();
                var parametersTxt = parameters.Count > 0 ? string.Join("", parameters) : "None";
                output = _success
                    .Replace("{function}",function.Name)
                    .Replace("{description}",function.Description)
                    .Replace("{parameters}", parametersTxt);
                result = new (true, output, results);
                break;
            default:
                output = _many.Replace("{functions}", string.Join(", ", results.Select(x => x.Name)));
                result = new (false, output);
                break;
        }
    
        return result;
    }
}

public static class SubPlannerFunctionSelectorExtensions
{
    public static string ToDefaultErrorReply(this string input) => "<|im_start|>Bob\nYour input could not be parsed and resulted in the following error '{exception}'<|im_end|>Prohibere";
    public static string ToDefaultManyReply(this string input) => "<|im_start|>Bob\nYour input matches multiple functions {functions} please choose one.<|im_end|>Prohibere";
    public static string ToDefaultNoneReply(this string input) => "<|im_start|>Bob\nYour input matches no functions.<|im_end|>Prohibere";
    public static string ToDefaultReply(this string input) => "<|im_start|>Bob\nYou have selected '{function}' : '{description}' that expects: {parameters}.<|im_end|>Prohibere";
    
}