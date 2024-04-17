using Autofac;
using Context;
using LLama.Common;
using Llm;
using NSubstitute;
using Router;
using Run;
namespace minimal.LLM.Console.Test;

public class RunTest
{
    [Fact]
    public async void should_run()
    {   
        Illm<IAsyncEnumerable<string>, string, Context.LlmContextInstance, bool > llm = Substitute.For<Illm<IAsyncEnumerable<string>, string, Context.LlmContextInstance, bool>>();
        IRouter<RoutingPayload> router = Substitute.For<IRouter<RoutingPayload>>();
        IModeSingleton modeSingleton= Substitute.For<IModeSingleton>();

        var consoleBuilder = new ContainerBuilder();
        consoleBuilder.Register(c => new RunLlmConsole(llm, router, modeSingleton, true)).As<IRun>();
        
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
        LlmContextInstance llmInstance = new LlmContextInstance(modelParams,  inferenceParams);
        
        var mockData = new[] { "mock prompt response" };
        llm.InferParams().Returns(llmInstance);
        llm.Infer(Arg.Any<string>()).Returns(mockData.ToAsyncEnumerable());
        modeSingleton.UseRouting().Returns(false);

        await consoleRunner.Run();
        llm.Received().Infer(Arg.Any<string>());
    }
}