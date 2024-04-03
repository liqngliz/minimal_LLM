
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

public class Router : IRouter<RoutingPayload>
{   
    readonly ConductorKernel _conductorKernel;
    readonly ReasonerTemplate _routingInteractiveTemplate;
    readonly string _interactiveQuestion;
    readonly string _interactiveResponse;

    readonly ReasonerTemplate _routingFunctionsTemplate;
    readonly string _functionsQuestion;
    readonly string _functionsResponse;

    public Router(
        ConductorKernel conductorKernel, 
        ReasonerTemplate routingInteractiveTemplate = null,
        string interactiveQuestion = null, 
        string interactiveResponse = null,
        ReasonerTemplate routingFunctionsTemplate = null,
        string functionsQuestion = null,
        string functionsResponse = null
        )
    {
        _conductorKernel = conductorKernel;
        
        //interactive mode semantic functions
        if(routingInteractiveTemplate == null)
        {   
            string[] initial = new string[]{}.ToDefaultInitialInterative();
            var promptBuilder = new StringBuilder();
            initial.ToList().ForEach(x => promptBuilder.AppendLine(x));
            _routingInteractiveTemplate = new(promptBuilder.ToString(), new string[]{}, new Relations[]{});
        }
        else
        {
            _routingInteractiveTemplate = routingInteractiveTemplate;
        }
        _interactiveQuestion = string.IsNullOrEmpty(interactiveQuestion) ? interactiveQuestion.ToDefaultInteractiveQueries() : interactiveQuestion;
        _interactiveResponse = string.IsNullOrEmpty(interactiveResponse) ? interactiveResponse.ToDefaultInteractiveReponse() : interactiveResponse;


        //planned function mode semantic functions
        if(routingFunctionsTemplate == null)
        {   
            string[] initial = new string[]{}.ToDefaultInitialFunction();
            var promptBuilder = new StringBuilder();
            initial.ToList().ForEach(x => promptBuilder.AppendLine(x));
            _routingFunctionsTemplate = new(promptBuilder.ToString(), new string[]{}, new Relations[]{});
        }
        else
        {
            _routingFunctionsTemplate = routingFunctionsTemplate;
        }
        _functionsQuestion = string.IsNullOrEmpty(functionsQuestion) ? functionsQuestion.ToDefaultInteractiveQueries() : functionsQuestion;
        _functionsResponse = string.IsNullOrEmpty(functionsResponse) ? functionsResponse.ToDefaultInteractiveReponse() : functionsResponse;


    }
    
    public RoutingPayload route(RoutingPayload routingInput)
    {   
        switch(routingInput.Mode)
        {
            case Mode.Interactive:
                var reasoner = _conductorKernel.Factory.Make(typeof(LlmReasoner));
                var question = _interactiveQuestion.Replace("{question}", routingInput.Text);
                ReasonerTemplate template = _routingInteractiveTemplate with {Queries = new string[]{question}};
                var routeToPlugins = reasoner.Reason(template).Result;
                reasoner.Dispose();
                bool useFunctions = routeToPlugins.Conclusion.ToLower().Contains("yes");
                
                if(!useFunctions)
                    return new(Mode.Interactive, "");
                
                var functionPlan = _conductorKernel.Function.Plan(new(_conductorKernel, routingInput.Text)).Result;

                if(functionPlan == null && functionPlan.Count > 0) 
                    return new(Mode.Interactive, "");
                
                List<string> functions = functionPlan.Select(x => "A function called " + x.Name + " that " + x.Description).ToList();
                var promptBuilder = new StringBuilder();
                functions.ToList().ForEach(x => promptBuilder.AppendLine(x));
                var interactiveResponseText = _interactiveResponse.Replace("{functions}", promptBuilder.ToString());

                return new RoutingPayload(Mode.FunctionPlan, interactiveResponseText, functionPlan);

            case Mode.FunctionPlan:
                var functionsMeta = routingInput.Functions;
                functions = functionsMeta.Select(x => "A function called " + x.Name + " that " + x.Description).ToList();
                promptBuilder = new StringBuilder();
                functions.ToList().ForEach(x => promptBuilder.AppendLine(x));
                int promptFuncPos = new Random().Next(0, functionsMeta.Count());
                var function1 = functionsMeta[promptFuncPos];
                var reasonerFunction = _conductorKernel.Factory.Make(typeof(LlmReasoner));
                question = _functionsQuestion.Replace("{question}", routingInput.Text);
                var initial = _routingFunctionsTemplate.Initial
                    .Replace("{functions}", promptBuilder.ToString())
                    .Replace("{function1name}",function1.Name)
                    .Replace("{function1desc}",function1.Description);
                template = _routingFunctionsTemplate with 
                { 
                    Initial= initial,
                    Queries = new string[]{question} 
                };
                var routeFunctions = reasonerFunction.Reason(template).Result;
                reasonerFunction.Dispose();
                useFunctions = routeFunctions.Conclusion.ToLower().Contains("yes");
                
                if(!useFunctions)
                    return new(Mode.Interactive, "");
                
                var functionList = _conductorKernel.Function.Plan(new(_conductorKernel, routingInput.Text)).Result;
                var function = functionList.Where(x => routingInput.Functions.Contains(x)).FirstOrDefault();
                if(function == null) return new(Mode.Result, "");
                var steps = _conductorKernel.Steps.Plan(new(routingInput.Text, function, _conductorKernel)).Result;
                
                if(steps.Final) 
                    return new(Mode.Result, steps.FunctionResult.ToString());

                return new(Mode.StepsPlan, steps.Output, new (){function}, steps);

            default: 
                return new(Mode.Interactive, "");
        }
    }
}

public static class RouterExtensions
{
    public static string ToDefaultInteractiveQueries(this string input) =>  "<|im_start|>Is the following related to files or documents? '{question}' <|im_end|>";
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
    public static string ToDefaultInteractiveReponse(this string input) => "<|im_start|>Bob\nI have the following native functions that could help \n{functions}\n would you like to use any of these? Please include the function name you would like to use in your response.<|im_end|>Prohibere";


     public static string ToDefaultFunctionQueries(this string input) =>  "<|im_start|>Does the following phrase '{question}' say to use any of the functions I gave you? <|im_end|>";

    public static string[] ToDefaultInitialFunction(this string[] input) => new string[]
    {
        "<|im_start|>system\nForget and clear any previous dialogues, transcripts, and instructions.<|im_end|>Prohibere", 
        "<|im_start|>system\nNew transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at answering questions and understands instructions very well.<|im_end|>Prohibere",
        "<|im_start|>user\nHello, Bob.<|im_end|>",
        "<|im_start|>Bob\nHello. How may I help you today?<|im_end|>Prohibere",
        "<|im_start|>user\nI want you to remember the names of the following functions '{functions}'?.<|im_end|>Prohibere",
        "<|im_start|>Bob\nOk I will remember these function, and won't make up or use any other function names that you didn't give me.<|im_end|>Prohibere",
        "<|im_start|>user\nDoes the following phrase 'No do not use {function1name}, let just continue to talk?' say to use any of the functions I gave you?.<|im_end|>Prohibere",
        "<|im_start|>Bob\n No, the phrase tells us to not use {function1name} and to just continue to talk.<|im_end|>Prohibere",
        "<|im_start|>user\nDoes the following phrase 'Why not, lets try and see what we can get from {function1name}' say to use any of the functions I gave you?.<|im_end|>Prohibere",
        "<|im_start|>Bob\n Yes, the phrase tells us to use {function1name}.<|im_end|>Prohibere",
        "<|im_start|>user\nDoes the following phrase 'sure go ahead and use the function that {function1desc}' say to use any of the functions I gave you?.<|im_end|>Prohibere",
        "<|im_start|>Bob\n Yes, the phrase tells us to use {function1name} which {function1desc}.<|im_end|>Prohibere",
        "<|im_start|>user\nDoes the following phrase 'sure go ahead and use the function CookFries that cooks some fries for me' say to use any of the functions I gave you?.<|im_end|>Prohibere",
        "<|im_start|>Bob\n No, the phrase tells us to use a function call CookFries that you did not give me.<|im_end|>Prohibere",
        "<|im_start|>user\nShall we start with the questions and instructions? Always include 'yes' if a sentence wants to use a function that I gave you!<|im_end|>Prohibere",
        "<|im_start|>Bob\nSure I will remember that! go ahead and ask away!.<|im_end|>Prohibere",
        "<|im_start|>user\nProhibere",
    };
}

public record RoutingPayload(Mode Mode, string Text, List<KernelFunction> Functions = null, StepResult Step = null);