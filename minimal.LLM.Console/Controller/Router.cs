
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
    readonly ConductorKernel _conductorKernel;
    readonly ReasonerTemplate _routingReasoningTemplate;
    readonly string _functionResponse;
    public Router(
        ConductorKernel conductorKernel, 
        ReasonerTemplate routingReasoningTemplate = null, 
        string functionResponse = null
        )
    {
        _conductorKernel = conductorKernel;
        
        if(routingReasoningTemplate == null)
        {   
            string[] initial = new string[]{}.ToDefaultInitialInterative();
            var promptBuilder = new StringBuilder();
            initial.ToList().ForEach(x => promptBuilder.AppendLine(x));
            _routingReasoningTemplate = new(promptBuilder.ToString(), new string[]{}, new Relations[]{});
        }
        else
        {
            _routingReasoningTemplate = routingReasoningTemplate;
        }

        _functionResponse = string.IsNullOrEmpty(functionResponse) ? functionResponse.ToDefaultFunction() : functionResponse;
    }
    
    public RoutedResult route(Mode mode, string input)
    {   
        bool useFunctions = false;
        RoutedResult routedResult = new(Mode.Interactive, "");

        if(mode == Mode.Interactive)
        {
            var reasoner = _conductorKernel.Factory.Make(typeof(LlmReasoner));
            var question = "".ToDefaultQueries().Replace("{question}", input);
            ReasonerTemplate template = _routingReasoningTemplate with {Queries = new string[]{question}};
            var routeToFiles = reasoner.Reason(template).Result;
            reasoner.Dispose();
            useFunctions = routeToFiles.Conclusion.ToLower().Contains("yes");
        }

        if(!useFunctions && mode == Mode.Interactive)
        {

        }

        if(useFunctions && mode == Mode.Interactive)
        {
            var functionPlan = _conductorKernel.Function.Plan(new(_conductorKernel, input)).Result;
            if(functionPlan == null && functionPlan.Count > 0) return routedResult;
            List<string> functions = functionPlan.Select(x => "A function called " + x.Name + " that " + x.Description).ToList();
            var promptBuilder = new StringBuilder();
            functions.ToList().ForEach(x => promptBuilder.AppendLine(x));
            routedResult = routedResult with {Mode = Mode.Planned, Output = _functionResponse.Replace("{functions}", promptBuilder.ToString())};
            return routedResult;
        }

        return routedResult;
    }
}

public static class RouterExtensions
{
    public static string ToDefaultQueries(this string input) =>  "<|im_start|>Is the following related to files or documents? '{question}' <|im_end|>";

    public static string[] ToDefaultInitialInterative(this string[] input) => new string[]
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

    public static string ToDefaultFunction(this string input) => "<|im_start|>Bob\nI have the following native functions that could help \n{functions}\n do you want to use them?<|im_end|>Prohibere";
}

public record RoutedResult(Mode Mode, string Output);