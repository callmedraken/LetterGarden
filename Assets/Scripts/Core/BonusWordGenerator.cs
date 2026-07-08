using System;
using System.Collections.Generic;
using System.Linq;

namespace LetterGarden.Core
{
    public static class BonusWordGenerator
    {
        public static IReadOnlyCollection<string> GenerateBonusWords(
            IEnumerable<string> dictionaryWords,
            IEnumerable<char> availableLetters,
            IEnumerable<string> requiredWords)
        {
            if (dictionaryWords == null)
            {
                throw new ArgumentNullException(nameof(dictionaryWords));
            }

            if (availableLetters == null)
            {
                throw new ArgumentNullException(nameof(availableLetters));
            }

            if (requiredWords == null)
            {
                throw new ArgumentNullException(nameof(requiredWords));
            }

            List<char> normalizedLetters = availableLetters
                .Select(char.ToUpperInvariant)
                .ToList();
            HashSet<string> normalizedRequiredWords = new HashSet<string>(
                requiredWords
                    .Where(word => !string.IsNullOrWhiteSpace(word))
                    .Select(word => word.Trim().ToUpperInvariant()),
                StringComparer.OrdinalIgnoreCase);
            List<string> bonusWords = new List<string>();
            HashSet<string> seenBonusWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string dictionaryWord in dictionaryWords)
            {
                if (string.IsNullOrWhiteSpace(dictionaryWord))
                {
                    continue;
                }

                string word = dictionaryWord.Trim().ToUpperInvariant();

                if (word.Length < 2 || normalizedRequiredWords.Contains(word))
                {
                    continue;
                }

                if (CanFormWord(word, normalizedLetters) && seenBonusWords.Add(word))
                {
                    bonusWords.Add(word);
                }
            }

            return bonusWords.AsReadOnly();
        }

        public static bool CanFormWord(string word, IEnumerable<char> availableLetters)
        {
            if (string.IsNullOrWhiteSpace(word) || availableLetters == null)
            {
                return false;
            }

            Dictionary<char, int> availableLetterCounts = new Dictionary<char, int>();

            foreach (char letter in availableLetters)
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
