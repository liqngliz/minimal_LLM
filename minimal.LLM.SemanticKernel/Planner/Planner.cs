using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LLama.Batched;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Reasoners;

namespace Planner;

public record KernelPlan(Kernel Kernel, string Prompt);
public class Planner : IPlanner<Task<List<Tuple<KernelFunction, KernelArguments>>>, KernelPlan>
{   
    public async Task<List<Tuple<KernelFunction, KernelArguments>>> CreatePlanAsync(KernelPlan Inputs)
    {   
        IReasoner<Reasoning, ReasonerTemplate> reasoner = Inputs.Kernel.Services.GetRequiredKeyedService<IReasoner<Reasoning, ReasonerTemplate>>("local-llama-reasoner");
        var functionsMeta = Inputs.Kernel.Plugins.GetFunctionsMetadata();
        List<Relations> categories = new List<Relations>();
        var relation = "<|im_start|>user\nThe label '{name}' corresponds to the description '{description}'.<|im_end|>";
        var names = new List<string>();
        functionsMeta.ToList().ForEach(x => {
            var name = x.Name.ToName();
            var description = x.Description.ToDescription();
            categories.Add(new Relations(name, description, relation));
            names.Add(name.Text);
        });

        var none = "None".ToName();
        var noneDesc = "No helper or function matches".ToDescription();
        categories.Add(new Relations(none, noneDesc, relation));
        names.Add(none.Text);
        
        var initial = new string[]
        {
            "<|im_start|>system\nForget and clear any previous dialogues, transcripts, and instructions.<|im_end|>Prohibere", 
            "<|im_start|>system\nNew transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at choosing what the most appropriate function from a list of available functions. <|im_end|>Prohibere",
            "<|im_start|>user\nHello, Bob.<|im_end|>",
            "<|im_start|>Bob\nHello. How may I help you today?<|im_end|>Prohibere",
            "<|im_start|>user\nI will be giving you some functions and helpers with their corresponding descriptions, I would like you to remember them when asked to select a function for an operation.<|im_end|>Prohibere",
            "<|im_start|>Bob\nOk, I am ready to recieve instructions.<|im_end|>Prohibere",
            "<|im_start|>user\nDo not make up helpers or functions that I don't tell you, and do NOT assume or use any helpers or operations that were not defined by me.<|im_end|>Prohibere",
            "<|im_start|>Bob\nOk, I am ready to recieve further instructions from you specifically.<|im_end|>Prohibere",
            "<|im_start|>user\nProhibere",
        };
        var namesLabel= string.Join(", ", names);

        var queries = new List<string>();
        queries.Add($"<|im_start|>user\nWhen asked about which function or functions to use answer with it's label in ''.<|im_end|>");
        queries.Add($"<|im_start|>user\nWhen asked about which function or functions to use answer only with relevant labels.<|im_end|>");
        queries.Add($"<|im_start|>user\nFor {Inputs.Prompt} which function should be used '{namesLabel}'?<|im_end|>");

        var promptBuilder = new StringBuilder();
        initial.ToList().ForEach(x => promptBuilder.AppendLine(x));
        var res = reasoner.Reason(new(promptBuilder.ToString(), queries.ToArray(), categories.ToArray())).Result;
        var conclusion = res.Conclusion;
        var operations = names.Where(x => conclusion.Contains(x)).ToList();
        
        var functionMetadatas = functionsMeta.Where(x => operations.Contains(x.Name));
        var pluginFuncTuples= new List<Tuple<KernelFunction, KernelArguments>>();

        foreach(var functionMetadata in functionMetadatas)
        {
            var function = Inputs.Kernel.Plugins.GetFunction(functionMetadata.PluginName, functionMetadata.Name);
            var functionParams = function.Metadata.Parameters.ToList();
            List<Relations> categoriesParam = new List<Relations>();
            functionParams.ForEach(x => 
            {
                var name = x.Name.ToName();
                var description = x.Description.ToDescription();
                var type = x.ParameterType.Name;
                var relation = "<|im_start|>user\nThe label '{name}' corresponds to the parameter description '{description}' of type {type}.<|im_end|>".Replace("{type}", type);
                categoriesParam.Add(new Relations(name, description, relation));
            });

            categoriesParam.Add
            (
                new Relations("None".ToName(), "No parameter matches description or type".ToDescription(), "<|im_start|>user\nThe label '{name}' corresponds to the description '{description}'.<|im_end|>")
            );

            

            //var funcRes = await Inputs.Kernel.InvokeAsync(function, new(){{"number1", 9}});

            //Parameter parsing
            var parameterAssistant = new string[]
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

            var queriesParameters = new List<string>();
            queriesParameters.Add("<|im_start|>user\nWhen no value can be found, answer with 'null'.<|im_end|>");

            var questions = new List<string>();

            foreach(var functionParam in functionParams)
            {
                var queryText = $"<|im_start|>user\nFor {Inputs.Prompt} what should be the value of {functionParam.Name}?<|im_end|>";
                var queryText2 = $"<|im_start|>user\nFormat your previous answer and put the value in single quotes ''.<|im_end|>";
                questions.Add(queryText);
                queriesParameters.Add(queryText);
                queriesParameters.Add(queryText2);
            }
            IReasoner<Reasoning, ReasonerTemplate> reasonerParam = Inputs.Kernel.Services.GetRequiredKeyedService<IReasoner<Reasoning, ReasonerTemplate>>("local-llama-reasoner");
            var resParam = reasonerParam.Reason(new(parameterAssistant.ToString(), queriesParameters.ToArray(), categoriesParam.ToArray())).Result;
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
            
            pluginFuncTuples.Add(new(function, args));
        }
        
        return pluginFuncTuples;
    }
}

public static class PlannerExtensions
{
    public static T ConvertObject<T>(this object input) {
        return (T) Convert.ChangeType(input, typeof(T));
    }
}