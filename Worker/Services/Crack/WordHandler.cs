using System.Text;
using Worker.Services.Crack.Models;

namespace Worker.Services.Crack
{
    public interface IWordHandler
    {
        public void InitializeHandler(CrackRequest request);

        public IEnumerable<string> GetLeftWords();
    }

    public class WordHandler : IWordHandler
    {
        private List<string> _alphabet;
        private int _leftWordsCount;
        private List<int> _symbolsIndexes;

        private const int EmptySymbolIndex = -1;

        private readonly ILogger<WordHandler> _logger;

        public WordHandler(ILogger<WordHandler> logger) {
            _logger = logger;
        }

        public void InitializeHandler(CrackRequest request) {
            _alphabet = request.Alphabet;
            int alphabetLength = _alphabet.Count;

            var allWordsCount = (int)Math.Pow(alphabetLength, request.MaxLength);
            var startIndex = allWordsCount * request.PartNumber / request.PartCount;
            var endIndex = allWordsCount * (request.PartNumber + 1) / request.PartCount;
            _leftWordsCount = endIndex - startIndex;

            _symbolsIndexes = new List<int>(Enumerable.Repeat(EmptySymbolIndex, request.MaxLength));

            _logger.LogInformation("allWordsCount " + allWordsCount + "\n" +
                "startIndex " + startIndex + "\n" +
                "endIndex " + endIndex + "\n" +
                "leftWordsCount " + _leftWordsCount);

            var symbolIndex = 0;
            while(startIndex > 0) {
                _symbolsIndexes[symbolIndex] =  startIndex % alphabetLength;
                startIndex /= alphabetLength;
                symbolIndex++;
            }
        }

        public IEnumerable<string> GetLeftWords() {
            for (; _leftWordsCount > 0; _leftWordsCount--) {
                yield return GetNextWord();
            }
        }

        private string GetNextWord() {
            var result = new StringBuilder();
            for (int i = 0; i < _symbolsIndexes.Count; i++) {
                if (_symbolsIndexes[i] == EmptySymbolIndex) {
                    break;
                }

                result.Append(_alphabet[_symbolsIndexes[i]]);
            }

            if (_leftWordsCount > 1) {
                NextWord();
            }

            return result.ToString();
        }

        private void NextWord() {
            int i = 0;
            for (; _symbolsIndexes[i] + 1 == _alphabet.Count && i < _symbolsIndexes.Count; ++i) {
                _symbolsIndexes[i] = 0;
            }

            if (i < _symbolsIndexes.Count) {
                _symbolsIndexes[i]++;
            }
        }
    }
}
