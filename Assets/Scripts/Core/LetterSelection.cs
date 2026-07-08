using System;
using System.Collections.Generic;
using System.Linq;

namespace LetterGarden.Core
{
    /// <summary>
    /// Tracks the letter indices selected while assembling a word.
    /// </summary>
    public sealed class LetterSelection
    {
        private readonly IReadOnlyList<char> availableLetters;
        private readonly List<int> selectedIndices;

        /// <summary>
        /// Initializes a new instance of the <see cref="LetterSelection"/> class.
        /// </summary>
        /// <param name="availableLetters">The letters available for selection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="availableLetters"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="availableLetters"/> is empty or contains whitespace.
        /// </exception>
        public LetterSelection(IReadOnlyList<char> availableLetters)
        {
            if (availableLetters == null)
            {
                throw new ArgumentNullException(nameof(availableLetters));
            }

            if (availableLetters.Count == 0)
            {
                throw new ArgumentException("Available letters cannot be empty.", nameof(availableLetters));
            }

            if (availableLetters.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException("Available letters cannot contain whitespace.", nameof(availableLetters));
            }

            this.availableLetters = availableLetters
                .Select(char.ToUpperInvariant)
                .ToList()
                .AsReadOnly();
            selectedIndices = new List<int>();
        }

        /// <summary>
        /// Gets the word currently represented by the selected letter indices.
        /// </summary>
        public string CurrentWord => new string(selectedIndices.Select(index => availableLetters[index]).ToArray());

        /// <summary>
        /// Gets the selected letter indices in selection order.
        /// </summary>
        public IReadOnlyList<int> SelectedIndices => selectedIndices.ToList().AsReadOnly();

        /// <summary>
        /// Attempts to select the letter at the specified index.
        /// </summary>
        /// <param name="index">The index of the letter to select.</param>
        /// <returns>True if the index was selected; otherwise, false.</returns>
        public bool TrySelectLetter(int index)
        {
            if (index < 0 || index >= availableLetters.Count || IsSelected(index))
            {
                return false;
            }

            selectedIndices.Add(index);
            return true;
        }

        /// <summary>
        /// Determines whether the specified letter index has already been selected.
        /// </summary>
        /// <param name="index">The letter index to check.</param>
        /// <returns>True if the index has been selected; otherwise, false.</returns>
        public bool IsSelected(int index)
        {
            return selectedIndices.Contains(index);
        }

        /// <summary>
        /// Clears all selected letter indices.
        /// </summary>
        public void Clear()
        {
            selectedIndices.Clear();
        }

        /// <summary>
        /// Removes the most recently selected letter index.
        /// </summary>
        /// <returns>True if a selected index was removed; otherwise, false.</returns>
        public bool Backspace()
        {
            if (selectedIndices.Count == 0)
            {
                return false;
            }

            selectedIndices.RemoveAt(selectedIndices.Count - 1);
            return true;
        }
    }
}
