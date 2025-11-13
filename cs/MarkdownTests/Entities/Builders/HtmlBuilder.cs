using System.Text;
using Markdown.Enums;
using Markdown.Models;
using Markdown.Models.SyntaxTreeModels;

namespace Markdown.Entities.Builders
{
    /// <summary>
    /// Преобразует абстрактное синтаксическое дерево (AST) в HTML-код.
    /// Выполняет обход дерева в глубину и генерирует соответствующие HTML-теги для каждого узла.
    /// </summary>
    /// <param name="tree">Абстрактное синтаксическое дерево для преобразования</param>
    /// <returns>HTML-код, соответствующий структуре исходного дерева</returns>
    /// <remarks>
    /// Для каждого типа узла AST генерирует соответствующий HTML-тег:
    /// - Заголовки → h1
    /// - Жирный текст → strong
    /// - Курсив → em
    /// - Параграфы → p
    /// </remarks>
    public class HtmlBuilder : IBuilder
    {
        private readonly StringBuilder _htmlBuilder;
        private readonly Dictionary<NodeType, string> _tagMapping;

        public HtmlBuilder()
        {
            _htmlBuilder = new StringBuilder();
            _tagMapping = new Dictionary<NodeType, string>
            {
                { NodeType.Document, "" },
                { NodeType.Paragraph, "p" },
                { NodeType.Header, "h1" },
                { NodeType.Bold, "strong" },
                { NodeType.Italic, "em" },
                { NodeType.Text, "" }
            };
        }

        public string Build(ISyntaxTree tree)
        {
            if (tree == null)
                return string.Empty;

            _htmlBuilder.Clear();
            BuildNodes(tree.Tree);
            return _htmlBuilder.ToString();
        }

        private void BuildNodes(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                BuildNode(node);
            }
        }

        private void BuildNode(Node node)
        {
            switch (node.Type)
            {
                case NodeType.Text:
                    BuildTextNode(node);
                    break;
                case NodeType.Document:
                    BuildDocumentNode(node);
                    break;
                default:
                    BuildFormattedNode(node);
                    break;
            }
        }

        private void BuildTextNode(Node node)
        {
            if (!string.IsNullOrEmpty(node.Value))
            {
                var escapedText = EscapeHtml(node.Value);
                _htmlBuilder.Append(escapedText);
            }
        }

        private void BuildDocumentNode(Node node)
        {
            if (node.ChildrenNodes != null)
            {
                BuildNodes(node.ChildrenNodes);
            }
        }

        private void BuildFormattedNode(Node node)
        {
            var tagName = _tagMapping[node.Type];

            if (!string.IsNullOrEmpty(tagName))
            {
                _htmlBuilder.Append($"<{tagName}>");
            }

            if (node.ChildrenNodes != null && node.ChildrenNodes.Count > 0)
            {
                BuildNodes(node.ChildrenNodes);
            }
            else if (!string.IsNullOrEmpty(node.Value))
            {
                BuildTextNode(node);
            }

            if (!string.IsNullOrEmpty(tagName))
            {
                _htmlBuilder.Append($"</{tagName}>");
            }
        }

        private string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Эскейпинг HTML-символов для безопасности
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}