using System.Text;
using Rewrite.Core;
using Rewrite.RewriteJava;
using Rewrite.RewriteJava.Tree;
using ExecutionContext = Rewrite.Core.ExecutionContext;

namespace Rewrite.DemoDotnetRecipes;

public class InvertAssertionRecipe : Recipe
{
    private const string ASSERT_TRUE = "Xunit.Assert.True";

    public override ITreeVisitor<Tree, ExecutionContext> GetVisitor()
    {
        return new InvertAssertionVisitor();
    }

    private class InvertAssertionVisitor : JavaVisitor<ExecutionContext>
    {
        public override J.MethodInvocation VisitMethodInvocation(J.MethodInvocation method, ExecutionContext ctx)
        {
            // Assert.True(!a);
            var mi = (J.MethodInvocation)base.VisitMethodInvocation(method, ctx);

            if (!ASSERT_TRUE.EndsWith(ExtractName(mi)) || !IsUnaryOperatorNot(mi)) return mi;

            var unary = (J.Unary)mi.Arguments[0];

            return mi.WithArguments([unary.Expression]).WithName(mi.Name.WithSimpleName("False"));
        }

        private static string ExtractName(J.MethodInvocation mi)
        {
            return (mi.Select is J.Identifier i ? (i.SimpleName + ".") : "") + mi.Name.SimpleName;
        }

        private static bool IsUnaryOperatorNot(J.MethodInvocation method)
        {
            if (method.Arguments.Count == 1 && method.Arguments[0] is J.Unary unary)
            {
                return unary.Operator.Equals(J.Unary.Type.Not);
            }

            return false;
        }
    };
}