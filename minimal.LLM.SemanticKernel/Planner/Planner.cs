using System.Text;
using LLama.Batched;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Reasoners;

namespace Planner;

public record KernelPlan(Kernel Kernel, string Prompt);

public class Planner : IPlanner<Task<string>, KernelPlan>
{   
    public async Task<string> CreatePlanAsync(KernelPlan Inputs)
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

        
        var queries = new List<string>();

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
        queries.Add($"<|im_start|>user\nWhen asked about which function to use answer with it's label in ''.<|im_end|>");
        queries.Add($"<|im_start|>user\nWhen asked about which function to use answer only with one label, the most relevant one.<|im_end|>");
        queries.Add($"<|im_start|>user\nFor {Inputs.Prompt} which function should be used '{namesLabel}'?<|im_end|>");

        var promptBuilder = new StringBuilder();
        initial.ToList().ForEach(x => promptBuilder.AppendLine(x));
        var res = reasoner.Reason(new(promptBuilder.ToString(), queries.ToArray(), categories.ToArray())).Result;
        var conclusion = res.Conclusion;
        var operation = names.Where(x => conclusion.Contains(x)).Single();
        
        return $"{operation}, [inputs]";
    }
}