using NUnit.Framework;
using Markdown.Models;
using Markdown.Models.SyntaxTree;
using Markdown.Enums;

namespace Markdown.MarkdownTests.SyntaxTreeTests
{
    [TestFixture]
    public class SyntaxTreeTests
    {
        [Test]
        public void Parse_ItalicText_ReturnsItalicNode()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.ItalicsStart),
                new Token(TokenType.Text, "окруженный с двух сторон"),
                new Token(TokenType.ItalicsEnd)
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];
            Assert.That(paragraph.Type, Is.EqualTo(NodeType.Paragraph));
            Assert.That(paragraph.ChildrenNodes, Has.Count.EqualTo(1));

            var italicNode = paragraph.ChildrenNodes[0];
            Assert.That(italicNode.Type, Is.EqualTo(NodeType.Italic));
            Assert.That(italicNode.ChildrenNodes, Has.Count.EqualTo(1));
            Assert.That(italicNode.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(italicNode.ChildrenNodes[0].Value, Is.EqualTo("окруженный с двух сторон"));
        }

        [Test]
        public void Parse_BoldText_ReturnsBoldNode()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.BoldStart),
                new Token(TokenType.Text, "выделенный текст"),
                new Token(TokenType.BoldEnd)
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];
            Assert.That(paragraph.Type, Is.EqualTo(NodeType.Paragraph));

            var boldNode = paragraph.ChildrenNodes[0];
            Assert.That(boldNode.Type, Is.EqualTo(NodeType.Bold));
            Assert.That(boldNode.ChildrenNodes, Has.Count.EqualTo(1));
            Assert.That(boldNode.ChildrenNodes[0].Value, Is.EqualTo("выделенный текст"));
        }

        [Test]
        public void Parse_EscapedUnderscore_ReturnsTextNode()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Text, "_Вот это_")
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];
            Assert.That(paragraph.Type, Is.EqualTo(NodeType.Paragraph));
            Assert.That(paragraph.ChildrenNodes, Has.Count.EqualTo(1));
            Assert.That(paragraph.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(paragraph.ChildrenNodes[0].Value, Is.EqualTo("_Вот это_"));
        }

        [Test]
        public void Parse_BoldInsideItalic_WorksCorrectly()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.BoldStart),
                new Token(TokenType.Text, "внутри двойного "),
                new Token(TokenType.ItalicsStart),
                new Token(TokenType.Text, "одинарное работает"),
                new Token(TokenType.ItalicsEnd),
                new Token(TokenType.BoldEnd)
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];

            var boldNode = paragraph.ChildrenNodes[0];
            Assert.That(boldNode.Type, Is.EqualTo(NodeType.Bold));
            Assert.That(boldNode.ChildrenNodes, Has.Count.EqualTo(2));

            Assert.That(boldNode.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(boldNode.ChildrenNodes[0].Value, Is.EqualTo("внутри двойного "));

            var italicNode = boldNode.ChildrenNodes[1];
            Assert.That(italicNode.Type, Is.EqualTo(NodeType.Italic));
            Assert.That(italicNode.ChildrenNodes[0].Value, Is.EqualTo("одинарное работает"));
        }

        [Test]
        public void Parse_ItalicInsideBold_DoesNotWork()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.ItalicsStart),
                new Token(TokenType.Text, "внутри одинарного __двойное__ не работает"),
                new Token(TokenType.ItalicsEnd)
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];

            var italicNode = paragraph.ChildrenNodes[0];
            Assert.That(italicNode.Type, Is.EqualTo(NodeType.Italic));

            Assert.That(italicNode.ChildrenNodes, Has.Count.EqualTo(1));
            Assert.That(italicNode.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(italicNode.ChildrenNodes[0].Value, Is.EqualTo("внутри одинарного __двойное__ не работает"));
        }

        [Test]
        public void Parse_UnderscoresWithNumbers_RemainText()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Text, "цифрами_12_3")
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];
            Assert.That(paragraph.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(paragraph.ChildrenNodes[0].Value, Is.EqualTo("цифрами_12_3"));
        }

        [Test]
        public void Parse_WordParts_CanBeFormatted()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Text, "в нач"),
                new Token(TokenType.ItalicsStart),
                new Token(TokenType.Text, "але"),
                new Token(TokenType.ItalicsEnd)
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];
            Assert.That(paragraph.ChildrenNodes, Has.Count.EqualTo(2));
            Assert.That(paragraph.ChildrenNodes[0].Value, Is.EqualTo("в нач"));
            Assert.That(paragraph.ChildrenNodes[1].Type, Is.EqualTo(NodeType.Italic));
            Assert.That(paragraph.ChildrenNodes[1].ChildrenNodes[0].Value, Is.EqualTo("але"));
        }

        [Test]
        public void Parse_DifferentWords_NotFormatted()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Text, "ра"),
                new Token(TokenType.ItalicsStart),
                new Token(TokenType.Text, "зных сл"),
                new Token(TokenType.ItalicsEnd),
                new Token(TokenType.Text, "овах")
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];

            Assert.That(paragraph.ChildrenNodes, Has.Count.EqualTo(3));
            Assert.That(paragraph.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(paragraph.ChildrenNodes[1].Type, Is.EqualTo(NodeType.Italic));
            Assert.That(paragraph.ChildrenNodes[2].Type, Is.EqualTo(NodeType.Text));
        }

        [Test]
        public void Parse_EmptyFormatting_RemainsText()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Text, "____")
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var paragraph = result[0];
            Assert.That(paragraph.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(paragraph.ChildrenNodes[0].Value, Is.EqualTo("____"));
        }

        [Test]
        public void Parse_HeaderWithFormatting_CreatesHeaderWithNestedFormatting()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Header),
                new Token(TokenType.Text, "Заголовок "),
                new Token(TokenType.BoldStart),
                new Token(TokenType.Text, "с "),
                new Token(TokenType.ItalicsStart),
                new Token(TokenType.Text, "разными"),
                new Token(TokenType.ItalicsEnd),
                new Token(TokenType.Text, " символами"),
                new Token(TokenType.BoldEnd),
                new Token(TokenType.Newline)
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(1));
            var header = result[0];
            Assert.That(header.Type, Is.EqualTo(NodeType.Header));
            Assert.That(header.ChildrenNodes, Has.Count.EqualTo(2));

            Assert.That(header.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(header.ChildrenNodes[0].Value, Is.EqualTo("Заголовок "));

            var boldNode = header.ChildrenNodes[1];
            Assert.That(boldNode.Type, Is.EqualTo(NodeType.Bold));
            Assert.That(boldNode.ChildrenNodes, Has.Count.EqualTo(3));

            Assert.That(boldNode.ChildrenNodes[0].Type, Is.EqualTo(NodeType.Text));
            Assert.That(boldNode.ChildrenNodes[0].Value, Is.EqualTo("с "));

            var italicNode = boldNode.ChildrenNodes[1];
            Assert.That(italicNode.Type, Is.EqualTo(NodeType.Italic));
            Assert.That(italicNode.ChildrenNodes[0].Value, Is.EqualTo("разными"));

            Assert.That(boldNode.ChildrenNodes[2].Type, Is.EqualTo(NodeType.Text));
            Assert.That(boldNode.ChildrenNodes[2].Value, Is.EqualTo(" символами"));
        }

        [Test]
        public void Parse_MultipleParagraphs_SeparatesCorrectly()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Text, "Первый абзац"),
                new Token(TokenType.Newline),
                new Token(TokenType.Text, "Второй абзац"),
                new Token(TokenType.Newline),
                new Token(TokenType.Newline),
                new Token(TokenType.Text, "Третий абзац")
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].Type, Is.EqualTo(NodeType.Paragraph));
            Assert.That(result[1].Type, Is.EqualTo(NodeType.Paragraph));
            Assert.That(result[2].Type, Is.EqualTo(NodeType.Paragraph));
        }

        [Test]
        public void Parse_ComplexDocument_CreatesCorrectStructure()
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Header),
                new Token(TokenType.Text, "Заголовок"),
                new Token(TokenType.Newline),
                new Token(TokenType.Text, "Обычный "),
                new Token(TokenType.ItalicsStart),
                new Token(TokenType.Text, "курсив"),
                new Token(TokenType.ItalicsEnd),
                new Token(TokenType.Text, " и "),
                new Token(TokenType.BoldStart),
                new Token(TokenType.Text, "жирный"),
                new Token(TokenType.BoldEnd),
                new Token(TokenType.Newline),
                new Token(TokenType.Newline),
                new Token(TokenType.Text, "Новый абзац")
            };

            var syntaxTree = new SyntaxTree(tokens);
            var result = syntaxTree.Tree;

            Assert.That(result, Has.Count.EqualTo(3));

            Assert.That(result[0].Type, Is.EqualTo(NodeType.Header));

            var firstParagraph = result[1];
            Assert.That(firstParagraph.Type, Is.EqualTo(NodeType.Paragraph));
            Assert.That(firstParagraph.ChildrenNodes, Has.Count.EqualTo(4));

            var secondParagraph = result[2];
            Assert.That(secondParagraph.Type, Is.EqualTo(NodeType.Paragraph));
            Assert.That(secondParagraph.ChildrenNodes[0].Value, Is.EqualTo("Новый абзац"));
        }
    }
}