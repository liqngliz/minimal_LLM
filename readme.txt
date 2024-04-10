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
    "Model": <GGUF model file location in bin>,
    "Prompt": <prompt.txt file location to initiate conversation>
}

The program was built and tested against a 4KM quantized Phi-2 OpenHermes Finetuned Model in GGUF format. 
This model needs to be added to your bin folders for testing and execution

2.7B Phi-2 Open Hermes Project page:
https://huggingface.co/g-ronimo/phi-2-OpenHermes-2.5/tree/main

LlammCPP used to quantize JSON model to GGUF format to use with LlamaSharp:
https://github.com/ggerganov/llama.cpp


