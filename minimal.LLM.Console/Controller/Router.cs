
using System.Text;
using Context;
using Factory;
using Llm;
using Microsoft.SemanticKernel;
using minimal.LLM.SemanticKernel;
using Planner;
using Planner.StepPlanner;
using Planner.Validators;
using Reasoners;

namespace minimal.LLM.Console.Router;

public class Router : IRouter<RoutedResult>
{   
    readonly IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> _parameterPlanner;
    readonly IPlanner<Task<Validation>, KernelParamValidationPlan> _validationPlanner;
    readonly IPlanner<Task<StepResult>, StepInput> _stepsPlanner;
    readonly IPlanner<Task<List<KernelFunction>>, KernelPlan> _functionsPlanner;
    readonly IFactory<IReasoner<Reasoning, ReasonerTemplate>> _reasonerFactory;

    public Router(
        IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> parameterPlanner,
        IPlanner<Task<Validation>, KernelParamValidationPlan> validationPlanner,
        IPlanner<Task<StepResult>, StepInput> stepsPlanner,
        IPlanner<Task<List<KernelFunction>>, KernelPlan> functionsPlanner,
        IFactory<IReasoner<Reasoning, ReasonerTemplate>> reasonerFactory
        )
    {
        _parameterPlanner = parameterPlanner;
        _functionsPlanner = functionsPlanner;
        _validationPlanner = validationPlanner;
        _stepsPlanner = stepsPlanner;
        _functionsPlanner = functionsPlanner;
        _reasonerFactory = reasonerFactory;
    }

    
    public RoutedResult route(InputMode inputMode, string input)
    {   
        var reasoner = _reasonerFactory.Make(typeof(LlmReasoner));
        string[] initial = new string[]{}.ToDefaultInitial();
        var promptBuilder = new StringBuilder();
        initial.ToList().ForEach(x => promptBuilder.AppendLine(x));
        var question = "".ToDefaultQueries().Replace("{question}", input);
        ReasonerTemplate template = new(promptBuilder.ToString(), new string[]{question}, new Relations[]{});
        var routeToFiles = reasoner.Reason(template).Result;
        RoutedResult routedResult = new(routeToFiles.Conclusion);
        reasoner.Dispose();
        return routedResult;
    }
}

public static class RouterExtensions
{
    public static string ToDefaultQueries(this string input) =>  "<|im_start|>Is the following related to files or documents? '{question}' <|im_end|>";

    public static string[] ToDefaultInitial(this string[] input) => new string[]
        {
            "<|im_start|>system\nForget and clear any previous dialogues, transcripts, and instructions.<|im_end|>Prohibere", 
            "<|im_start|>system\nNew transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at answering questions and understands instructions very well.<|im_end|>Prohibere",
            "<|im_start|>user\nHello, Bob.<|im_end|>",
            "<|im_start|>Bob\nHello. How may I help you today?<|im_end|>Prohibere",
            "<|im_start|>user\nDoes 'Can you give me the largest city in moscow and tell me what is 107 times 98?' contain synonymes for files or documents?.<|im_end|>Prohibere",
            "<|im_start|>Bob\nIt is asking about cities and multiplication and does not contain synonymes for files and document.<|im_end|>Prohibere",
            "<|im_start|>user\nDoes 'Can you check if you have a text file about startups?' contain synonymes for files or documents?.<|im_end|>Prohibere",
            "<|im_start|>Bob\n Yes text file is a type of file and could also be considered a type of document!<|im_end|>Prohibere",
            "<|im_start|>user\nShall we start with the questions and instructions? Always include 'yes' if a sentence contains files, documents, or somthing similar!<|im_end|>Prohibere",
            "<|im_start|>Bob\nSure I will remember that! go ahead and ask away!.<|im_end|>Prohibere",
            "<|im_start|>user\nProhibere",
        };

}

public record RoutedResult(string Output);