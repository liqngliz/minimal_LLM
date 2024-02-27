using Autofac;
using Context;
using LLama.Common;
using Llm;
using NSubstitute;
using Run;
namespace minimal.LLM.Console.Test;

public class RunTest
{
    [Fact]
    public async void should_run()
    {   
        Illm<IAsyncEnumerable<string>, string, Context.LlmContextInstance, bool > llm = Substitute.For<Illm<IAsyncEnumerable<string>, string, Context.LlmContextInstance, bool>>();

        var consoleBuilder = new ContainerBuilder();
        consoleBuilder.Register(c => new RunLlmConsole(llm, true)).As<IRun>();
        
        var consoleContainer =consoleBuilder.Build();
        var consoleRunner = consoleContainer.Resolve<IRun>();
        
        var inferenceParams = new InferenceParams() 
            { 
                Temperature = 0.8f, 
                RepeatPenalty = 1.0f, 
                AntiPrompts = new List<string> { "User:" }, 
                MaxTokens = 1024 
            };
        ModelParams modelParams = new ModelParams("");
        LlmContextInstance llmInstance = new LlmContextInstance(modelParams,  inferenceParams, "someprompt");
        var mockData = new[] { "mock prompt response" };
        llm.InferParams().Returns(llmInstance);
        llm.Infer("someprompt").Returns(mockData.ToAsyncEnumerable());
        await consoleRunner.Run();
        llm.Received().Infer("someprompt");
    }
}