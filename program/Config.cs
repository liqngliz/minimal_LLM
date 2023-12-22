namespace Configuration;

public record Config(uint ContextSize, uint Seed , int GpuLayerCount, int MaxTokens, float Temperature, float RepeatPenalty, string Model ,string Prompt); 

