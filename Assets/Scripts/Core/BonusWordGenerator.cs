using System;
using System.Collections.Generic;
using System.Linq;

namespace LetterGarden.Core
{
    /// <summary>
    /// Generates level-specific bonus words from a shared word list.
    /// </summary>
    public static class BonusWordGenerator
    {
        /// <summary>
        /// Returns dictionary words that can be formed from the level letters and are not required words.
        /// </summary>
        /// <param name="dictionaryWords">The shared accepted word list.</param>
        /// <param name="availableLetters">The letters available in the level.</param>
        /// <param name="requiredWords">Words that should be treated as required, not bonus.</param>
        /// <param name="minimumWordLength">The minimum allowed dictionary word length.</param>
        /// <returns>Normalized bonus words for the level.</returns>
        public static IReadOnlyCollection<string> GenerateBonusWords(
            IEnumerable<string> dictionaryWords,
            IEnumerable<char> availableLetters,
            IEnumerable<string> requiredWords,
            int minimumWordLength = WordDictionary.DefaultMinimumWordLength)
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
            WordDictionary dictionary = WordDictionary.FromWords(dictionaryWords, minimumWordLength);
            HashSet<string> requiredWordSet = new HashSet<string>(
                requiredWords
                    .Where(word => !string.IsNullOrWhiteSpace(word))
                    .Select(word => word.Trim().ToUpperInvariant()),
                StringComparer.OrdinalIgnoreCase);
            HashSet<string> bonusWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int maximumWordLength = normalizedLetters.Count;

            foreach (string normalizedWord in dictionary.Words)
            {
                if (normalizedWord.Length > maximumWordLength
                    || requiredWordSet.Contains(normalizedWord)
                    || !WordFormation.CanFormWord(normalizedWord, normalizedLetters))
                {
                    continue;
                }

                bonusWords.Add(normalizedWord);
            }

            return bonusWords.ToList().AsReadOnly();
        }
    }
}
