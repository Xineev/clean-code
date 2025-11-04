using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdown.Entities.Builders;
using Markdown.Entities.Parsers;
using Markdown.Entities.Renderers;
using Markdown.Models;
using Markdown.Models.SyntaxTree;

namespace Markdown.Entities.Converters
{
    /// <summary>
    /// Выполняет полный цикл преобразования Markdown в HTML.
    /// Координирует работу токенизатора, парсера и HTML-билдера.
    /// </summary>
    /// <param name="text">Исходный Markdown-текст</param>
    /// <returns>Строка с HTML-разметкой</returns>
    /// <remarks>
    /// Последовательность преобразований:
    /// 1. Токенизация исходного текста
    /// 2. Построение абстрактного синтаксического дерева (AST)
    /// 3. Рендеринг AST в HTML-формат
    /// </remarks>
    public class MarkdownToHtmlConverter : IConverter
    {
        public string Convert(string text)
        {
            var tokens = new MarkdownTokenizer().Tokenize(text);
            var ast = new SyntaxTree(tokens);
            var convertedText = new HtmlBuilder().Build(ast);
            return convertedText;
        }
    }
}
