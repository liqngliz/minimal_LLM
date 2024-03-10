using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Reasoners;

namespace Planner.Parameters;

public class SubPlannerParameters : IPlanner<Task<KernelArguments>, KernelFunctionPlan>
{   

    readonly string _relation;
    readonly string[] _initial;
    readonly string _noParam;
    readonly string _finalQuery;
    readonly string _format;

    public SubPlannerParameters(string relation = null, string[] initial = null, string final = null, string format = null, string noParam = null)
    {
        _relation = relation == null? relation.ToDefaultRelation() : relation;
        _initial = initial == null? initial.ToDefaultInitial() : initial;

        _finalQuery = final == null? final.ToDefaultFinalQuery() : final;
        _format = format == null ? format.ToDefaultFormatQuery() : format;
        _noParam = noParam == null ? noParam.ToDefaultNullResultQuery() : noParam;
    }
    public async Task<KernelArguments> Plan(KernelFunctionPlan Inputs)
    {
        KernelFunction function = Inputs.KernelFunction;

        var functionMetadata = function.Metadata;
        var functionParams = function.Metadata.Parameters.ToList();
        List<Relations> categoriesParam = new List<Relations>();

        functionParams.ForEach(x => 
        {
            var name = x.Name.ToName();
            var description = x.Description.ToDescription();
            var type = x.ParameterType.Name;
            var relation = _relation;
            relation = relation.Replace("{type}", type).Replace("{function}", function.Description);
            categoriesParam.Add(new Relations(name, description, relation));
        });

        //Parameter parsing
        var parameterAssistant = _initial;

        var queriesParameters = new List<string>();
        
        var questions = new List<string>();

        foreach(var functionParam in functionParams)
        {
            var queryText = _finalQuery.Replace("{inputPrompt}", Inputs.Prompt).Replace("{functionParamName}", functionParam.Name).Replace("{functionDescription}", functionMetadata.Description);
            var format = _format.Replace("{functionParamName}", functionParam.Name);
            questions.Add(queryText);
            queriesParameters.Add(_noParam);
            queriesParameters.Add(queryText);
            queriesParameters.Add(format);
        }

        IReasoner<Reasoning, ReasonerTemplate> reasonerParam = Inputs.Kernel.Services.GetRequiredKeyedService<IReasoner<Reasoning, ReasonerTemplate>>("local-llama-reasoner");
            
        var promptBuilderParam = new StringBuilder();
        parameterAssistant.ToList().ForEach(x => promptBuilderParam.AppendLine(x));
        var resParam = reasonerParam.Reason(new(promptBuilderParam.ToString(), queriesParameters.ToArray(), categoriesParam.ToArray())).Result;
        var transcript = resParam.Transcript;

        KernelArguments args = new KernelArguments();

        for(int i = 0; i < functionParams.Count; i++)
        {   
            var functionParam = functionParams[i];
            string paramName = functionParam.Name;
            Type? paramType = functionParam.ParameterType;
            var question = questions[i];
            var answerPosition = transcript.IndexOf(question);
            var answer = transcript[answerPosition + 3];
            var value = Regex.Match(answer, @"'(.+?)'").Groups[1].Value;
            var paramVal = Convert.ChangeType(value, paramType);
            args.Add(paramName, paramVal);
        }

        return args;
    }
}

public static class SubPlannerParametersExt 
{
    public static string ToDefaultRelation(this string input) => "<|im_start|>user\n'{name}' corresponds to the parameter '{description}' of type {type} used in the function that {function}.<|im_end|>";
    public static string[] ToDefaultInitial(this string[] input) => new string[]
        {
            "<|im_start|>system\nForget and clear any previous dialogues, transcripts, and instructions.<|im_end|>Prohibere", 
            "<|im_start|>system\nNew transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at reading text and finding the parameters that should be used for a function. <|im_end|>Prohibere",
            "<|im_start|>user\nHello, Bob.<|im_end|>",
            "<|im_start|>Bob\nHello. How may I help you today?<|im_end|>Prohibere",
            "<|im_start|>user\nI will be giving you some parameters with their corresponding descriptions, I would like you to remember them when asked to select the values that match the parameter.<|im_end|>Prohibere",
            "<|im_start|>Bob\nOk, I am ready to recieve instructions.<|im_end|>Prohibere",
            "<|im_start|>user\nDo not make up parameters or values that I don't tell you, and do NOT assume or use any parameters or values that were not defined by me.<|im_end|>Prohibere",
            "<|im_start|>Bob\nOk, I am ready to recieve further instructions from you specifically.<|im_end|>Prohibere",
            "<|im_start|>user\nProhibere",
        };
    public static List<string> ToDefaultQueries(this List<string> input) => new List<string>(){
    };
    public static string ToDefaultNullResultQuery(this string input) => "<|im_start|>user\nWhen no value can be found, answer with 'null'.<|im_end|>";
    public static string ToDefaultFinalQuery(this string input) => "<|im_start|>user\nto answer {inputPrompt} what should be the value of {functionParamName} to be used in a function that {functionDescription}?<|im_end|>";
    public static string ToDefaultFormatQuery(this string input) => "<|im_start|>user\nThat is correct! Can you repeat that but format the value of {functionParamName} between single quotes?<|im_end|>";
}