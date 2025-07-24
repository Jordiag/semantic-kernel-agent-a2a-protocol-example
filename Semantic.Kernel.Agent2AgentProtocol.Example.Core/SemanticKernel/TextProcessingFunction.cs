using Microsoft.SemanticKernel;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.SemanticKernel;

public static class TextProcessingFunction
{
    public static KernelFunction GetFunctionByType(string type)
    {
        return type.ToUpperInvariant() switch
        {
            "REVERSE" => KernelFunctionFactory.CreateFromMethod(Reverse),
            "UPPER" => KernelFunctionFactory.CreateFromMethod(Upper),
            _ => throw new InvalidOperationException($"Unknown function type: {type}")
        };
    }

    private static string Reverse(string input) =>
        new([.. input.Reverse()]);

    private static string Upper(string input) =>
        input.ToUpperInvariant();
}
