
using Markdown;

namespace markdown
{
    public class Markdown
    {
        public static string ParseMarkdownToHtml(string text)
        {
            var tokens = Scanner.TextToListOfTokens(text);

            var AST = SyntaxTreeBuilder.BuildTreeFromTokensList(tokens);

            return RenderTextFromAST(AST);
        }

        private static string RenderTextFromAST(List<Node> AST)
        {
            throw new NotImplementedException();
        }
    }
}