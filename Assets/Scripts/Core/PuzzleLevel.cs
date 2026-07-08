using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LetterGarden.Core
{
    /// <summary>
    /// Represents a playable word puzzle level.
    /// </summary>
    public sealed class PuzzleLevel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PuzzleLevel"/> class.
        /// </summary>
        /// <param name="levelId">The unique identifier for the level.</param>
        /// <param name="availableLetters">The letters available to form words in the level.</param>
        /// <param name="validWords">The required words that can be found in the level.</param>
        /// <param name="difficulty">The numeric difficulty rating for the level.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="levelId"/> is empty, <paramref name="availableLetters"/> is empty,
        /// <paramref name="availableLetters"/> contains whitespace, <paramref name="validWords"/> is empty,
        /// or <paramref name="difficulty"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="levelId"/>, <paramref name="availableLetters"/>, or
        /// <paramref name="validWords"/> is null.
        /// </exception>
        public PuzzleLevel(
            string levelId,
            IEnumerable<char> availableLetters,
            IEnumerable<string> validWords,
            int difficulty)
            : this(levelId, availableLetters, validWords, Array.Empty<string>(), difficulty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PuzzleLevel"/> class.
        /// </summary>
        /// <param name="levelId">The unique identifier for the level.</param>
        /// <param name="availableLetters">The letters available to form words in the level.</param>
        /// <param name="requiredWords">The words required to complete the level.</param>
        /// <param name="bonusWords">The optional accepted words that are not required to complete the level.</param>
        /// <param name="difficulty">The numeric difficulty rating for the level.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="levelId"/> is empty, <paramref name="availableLetters"/> is empty,
        /// <paramref name="availableLetters"/> contains whitespace, <paramref name="requiredWords"/> is empty,
        /// <paramref name="requiredWords"/> contains an empty entry, <paramref name="bonusWords"/> contains an empty
        /// entry, or <paramref name="difficulty"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="levelId"/>, <paramref name="availableLetters"/>,
        /// <paramref name="requiredWords"/>, or <paramref name="bonusWords"/> is null.
        /// </exception>
        public PuzzleLevel(
            string levelId,
            IEnumerable<char> availableLetters,
            IEnumerable<string> requiredWords,
            IEnumerable<string> bonusWords,
            int difficulty)
        {
            if (levelId == null)
            {
                throw new ArgumentNullException(nameof(levelId));
            }

            if (string.IsNullOrWhiteSpace(levelId))
            {
                throw new ArgumentException("Level ID cannot be empty.", nameof(levelId));
            }

            if (availableLetters == null)
            {
                throw new ArgumentNullException(nameof(availableLetters));
            }

            if (requiredWords == null)
            {
                throw new ArgumentNullException(nameof(requiredWords));
            }

            if (bonusWords == null)
            {
                throw new ArgumentNullException(nameof(bonusWords));
            }

            if (difficulty < 0)
            {
                throw new ArgumentException("Difficulty cannot be negative.", nameof(difficulty));
            }

            List<char> normalizedLetters = availableLetters.ToList();

            if (normalizedLetters.Count == 0)
            {
                throw new ArgumentException("Available letters cannot be empty.", nameof(availableLetters));
            }

            if (normalizedLetters.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException("Available letters cannot contain whitespace.", nameof(availableLetters));
            }

            ReadOnlyCollection<char> letters = normalizedLetters
                .Select(char.ToUpperInvariant)
                .ToList()
                .AsReadOnly();

            List<string> normalizedRequiredWords = NormalizeWords(requiredWords, nameof(requiredWords));

            if (normalizedRequiredWords.Count == 0)
            {
                throw new ArgumentException("Required words cannot be empty.", nameof(requiredWords));
            }

            List<string> normalizedBonusWords = NormalizeWords(bonusWords, nameof(bonusWords));
            List<string> requiredOnlyBonusWords = normalizedBonusWords
                .Where(word => !normalizedRequiredWords.Contains(word, StringComparer.OrdinalIgnoreCase))
                .ToList();
            List<string> allAcceptedWords = normalizedRequiredWords
                .Concat(requiredOnlyBonusWords)
                .ToList();

            LevelId = levelId.Trim();
            AvailableLetters = letters;
            RequiredWords = normalizedRequiredWords.AsReadOnly();
            BonusWords = requiredOnlyBonusWords.AsReadOnly();
            AllAcceptedWords = allAcceptedWords.AsReadOnly();
            Difficulty = difficulty;
        }

        private static List<string> NormalizeWords(IEnumerable<string> words, string parameterName)
        {
            List<string> normalizedWords = new List<string>();
            HashSet<string> seenWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    throw new ArgumentException("Words cannot contain empty entries.", parameterName);
                }

                string normalizedWord = word.Trim().ToUpperInvariant();

                if (seenWords.Add(normalizedWord))
                {
                    normalizedWords.Add(normalizedWord);
                }
            }

            return normalizedWords;
        }

        /// <summary>
        /// Gets the unique identifier for the level.
        /// </summary>
        public string LevelId { get; }

        /// <summary>
        /// Gets the letters available to form words in the level.
        /// </summary>
        public IReadOnlyList<char> AvailableLetters { get; }

        /// <summary>
        /// Gets the words required to complete the level.
        /// </summary>
        public IReadOnlyCollection<string> RequiredWords { get; }

        /// <summary>
        /// Gets the optional accepted words that are not required to complete the level.
        /// </summary>
        public IReadOnlyCollection<string> BonusWords { get; }

        /// <summary>
        /// Gets all words accepted by the level.
        /// </summary>
        public IReadOnlyCollection<string> AllAcceptedWords { get; }

        /// <summary>
        /// Gets all words accepted by the level.
        /// </summary>
        public IReadOnlyCollection<string> ValidWords => AllAcceptedWords;

        /// <summary>
        /// Gets the numeric difficulty rating for the level.
        /// </summary>
        public int Difficulty { get; }
    }
}
