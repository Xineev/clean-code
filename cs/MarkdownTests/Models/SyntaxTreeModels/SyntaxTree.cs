using Markdown.Enums;
using Markdown.Models.SyntaxTreeModels;

namespace Markdown.Models.SyntaxTree
{
    public class SyntaxTree : ISyntaxTree
    {
        /// <summary>
        /// Коллекция корневых узлов абстрактного синтаксического дерева
        /// </summary>
        public List<Node> Tree { get; }

        private List<Token> tokens;
        private int currentIndex;

        /// <summary>
        /// Строит абстрактное синтаксическое дерево (AST) из потока токенов.
        /// Анализирует последовательность токенов и создает иерархическую структуру узлов,
        /// отражающую семантическую структуру исходного документа.
        /// </summary>
        /// <param name="tokens">Коллекция токенов, полученная от лексического анализатора</param>
        public SyntaxTree(List<Token> tokens)
        {
            this.tokens = tokens ?? new List<Token>();
            currentIndex = 0;
            Tree = ParseDocument();
        }

        private List<Node> ParseDocument()
        {
            var documentNodes = new List<Node>();

            while (currentIndex < tokens.Count)
            {
                var node = ParseBlock();
                if (node != null)
                {
                    documentNodes.Add(node);
                }
            }

            return documentNodes;
        }

        private Node ParseBlock()
        {
            if (currentIndex >= tokens.Count)
                return null;

            var token = tokens[currentIndex];

            return token.Type switch
            {
                TokenType.Header => ParseHeader(),
                TokenType.Newline => ParseNewline(),
                _ => ParseParagraph()
            };
        }

        private Node ParseHeader()
        {
            currentIndex++;

            var headerContent = new List<Node>();
            while (CanContinueParsingHeaderContent())
            {
                var inlineNode = ParseInline();
                if (inlineNode != null)
                {
                    headerContent.Add(inlineNode);
                }
            }

            if (HasNewlineTokenAtCurrentPosition())
            {
                currentIndex++;
            }

            return new Node(NodeType.Header, headerContent, null);
        }

        private Node ParseParagraph()
        {
            var paragraphContent = new List<Node>();

            while (CanContinueParsingParagraphContent())
            {
                var inlineNode = ParseInline();
                if (inlineNode != null)
                {
                    paragraphContent.Add(inlineNode);
                }
            }

            if (HasNewlineTokenAtCurrentPosition())
            {
                currentIndex++;
            }

            return paragraphContent.Count > 0
                ? new Node(NodeType.Paragraph, paragraphContent, null)
                : null;
        }

        private Node ParseNewline()
        {
            currentIndex++;
            return null;
        }

        private Node ParseInline()
        {
            if (currentIndex >= tokens.Count)
                return null;

            var token = tokens[currentIndex];

            return token.Type switch
            {
                TokenType.BoldStart => ParseBold(),
                TokenType.ItalicsStart => ParseItalic(),
                TokenType.Text => ParseText(),
                _ => HandleUnexpectedToken()
            };
        }

        private Node ParseBold()
        {
            currentIndex++;

            var boldContent = new List<Node>();

            while (CanContinueParsingBoldContent())
            {
                var inlineNode = ParseInline();
                if (inlineNode != null)
                {
                    boldContent.Add(inlineNode);
                }
            }

            if (HasBoldEndTokenAtCurrentPosition())
            {
                currentIndex++;
            }

            return new Node(NodeType.Bold, boldContent, null);
        }

        private Node ParseItalic()
        {
            currentIndex++;

            var italicContent = new List<Node>();

            while (CanContinueParsingItalicContent())
            {
                var inlineNode = ParseInline();
                if (inlineNode != null)
                {
                    italicContent.Add(inlineNode);
                }
            }

            if (HasItalicsEndTokenAtCurrentPosition())
            {
                currentIndex++;
            }

            return new Node(NodeType.Italic, italicContent, null);
        }

        private Node ParseText()
        {
            var token = tokens[currentIndex];
            currentIndex++;
            return new Node(NodeType.Text, null, token.Value);
        }

        private Node HandleUnexpectedToken()
        {
            currentIndex++;
            return null;
        }

        private bool CanContinueParsingHeaderContent()
        {
            return currentIndex < tokens.Count && tokens[currentIndex].Type != TokenType.Newline;
        }

        private bool HasNewlineTokenAtCurrentPosition()
        {
            return currentIndex < tokens.Count && tokens[currentIndex].Type == TokenType.Newline;
        }

        private bool CanContinueParsingParagraphContent()
        {
            return currentIndex < tokens.Count && tokens[currentIndex].Type != TokenType.Newline;
        }

        private bool CanContinueParsingBoldContent()
        {
            return currentIndex < tokens.Count && tokens[currentIndex].Type != TokenType.BoldEnd;
        }

        private bool HasBoldEndTokenAtCurrentPosition()
        {
            return currentIndex < tokens.Count && tokens[currentIndex].Type == TokenType.BoldEnd;
        }

        private bool CanContinueParsingItalicContent()
        {
            return currentIndex < tokens.Count && tokens[currentIndex].Type != TokenType.ItalicsEnd;
        }

        private bool HasItalicsEndTokenAtCurrentPosition()
        {
            return currentIndex < tokens.Count && tokens[currentIndex].Type == TokenType.ItalicsEnd;
        }
    }
}