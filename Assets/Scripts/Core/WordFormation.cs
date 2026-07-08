using System.Collections.Generic;
using System.Linq;

namespace LetterGarden.Core
{
    /// <summary>
    /// Provides helpers for checking whether words can be built from a set of letters.
    /// </summary>
    public static class WordFormation
    {
        /// <summary>
        /// Determines whether a word can be formed without using any letter more times than it is available.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <param name="availableLetters">The letters available to build the word.</param>
        /// <returns>True if the word can be formed; otherwise, false.</returns>
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

            Dictionary<char, int> letterCounts = new Dictionary<char, int>();

            foreach (char letter in letters)
            {
                char normalizedLetter = char.ToUpperInvariant(letter);

                if (!letterCounts.ContainsKey(normalizedLetter))
                {
                    letterCounts[normalizedLetter] = 0;
                }

                letterCounts[normalizedLetter]++;
            }

            foreach (char letter in word.Trim().ToUpperInvariant())
            {
                if (!letterCounts.ContainsKey(letter) || letterCounts[letter] == 0)
                {
                    return false;
                }

                letterCounts[letter]--;
            }

            return true;
        }
    }
}
