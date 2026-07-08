using System;
using System.Collections.Generic;

namespace LetterGarden.Core
{
    /// <summary>
    /// Tracks progress for a single active puzzle level.
    /// </summary>
    public sealed class GameSession
    {
        private readonly WordValidator wordValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSession"/> class.
        /// </summary>
        /// <param name="level">The puzzle level to play.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="level"/> is null.</exception>
        public GameSession(PuzzleLevel level)
        {
            CurrentLevel = level ?? throw new ArgumentNullException(nameof(level));
            wordValidator = new WordValidator(level.ValidWords);
            CurrentWord = string.Empty;
        }

        /// <summary>
        /// Gets the puzzle level currently being played.
        /// </summary>
        public PuzzleLevel CurrentLevel { get; }

        /// <summary>
        /// Gets the word currently being assembled by the player.
        /// </summary>
        public string CurrentWord { get; private set; }

        /// <summary>
        /// Gets the words that have already been found.
        /// </summary>
        public IReadOnlyCollection<string> FoundWords => wordValidator.GetFoundWords();

        /// <summary>
        /// Gets a value indicating whether every valid word in the level has been found.
        /// </summary>
        public bool IsComplete => FoundWords.Count == CurrentLevel.ValidWords.Count;

        /// <summary>
        /// Adds a letter to the current word.
        /// </summary>
        /// <param name="letter">The letter to add.</param>
        public void AddLetter(char letter)
        {
            CurrentWord += char.ToUpperInvariant(letter);
        }

        /// <summary>
        /// Clears the current word.
        /// </summary>
        public void ClearCurrentWord()
        {
            CurrentWord = string.Empty;
        }

        /// <summary>
        /// Submits the current word for validation.
        /// </summary>
        /// <returns>True if the current word was valid and newly found; otherwise, false.</returns>
        public bool SubmitCurrentWord()
        {
            bool wasNewlyFound = wordValidator.TryFindWord(CurrentWord);
            ClearCurrentWord();
            return wasNewlyFound;
        }
    }
}
