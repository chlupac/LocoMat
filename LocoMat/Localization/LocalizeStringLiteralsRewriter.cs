using LocoMat.Localization.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace LocoMat.Localization;

public class LocalizeStringLiteralsRewriter : CSharpSyntaxRewriter
{
    private readonly ILogger<LocalizeStringLiteralsRewriter> _logger;
    private readonly ResourceKeys _modelKeys;
    private readonly ILiteralFilter _filter;


    public LocalizeStringLiteralsRewriter(ResourceKeys modelKeys, ILiteralFilter filter) : base()
    {
        _logger = new Logger<LocalizeStringLiteralsRewriter>(new LoggerFactory());
        _modelKeys = modelKeys;
        _filter = filter;
    }

    public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        if (!node.IsKind(SyntaxKind.StringLiteralExpression))
            return base.VisitLiteralExpression(node);

        if (!IsLocalizable(node)) return base.VisitLiteralExpression(node);
        var message = $"Processing literal \"{node.Token.ValueText}\"";
        _logger.LogInformation(message);
        var text = node.Token.ValueText;
        var resourceKey = node.GetResourceKey();
        var invocationExpr = SyntaxFactory.ParseExpression($"D[\"{resourceKey}\"]");
        _modelKeys.TryAdd(resourceKey, text);
        return invocationExpr;
    }

    public bool IsLocalizable(LiteralExpressionSyntax literal)
    {
        if (literal == null) return false;
        return !_filter.IsProhibited(literal);
    }
}