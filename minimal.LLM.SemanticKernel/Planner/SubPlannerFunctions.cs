using System.Text;
using Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Reasoners;

namespace Planner.Functions;

public class SubPlannerFunctions : IPlanner<Task<List<KernelFunction>>, KernelPlan>
{   
    readonly string _relation;
    readonly string[] _initial;
    readonly List<string> _queries;    
    private string finalQuery;

    public SubPlannerFunctions(SubPlannerFunctionsTemplate Template = null)
    {   
        if(Template == null) Template =new SubPlannerFunctionsTemplate();
        _relation = Template.Relation == null? Template.Relation.ToDefaultRelation() : Template.Relation;
        _initial = Template.Initial == null? Template.Initial.ToDefaultInitial() : Template.Initial;
        _queries = Template.Queries == null? Template.Queries.ToDefaultQueries() : Template.Queries;
        finalQuery = Template.Final == null? Template.Final.ToDefaultFinalQuery() : Template.Final;
    }

    public async Task<List<KernelFunction>> Plan(KernelPlan Inputs)
    {   
        IFactory<IReasoner<Reasoning, ReasonerTemplate>> reasonerFactory = Inputs.Kernel.Services.GetService<IFactory<IReasoner<Reasoning, ReasonerTemplate>>>();
        IReasoner<Reasoning, ReasonerTemplate> reasoner = reasonerFactory.Make(typeof(LlmReasoner));

        var functionsMeta = Inputs.Kernel.Plugins.GetFunctionsMetadata();
        List<Relations> categories = new List<Relations>();
        var relation = _relation;
        var names = new List<string>();
        functionsMeta.ToList().ForEach(x => {
            var name = x.Name.ToName();
            var description = x.Description.ToDescription();
            categories.Add(new Relations(name, description, relation));
            names.Add(name.Text);
        });

        var none = "None".ToName();
        var noneDesc = "Nothing is relevant or matches".ToDescription();
        categories.Add(new Relations(none, noneDesc, relation));
        names.Add(none.Text);
        
        var initial = _initial;
        var namesLabel= string.Join(", ", names);

        var queries = _queries;
        queries.Add(finalQuery.Replace("{inputPrompt}", Inputs.Prompt).Replace("{namesLabel}",namesLabel));

        var promptBuilder = new StringBuilder();
        initial.ToList().ForEach(x => promptBuilder.AppendLine(x));
        var res = reasoner.Reason(new(promptBuilder.ToString(), queries.ToArray(), categories.ToArray())).Result;
        var conclusion = res.Conclusion;
        var operations = names.Where(x => conclusion.Contains(x)).ToList();
        
        var functionMetadatas = functionsMeta.Where(x => operations.Contains(x.Name));
        List<KernelFunction> functions = new List<KernelFunction>();
        foreach(var functionMetadata in functionMetadatas)
        {
            var function = Inputs.Kernel.Plugins.GetFunction(functionMetadata.PluginName, functionMetadata.Name);
            functions.Add(function);
        }
        
        reasoner.Dispose();
        return functions;
    }
}

public record SubPlannerFunctionsTemplate(string Relation = null, string[] Initial = null, List<string> Queries = null, string Final = null);

public static class SubPlannerFunctionsExt 
{
    public static string ToDefaultRelation(this string input) => "<|im_start|>user\nThe label '{name}' corresponds to the description '{description}'.<|im_end|>";
    public static string[] ToDefaultInitial(this string[] input) => new string[]
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
    public static List<string> ToDefaultQueries(this List<string> input) => new List<string>(){
        "<|im_start|>user\nWhen asked about which function or functions to use answer with it's label in ''.<|im_end|>",
        "<|im_start|>user\nWhen asked about which function or functions to use answer only with relevant labels.<|im_end|>"
    };

    public static string ToDefaultFinalQuery(this string input) => "<|im_start|>user\nFor {inputPrompt} which function should be used '{namesLabel}'?<|im_end|>";

}