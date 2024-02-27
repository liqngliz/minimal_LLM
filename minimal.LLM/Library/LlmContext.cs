using LLama.Common;
using LLama;
using Configuration;
using LLama.Native;

namespace Context;

public record LlmContextInstance(ModelParams ModelParams, InferenceParams InferenceParams, string Prompt);

public class LlamaSharpContext : IContext<LlmContextInstance>
{   
    readonly Config _config;
    public LlamaSharpContext(Config config)
    {
        _config = config;
    }

    public async Task<LlmContextInstance> Init()
    {   

        string modelPath = _config.Model; 
        
        
        string prompt = File.ReadAllText(_config.Prompt);

    
        var parameters = new ModelParams(modelPath)
        {   
            ContextSize = _config.ContextSize,
            Seed = _config.Seed,
            GpuLayerCount = _config.GpuLayerCount
        };

        var inferenceParams = new InferenceParams() 
            { 
                Temperature = _config.Temperature, 
                RepeatPenalty = _config.RepeatPenalty, 
                AntiPrompts = _config.AntiPrompts, 
                MaxTokens = _config.MaxTokens 
            };
        inferenceParams.AntiPrompts = _config.AntiPrompts;
        
        return  new LlmContextInstance(parameters, inferenceParams, prompt);
    }
}