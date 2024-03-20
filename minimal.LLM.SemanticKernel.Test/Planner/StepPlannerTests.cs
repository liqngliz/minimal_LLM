using Microsoft.SemanticKernel;
using minimal.LLM.SemanticKernel.Plugins;
using Planner;
using Planner.Parameters;
using Planner.StepPlanner;
using Planner.Validators;

namespace PlannerTests;

public class StepPlannerTest
{
    private IPlanner<Task<StepResult>, string> sut;
    readonly IKernelBuilder _builder;
    readonly Kernel _kernel;
    public StepPlannerTest()
    {
        _builder = Kernel.CreateBuilder();
        _builder.Plugins.AddFromType<MathPlugin>();
        _builder.Plugins.AddFromType<FilePlugin>();
        _kernel = _builder.Build();
        
    }

    [Theory]
    [InlineData("MathPlugin", "Sqrt", new string[]{"What is the root of 9","9"}, "3")]
    [InlineData("MathPlugin", "Add", new string[]{"What is 9 plus 10","9","10"}, "19")]
    [InlineData("MathPlugin", "Multiply", new string[]{"What is 9 * 10","9","bad input","10"}, "90")]
    [InlineData("FilePlugin", "GetFileList", new string[]{"What files are here"}, "test_text.txt")]
    [InlineData("FilePlugin", "GetContent", new string[]{"what is the content of test_text?","test_text.txt"}, @"5. Tax and legal issues for start-ups
You should choose the right legal structure to suit your circumstances and register it with HM Revenue and Customs. Should you choose the limited company route you are required to register with Companies House.

You may need to seek specialist advice on intellectual property protection to cover copyright, trade marking, design registration or patenting.

It is vital to keep accurate records and pay tax and National Insurance.

6. Business planning for start-ups
You should plan your business carefully before you start up. The headings in a business plan can be thought of as a checklist of questions you need to ask yourself to reassure yourself that your venture will work.

Writing the plan down helps to clarify your thinking and identifies where you intend to get to and how you intend to get there.

Read our guide on how to prepare a business plan and download our business plan template.")]
    public async void should_invoke_semantic_function(string plugin, string function, string[] sequence, string expected)
    {
        var func = _kernel.Plugins.GetFunction(plugin, function);
        sut = new StepPlanner(new SubPlannerParameter(), new SubPlannerValidator(), func, _kernel);
        for(var i = 0; i < sequence.Length; i++)
        {
            var res = await sut.Plan(sequence[i]);
            
            if(i == sequence.Length - 1) 
                Assert.Equivalent(expected, res.FunctionResult.ToString());
        }
    }
}