using System;
using System.Collections.Generic;
using System.Linq;

namespace LetterGarden.Core
{
    public class WordValidator
    {
        private readonly HashSet<string> validWords;
        private readonly HashSet<string> foundWords;

        public WordValidator(IEnumerable<string> validWords)
        {
            if (validWords == null)
            {
                throw new ArgumentNullException(nameof(validWords));
            }

            this.validWords = new HashSet<string>(validWords, StringComparer.OrdinalIgnoreCase);
            foundWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool IsValidWord(string word)
        {
            return !string.IsNullOrWhiteSpace(word) && validWords.Contains(word);
        }

        public bool HasWordBeenFound(string word)
        {
            return !string.IsNullOrWhiteSpace(word) && foundWords.Contains(word);
        }

        public bool TryFindWord(string word)
        {
            if (!IsValidWord(word) || HasWordBeenFound(word))
            {
                return false;
            }

            foundWords.Add(word);
            return true;
        }

        public IReadOnlyCollection<string> GetFoundWords()
        {
            return foundWords.ToList().AsReadOnly();
        }
    }
}
