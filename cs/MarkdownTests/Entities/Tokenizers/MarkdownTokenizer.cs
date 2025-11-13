using System.Text;
using Markdown.Enums;
using Markdown.Models;

namespace Markdown.Entities.Parsers
{
    /// <summary>
    /// Tokenizer который работает с markdown текстом
    /// </summary>
    public class MarkdownTokenizer : ITokenizer
    {
        public List<Token> Tokenize(string text)
        {
            return TokenizeLines(TextToLines(text));
        }

        public List<string> TextToLines(string text)
        {
            return text.Split("\n").ToList();
        }

        public List<Token> TokenizeLines(IEnumerable<string> lines)
        {
            var tokens = new List<Token>();
            foreach (var line in lines)
            {
                tokens.AddRange(TokenizeLine(line));
                tokens.Add(new Token(TokenType.Newline));
            }
            return tokens;
        }

        /// <summary>
        /// Ключевые правила из спецификации:
        /// Приоритет обработки: экранирование → заголовки → жирный → курсив
        /// Вложенность: жирный может содержать курсив, но не наоборот
        /// Экранирование: \ отменяет разметку следующего символа
        /// Заголовки: только в начале строки с #
        /// Валидация: проверки на пробелы, цифры, пересечение слов
        /// Непарные теги: остаются как обычный текст
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public List<Token> TokenizeLine(string line)
        {
            var tokens = new List<Token>();
            var currentPosition = 0;
            var textBuffer = new StringBuilder();

            if (IsHeader(line))
            {
                ProcessHeader(line, ref currentPosition, tokens);
            }

            while (currentPosition < line.Length)
            {
                var currentChar = line[currentPosition];

                if (currentChar == '\\')
                {
                    ProcessEscapeCharacter(line, ref currentPosition, textBuffer);
                    continue;
                }

                if (IsBoldMarker(line, currentPosition))
                {
                    ProcessBoldMarker(line, ref currentPosition, tokens, textBuffer);
                    continue;
                }

                if (IsItalicsMarker(line, currentPosition))
                {
                    ProcessItalicsMarker(line, ref currentPosition, tokens, textBuffer, false);
                    continue;
                }

                textBuffer.Append(currentChar);
                currentPosition++;
            }

            if(textBuffer.Length > 0) FlushTextBufferIfNotEmpty(textBuffer, tokens);
            return tokens;
        }

        /// <summary>
        /// Метод обработки тэга курсива, превращает часть строки в набор токенов если она удовлетворяет условиям спецификации
        /// Дополнительно обрабатывает случай неправильной вложенности тэга курсива и полужирного тэга
        /// </summary>
        /// <param name="line"></param>
        /// <param name="currentPosition"></param>
        /// <param name="tokens"></param>
        /// <param name="originalTextBuffer"></param>
        /// <param name="isNestedCall"> - флаг для обработки случая когда находим курсив внутри полужирного текста</param>
        private void ProcessItalicsMarker(string line, ref int currentPosition, List<Token> tokens, StringBuilder originalTextBuffer, bool isNestedCall)
        {
            var isInWord = false;
            var startPosition = currentPosition;
            var foundBoldMarker = false;

            var textBuffer = new StringBuilder();

            if (IsMarkerInsideWord(line, currentPosition, false)) isInWord = true;

            SkipItalicsMarker(ref currentPosition);

            while (currentPosition < line.Length)
            {
                var currentChar = line[currentPosition];

                if (IsValidClosingMarker(line, currentPosition, false))
                {
                    var isEmptyWord = IsEmptyMarkedWord(currentPosition, startPosition, false);
                    if (isEmptyWord)
                    {
                        originalTextBuffer.Append(textBuffer);
                        return;
                    }

                    if (foundBoldMarker)
                    {
                        PrependMarkerToBuffer(textBuffer, false);
                        originalTextBuffer.Append(textBuffer);
                        return;
                    }

                    FlushTextBufferIfNotEmpty(originalTextBuffer, tokens);
                    AddTokensFromBufferWithSpecifiedTags(textBuffer, tokens, TokenType.ItalicsStart, TokenType.ItalicsEnd);
                    SkipItalicsMarker(ref currentPosition);
                    return;
                }

                if (currentChar == '_')
                {
                    if (line[currentPosition + 1] == '_')
                    {
                        if (isNestedCall)
                        {
                            AppendMarkerToBuffer(textBuffer, true);
                            SkipBoldMarker(ref currentPosition);

                            AppendMarkerToBuffer(originalTextBuffer, false);
                            originalTextBuffer.Append(textBuffer);
                            return;
                        }
                        //Меняем состояние флага на обратное
                        foundBoldMarker = !foundBoldMarker;
                    }
                }

                if (currentChar == ' ')
                {
                    if (isInWord)
                    {
                        AppendMarkerToBuffer(originalTextBuffer, false);
                        originalTextBuffer.Append(textBuffer);
                        return;
                    }

                }

                if (currentChar == '\\')
                {
                    ProcessEscapeCharacter(line, ref currentPosition, textBuffer);
                    continue;
                }

                textBuffer.Append(currentChar);
                currentPosition++;
            }

            PrependMarkerToBuffer(textBuffer, false);
            originalTextBuffer.Append(textBuffer);
        }

        /// <summary>
        /// Метод обработки тэга полужирного текста, превращает часть строки в набор токенов если она удовлетворяет условиям спецификации
        /// </summary>
        /// <param name="line"></param>
        /// <param name="currentPosition"></param>
        /// <param name="tokens"></param>
        /// <param name="originalTextBuffer"></param>
        private void ProcessBoldMarker(string line, ref int currentPosition, List<Token> tokens, StringBuilder originalTextBuffer)
        {
            var isClosingFound = false;
            var isInWord = IsMarkerInsideWord(line, currentPosition, true);
            var startPosition = currentPosition;

            var textBuffer = new StringBuilder();
            var innerTokens = new List<Token>();

            SkipBoldMarker(ref currentPosition);

            while (currentPosition < line.Length)
            {
                var currentChar = line[currentPosition];

                if (IsValidClosingMarker(line, currentPosition, true))
                {
                    var isEmptyWord =  IsEmptyMarkedWord(currentPosition, startPosition, true);
                    if (isEmptyWord)
                    {
                        AppendMarkerToBuffer(textBuffer, true);
                        originalTextBuffer.Append(textBuffer);
                        return;
                    }

                    //если нашли токены после вложенного вызова ProcessItalicsMarker 
                    if (innerTokens.Count > 0)
                    {
                        FlushTextBufferIfNotEmpty(originalTextBuffer, tokens);
                        AddNestedItalicsTokens(textBuffer, tokens, innerTokens);
                        SkipBoldMarker(ref currentPosition);
                        return;
                    }

                    FlushTextBufferIfNotEmpty(originalTextBuffer, tokens);
                    AddTokensFromBufferWithSpecifiedTags(textBuffer, tokens, TokenType.BoldStart, TokenType.BoldEnd);
                    SkipBoldMarker(ref currentPosition);
                    return;
                }

                if (currentChar == '_')
                {
                    ProcessItalicsMarker(line, ref currentPosition, innerTokens, textBuffer, true);
                    continue;
                }

                if (currentChar == ' ')
                {
                    if (isInWord)
                    {
                        AppendMarkerToBuffer(originalTextBuffer, true);
                        originalTextBuffer.Append(textBuffer);
                        return;
                    }

                }

                if (currentChar == '\\')
                {
                    ProcessEscapeCharacter(line, ref currentPosition, textBuffer);
                    continue;
                }

                textBuffer.Append(currentChar);
                currentPosition++;
            }

            PrependMarkerToBuffer(textBuffer, true);
            originalTextBuffer.Append(textBuffer);
        }

        private void AddNestedItalicsTokens(StringBuilder textBuffer, List<Token> tokens, List<Token> innerTokens)
        {
            tokens.Add(new Token(TokenType.BoldStart));
            tokens.AddRange(innerTokens);
            FlushTextBufferIfNotEmpty(textBuffer, tokens);
            tokens.Add(new Token(TokenType.BoldEnd));
        }

        private void AddTokensFromBufferWithSpecifiedTags(StringBuilder textBuffer, List<Token> tokens, TokenType startToken, TokenType endToken)
        {
            tokens.Add(new Token(startToken));
            FlushTextBufferIfNotEmpty(textBuffer, tokens);
            tokens.Add(new Token(endToken));
        }

        private bool IsEmptyMarkedWord(int currentPosition, int startPosition, bool isBold)
        {
            if (isBold) return currentPosition - startPosition == 2;
            return currentPosition - startPosition == 1;
        }

        private void SkipItalicsMarker(ref int currentPosition)
        {
            currentPosition++;
        }

        private void SkipBoldMarker(ref int currentPosition)
        {
            currentPosition += 2;
        }

        private void PrependMarkerToBuffer(StringBuilder textBuffer, bool isBold)
        {
            if (isBold)
            {
                textBuffer.Insert(0, "__");
            }
            else
            {
                textBuffer.Insert(0, '_');
            }
        }

        private void AppendMarkerToBuffer(StringBuilder textBuffer, bool isBold)
        {
            if (isBold)
            {
                textBuffer.Append("__");
            }
            else
            {
                textBuffer.Append('_');
            }
        }

        //проверяем что открывающий маркер находится внутри слова
        private bool IsMarkerInsideWord(string line, int currentPosition, bool isBold)
        {
            if (currentPosition == 0) return false;

            if (isBold)
            {
                return currentPosition + 2 < line.Length
                    && char.IsLetter(line[currentPosition - 1]) 
                    && char.IsLetter(line[currentPosition + 2]);
            }

            return (currentPosition + 1 < line.Length) 
                   && char.IsLetter(line[currentPosition - 1])
                   && char.IsLetter(line[currentPosition + 1]);
        }

        private bool IsValidClosingMarker(string line, int currentPosition, bool isBold)
        {
            if (isBold)
            {
                return currentPosition + 1 < line.Length
                    && line[currentPosition] == '_' 
                    && line[currentPosition + 1] == '_'
                    && !char.IsWhiteSpace(line[currentPosition - 1]);
            }
            //обработка для курсива
            if (currentPosition + 1 < line.Length)
            {
                return line[currentPosition] == '_'
                       && line[currentPosition + 1] != '_'
                       && line[currentPosition - 1] != '_'
                       && !char.IsWhiteSpace(line[currentPosition - 1]);
            }
            return line[currentPosition] == '_'
                   && !char.IsWhiteSpace(line[currentPosition - 1]);
        }

        private bool IsItalicsMarker(string line, int currentPosition)
        {
            return currentPosition + 1 < line.Length
                && line[currentPosition] == '_' && line[currentPosition + 1] != '_'
                && !IsSpaceAfterMarker(line, currentPosition, false)
                && !IsAmongDigits(line, currentPosition, false);
        }

        private bool IsAmongDigits(string line, int currentPosition, bool isBold)
        {
            //если маркер не в самом начале то посмотрим назад
            if (currentPosition > 0)
            {
                if (isBold)
                {
                    return currentPosition + 2 < line.Length
                        && (char.IsDigit(line[currentPosition - 1]) || char.IsDigit(line[currentPosition + 2]));
                }
                return char.IsDigit(line[currentPosition - 1]) 
                       || char.IsDigit(line[currentPosition + 1]);
            }

            //если маркер в начале строки посмотрим что числа только спереди
            if (isBold)
            {
                return currentPosition + 2 < line.Length
                    && char.IsDigit(line[currentPosition + 2]);
            }
            return char.IsDigit(line[currentPosition + 1]);
        }

        //проверим что текущий и след.символы у нас подчеркивания и непробельный символ после
        private bool IsBoldMarker(string line, int currentPosition)
        {
            return currentPosition + 1 < line.Length
                    && line[currentPosition] == '_' 
                    && line[currentPosition + 1] == '_'
                    && !IsSpaceAfterMarker(line, currentPosition, true)
                    && !IsAmongDigits(line, currentPosition, true);
        }

        private bool IsSpaceAfterMarker(string line, int currentPosition, bool isBold)
        {
            if(isBold) return char.IsWhiteSpace(line[currentPosition + 2]);
            return char.IsWhiteSpace(line[currentPosition + 1]);
        }

        private void FlushTextBufferIfNotEmpty(StringBuilder textBuffer, List<Token> tokens)
        {
            if (textBuffer.Length > 0)
            {
                tokens.Add(new Token(TokenType.Text, textBuffer.ToString()));
                textBuffer.Clear();
            }
        }

        private void ProcessEscapeCharacter(string line, ref int currentPosition, StringBuilder textBuffer)
        {
            var anyTagsAfter = IsEscapeBeforeMarkers(line, currentPosition);
            if (anyTagsAfter)
            {
                //пропустить текущий, добавить новый в буфер, перейти вперед снова
                if (currentPosition + 2 < line.Length)
                {
                    currentPosition++;
                    textBuffer.Append(line[currentPosition]);
                    if (line[currentPosition] == '_' && line[currentPosition + 1] == '_')
                    {
                        currentPosition++;
                        textBuffer.Append(line[currentPosition]);
                    }
                    if (currentPosition + 1 < line.Length)
                    {
                        currentPosition++;
                    }
                }
                else if (currentPosition + 1 < line.Length)
                {
                    currentPosition++;
                    textBuffer.Append(line[currentPosition]);
                    currentPosition++;
                }
            }
            else
            {
                //записать текущий слэш как есть
                if (currentPosition < line.Length)
                {
                    textBuffer.Append(line[currentPosition]);
                    currentPosition++;
                }
            }
        }

        //Находится ли символ экранирования перед маркерами или другим экранированием
        private bool IsEscapeBeforeMarkers(string line, int currentPosition)
        {
            if (currentPosition + 2 < line.Length)
            {
                return (line[currentPosition + 1] == '_' ||
                        line[currentPosition + 1] == '\\' ||
                        (line[currentPosition + 1] == '_' && line[currentPosition + 2] == '_'));
            }
            else if (currentPosition + 1 < line.Length)
            {
                return (line[currentPosition + 1] == '_' ||
                        line[currentPosition + 1] == '\\');
            }
            return false;
        }

        private void ProcessHeader(string line, ref int currentPosition, List<Token> tokens)
        {
            tokens.Add(new Token(TokenType.Header));
            if (line.Length > 2) currentPosition = 2;
        }

        private bool IsHeader(string line)
        {
            return line.StartsWith("# ");
        }

    }
}
