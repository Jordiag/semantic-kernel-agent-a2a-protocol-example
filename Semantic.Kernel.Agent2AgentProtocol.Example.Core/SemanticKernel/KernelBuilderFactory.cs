using Microsoft.SemanticKernel;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.SemanticKernel;

public static class KernelBuilderFactory
{
    public static Microsoft.SemanticKernel.Kernel Build()
    {
        var builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();
        // You can add AI services here, if needed (OpenAI, AzureOpenAI, etc.)
        return builder.Build();
    }
}
