using System;
using System.Collections.Generic;
using System.Linq;

namespace LetterGarden.Core
{
    /// <summary>
    /// Tracks progress for a single active puzzle level.
    /// </summary>
    public sealed class GameSession
    {
        private readonly LetterSelection letterSelection;
        private readonly HashSet<string> foundRequiredWords;
        private readonly HashSet<string> foundBonusWords;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSession"/> class.
        /// </summary>
        /// <param name="level">The puzzle level to play.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="level"/> is null.</exception>
        public GameSession(PuzzleLevel level)
        {
            CurrentLevel = level ?? throw new ArgumentNullException(nameof(level));
            letterSelection = new LetterSelection(level.AvailableLetters);
            foundRequiredWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foundBonusWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
        /// Gets the required words that have already been found.
        /// </summary>
        public IReadOnlyCollection<string> FoundRequiredWords => foundRequiredWords.ToList().AsReadOnly();

        /// <summary>
        /// Gets the bonus words that have already been found.
        /// </summary>
        public IReadOnlyCollection<string> FoundBonusWords => foundBonusWords.ToList().AsReadOnly();

        /// <summary>
        /// Gets all words that have already been found.
        /// </summary>
        public IReadOnlyCollection<string> FoundWords =>
            foundRequiredWords.Concat(foundBonusWords).ToList().AsReadOnly();

        /// <summary>
        /// Gets a value indicating whether every required word in the level has been found.
        /// </summary>
        public bool IsRequiredComplete => foundRequiredWords.Count == CurrentLevel.RequiredWords.Count;

        /// <summary>
        /// Gets a value indicating whether every bonus word in the level has been found.
        /// </summary>
        public bool AreAllBonusWordsFound => foundBonusWords.Count == CurrentLevel.BonusWords.Count;

        /// <summary>
        /// Gets a value indicating whether the player can advance to the next level.
        /// </summary>
        public bool CanAdvanceToNextLevel => IsRequiredComplete;

        /// <summary>
        /// Gets a value indicating whether every required and bonus word has been found.
        /// </summary>
        public bool IsFullyComplete => IsRequiredComplete && AreAllBonusWordsFound;

        /// <summary>
        /// Gets a value indicating whether every required word in the level has been found.
        /// </summary>
        public bool IsComplete => IsRequiredComplete;

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
        /// <returns>The result of submitting the current word.</returns>
        public WordSubmitResult SubmitCurrentWord()
        {
            WordSubmitResult result = GetSubmitResult(CurrentWord);
            ClearCurrentWord();
            return result;
        }

        private WordSubmitResult GetSubmitResult(string word)
        {
            if (CurrentLevel.RequiredWords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                if (!foundRequiredWords.Add(word))
                {
                    return WordSubmitResult.AlreadyFoundRequired;
                }

                return WordSubmitResult.RequiredWordFound;
            }

            if (CurrentLevel.BonusWords.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                if (!foundBonusWords.Add(word))
                {
                    return WordSubmitResult.AlreadyFoundBonus;
                }

                return WordSubmitResult.BonusWordFound;
            }

            return WordSubmitResult.Invalid;
        }
    }
}
