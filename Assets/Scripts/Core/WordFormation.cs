using System.Collections.Generic;
using System.Linq;

namespace LetterGarden.Core
{
    public static class WordFormation
    {
        public static bool CanFormWord(string word, IEnumerable<char> availableLetters)
        {
            if (string.IsNullOrWhiteSpace(word) || availableLetters == null)
            {
                return false;
            }

            List<char> letters = availableLetters.ToList();
            if (letters.Count == 0)
            {
                return false;
            }

            Dictionary<char, int> availableLetterCounts = new Dictionary<char, int>();

            foreach (char letter in letters)
            {
                char normalizedLetter = char.ToUpperInvariant(letter);

                if (!availableLetterCounts.ContainsKey(normalizedLetter))
                {
                    availableLetterCounts[normalizedLetter] = 0;
                }

                availableLetterCounts[normalizedLetter]++;
            }

            foreach (char letter in word.Trim().ToUpperInvariant())
            {
                if (!availableLetterCounts.ContainsKey(letter) || availableLetterCounts[letter] == 0)
                {
                    return false;
                }

                availableLetterCounts[letter]--;
            }

            return true;
        }
    }
}
