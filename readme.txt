Minimal C# wrapper for LLM's such as LLAMA 2

Compatible with any LLM in GGUF format.

Compiles to any x86, x64, ARM architecture

Runs as console app

Configurable GPU offloading (Max 20)

For JSON configuration (config.json)

{
    "ContextSize": <Size of Context number of tokens>,
    "Seed": <Seed randomized generation>,
    "GpuLayerCount": <Max 20, GPU offloading, reduce to reduce ram usuage>,
    "MaxTokens": <Max tokens per prompt>,
    "Temperature": <Temperature affect randomization>,
    "RepeatPenalty": <Repeat penalty, affects randomization>,
    "Model": <GGUF model file location>,
    "Prompt": <prompt.txt file location to initiate conversation>
}

To run program and tests, Mistral 7B or equivalent is required, add model to bin folder.

example file:
https://huggingface.co/TheBloke/Mistral-7B-Instruct-v0.2-GGUF/blob/main/mistral-7b-instruct-v0.2.Q2_K.gguf