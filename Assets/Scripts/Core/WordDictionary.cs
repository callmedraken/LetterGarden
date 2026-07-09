using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LetterGarden.Core
{
    /// <summary>
    /// Represents a normalized set of globally accepted dictionary words.
    /// </summary>
    public sealed class WordDictionary
    {
        /// <summary>
        /// The default minimum length for accepted dictionary words.
        /// </summary>
        public const int DefaultMinimumWordLength = 3;

        private readonly HashSet<string> wordSet;
        private readonly ReadOnlyCollection<string> words;

        private WordDictionary(HashSet<string> normalizedWords)
        {
            wordSet = normalizedWords;
            words = normalizedWords
                .OrderBy(word => word)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets the normalized dictionary words.
        /// </summary>
        public IReadOnlyCollection<string> Words => words;

        /// <summary>
        /// Creates a dictionary from raw word entries.
        /// </summary>
        /// <param name="rawWords">The raw dictionary entries.</param>
        /// <param name="minimumWordLength">The minimum accepted word length.</param>
        /// <returns>A normalized dictionary.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rawWords"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="minimumWordLength"/> is less than 1.
        /// </exception>
        public static WordDictionary FromWords(
            IEnumerable<string> rawWords,
            int minimumWordLength = DefaultMinimumWordLength)
        {
            if (rawWords == null)
            {
                throw new ArgumentNullException(nameof(rawWords));
            }

            if (minimumWordLength < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumWordLength));
            }

            HashSet<string> normalizedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string rawWord in rawWords)
            {
                if (string.IsNullOrWhiteSpace(rawWord))
                {
                    continue;
                }

                string normalizedWord = rawWord.Trim().ToUpperInvariant();

                if (normalizedWord.Length < minimumWordLength || !ContainsOnlyLetters(normalizedWord))
                {
                    continue;
                }

                normalizedWords.Add(normalizedWord);
            }

            return new WordDictionary(normalizedWords);
        }

        /// <summary>
        /// Determines whether the dictionary contains a word.
        /// </summary>
        /// <param name="word">The word to find.</param>
        /// <returns>True if the normalized word exists in the dictionary; otherwise, false.</returns>
        public bool Contains(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            return wordSet.Contains(word.Trim().ToUpperInvariant());
        }

        private static bool ContainsOnlyLetters(string word)
        {
            foreach (char letter in word)
            {
                if (letter < 'A' || letter > 'Z')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
