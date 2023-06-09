using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LocoMat.Localization.Filters;

public class MethodCallFilter : BaseLiteralFilter
{
    private readonly string[] _methodNames = { "ToString", "Format", "Get" };

    public MethodCallFilter()
    {
    }

    public MethodCallFilter(string[] methodNames)
    {
        if (methodNames != null) _methodNames = methodNames;
    }

    public MethodCallFilter(string methodName)
    {
        if (methodName != null) _methodNames = new string[] { methodName };
    }

    public override string Name => "Method calls";
    public override string Description => "Literals used as arguments in specific method calls";

    public override bool IsProhibited(LiteralExpressionSyntax literal)
    {
        var invocation = literal?.Parent?.Parent?.Parent as InvocationExpressionSyntax;
        var memberAccess = invocation?.Expression as MemberAccessExpressionSyntax;
        var text = memberAccess?.Name.Identifier.Text;
        var isProhibited = text != null && _methodNames.Contains(text);
        return isProhibited;
    }
}
