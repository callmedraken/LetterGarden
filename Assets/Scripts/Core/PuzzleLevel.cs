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
        /// <param name="validWords">The valid words that can be found in the level.</param>
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

            if (validWords == null)
            {
                throw new ArgumentNullException(nameof(validWords));
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

            List<string> normalizedWords = validWords
                .Select(word => word?.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedWords.Count == 0)
            {
                throw new ArgumentException("Valid words cannot be empty.", nameof(validWords));
            }

            if (normalizedWords.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Valid words cannot contain empty entries.", nameof(validWords));
            }

            ReadOnlyCollection<string> words = normalizedWords.AsReadOnly();

            LevelId = levelId.Trim();
            AvailableLetters = letters;
            ValidWords = words;
            Difficulty = difficulty;
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
        /// Gets the valid words that can be found in the level.
        /// </summary>
        public IReadOnlyCollection<string> ValidWords { get; }

        /// <summary>
        /// Gets the numeric difficulty rating for the level.
        /// </summary>
        public int Difficulty { get; }
    }
}
