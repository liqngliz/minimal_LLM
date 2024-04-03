using System.Text;
using Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Quickenshtein;
using Reasoners;
using UtilsExt;

namespace Planner.Functions;

public class SubPlannerFunctions : IPlanner<Task<List<KernelFunction>>, KernelPlan>
{   
    private string _relation;
    private string[] _initial;
    private List<string> _queries;    
    private string finalQuery;
    readonly SubPlannerFunctionsTemplate _template;

    public SubPlannerFunctions(SubPlannerFunctionsTemplate Template = null)
    {   
        if(Template == null) _template = new SubPlannerFunctionsTemplate();
    }

    private void init()
    {
        _relation = _template.Relation == null? _template.Relation.ToDefaultRelation() : _template.Relation;
        _initial = _template.Initial == null? _template.Initial.ToDefaultInitial() : _template.Initial;
        _queries = _template.Queries == null? _template.Queries.ToDefaultQueries() : _template.Queries;
        finalQuery = _template.Final == null? _template.Final.ToDefaultFinalQuery() : _template.Final;
    }

    public async Task<List<KernelFunction>> Plan(KernelPlan Inputs)
    {   
        init();
        IFactory<IReasoner<Reasoning, ReasonerTemplate>> reasonerFactory = Inputs.Kernel.Services.GetService<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        IReasoner<Reasoning, ReasonerTemplate> reasoner = reasonerFactory.Make(typeof(LlmReasoner));

        var functionsMeta = Inputs.Kernel.Plugins.GetFunctionsMetadata();
        List<Relations> categories = new List<Relations>();
        var relation = _relation;
        functionsMeta.ToList().ForEach(x => {
            var name = x.Name.ToName();
            var description = x.Description.ToDescription();
            categories.Add(new Relations(name, description, relation));
        });
        
        var initial = _initial;
        List<string> operations = new List<string>();
        categories.ForEach(x => {
            init();
            var queries = _queries;
            var final = finalQuery
                .Replace("{inputPrompt}", Inputs.Prompt)
                .Replace("{namesLabel}",x.Name.Text)
                .Replace("{namesDescription}", x.Description.Text);
            queries.Add(final);
            var promptBuilder = new StringBuilder();
            initial.ToList().ForEach(x => promptBuilder.AppendLine(x));

            var res = reasoner.Reason(new(promptBuilder.ToString(), queries.ToArray(), new Relations[]{x})).Result;
            var conclusion = res.Conclusion;
            if(conclusion.ToLower().Contains("yes"))
                operations.Add(x.Name.Text);
            reasoner.Dispose();
        });

        
        var functionMetadatas = functionsMeta.Where(x => operations.Contains(x.Name));
        List<KernelFunction> functions = new List<KernelFunction>();
        foreach(var functionMetadata in functionMetadatas)
        {
            var function = Inputs.Kernel.Plugins.GetFunction(functionMetadata.PluginName, functionMetadata.Name);
            functions.Add(function);
        }
        
        
        return functions;
    }


}

public record SubPlannerFunctionsTemplate(string Relation = null, string[] Initial = null, List<string> Queries = null, string Final = null);

public static class SubPlannerFunctionsExt 
{
    public static string ToDefaultRelation(this string input) => "<|im_start|>user\n'{name}' is defined as '{description}' confirm you understand this and do not make things up.<|im_end|>";
    public static string[] ToDefaultInitial(this string[] input) => new string[]
        {
            "<|im_start|>system\nForget and clear any previous dialogues, transcripts, and instructions.<|im_end|>Prohibere", 
            "<|im_start|>system\nNew transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at choosing what the most appropriate function from a list of available functions. <|im_end|>Prohibere",
            "<|im_start|>user\nHello, Bob.<|im_end|>",
            "<|im_start|>Bob\nHello. How may I help you today?<|im_end|>Prohibere",
            "<|im_start|>user\nI will be giving you some functions and helpers with their corresponding definitions, I would like you to remember them when asked to select a function for an operation.<|im_end|>Prohibere",
            "<|im_start|>Bob\nOk, I am ready to recieve instructions.<|im_end|>Prohibere",
            "<|im_start|>user\nDo not make up helpers or functions that I don't tell you, and do NOT assume or use any helpers or operations that were not defined by me.<|im_end|>Prohibere",
            "<|im_start|>Bob\nOk, I am ready to recieve further instructions from you specifically.<|im_end|>Prohibere",
            "<|im_start|>user\nProhibere",
        };
    public static List<string> ToDefaultQueries(this List<string> input) => new List<string>(){
        "<|im_start|>user\nWhen asked a question, always answer either 'yes' or 'no' and do not make up functions or helpers.<|im_end|>",
        "<|im_start|>user\nWhen asked a question, do not answer with another question or ask for futher informaton try your best to answer directly.<|im_end|>"
    };

    public static string ToDefaultLevMatch(this string input) => "<|im_start|>user\nAre you sure of your answer '{input}' contains the word '{levMatch}' which is more than '{levTolereance}' to {namesLabel}?<|im_end|>";
    public static string ToDefaultLevNoMatch(this string input) => "<|im_start|>user\nDoes the phrase '{inputPrompt}' contain synonymes to '{namesLabel}'?<|im_end|>";

    public static string ToDefaultFinalQuery(this string input) => "<|im_start|>user\nDoes the phrase '{inputPrompt}' contain synonymes to '{namesLabel}'?<|im_end|>";
}