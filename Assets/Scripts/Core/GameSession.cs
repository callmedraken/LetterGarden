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
        private readonly LetterSelection letterSelection;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSession"/> class.
        /// </summary>
        /// <param name="level">The puzzle level to play.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="level"/> is null.</exception>
        public GameSession(PuzzleLevel level)
        {
            CurrentLevel = level ?? throw new ArgumentNullException(nameof(level));
            wordValidator = new WordValidator(level.ValidWords);
            letterSelection = new LetterSelection(level.AvailableLetters);
        }

        /// <summary>
        /// Gets the puzzle level currently being played.
        /// </summary>
        public PuzzleLevel CurrentLevel { get; }

        /// <summary>
        /// Gets the word currently being assembled by the player.
        /// </summary>
        public string CurrentWord => letterSelection.CurrentWord;

        /// <summary>
        /// Gets the selected letter indices in selection order.
        /// </summary>
        public IReadOnlyList<int> SelectedLetterIndices => letterSelection.SelectedIndices;

        /// <summary>
        /// Gets the words that have already been found.
        /// </summary>
        public IReadOnlyCollection<string> FoundWords => wordValidator.GetFoundWords();

        /// <summary>
        /// Gets a value indicating whether every valid word in the level has been found.
        /// </summary>
        public bool IsComplete => FoundWords.Count == CurrentLevel.ValidWords.Count;

        /// <summary>
        /// Attempts to select the letter at the specified index.
        /// </summary>
        /// <param name="index">The index of the letter to select.</param>
        /// <returns>True if the index was selected; otherwise, false.</returns>
        public bool TrySelectLetter(int index)
        {
            return letterSelection.TrySelectLetter(index);
        }

        /// <summary>
        /// Determines whether the specified letter index has already been selected.
        /// </summary>
        /// <param name="index">The letter index to check.</param>
        /// <returns>True if the index has been selected; otherwise, false.</returns>
        public bool IsLetterSelected(int index)
        {
            return letterSelection.IsSelected(index);
        }

        /// <summary>
        /// Removes the most recently selected letter index.
        /// </summary>
        /// <returns>True if a selected index was removed; otherwise, false.</returns>
        public bool BackspaceLetter()
        {
            return letterSelection.Backspace();
        }

        /// <summary>
        /// Clears the current word.
        /// </summary>
        public void ClearCurrentWord()
        {
            letterSelection.Clear();
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
