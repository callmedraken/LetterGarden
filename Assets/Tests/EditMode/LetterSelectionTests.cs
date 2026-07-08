using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class LetterSelectionTests
    {
        [Test]
        public void TrySelectLetter_ReturnsFalse_WhenIndexIsAlreadySelected()
        {
            LetterSelection selection = new LetterSelection(new[] { 'C', 'A', 'T' });

            bool firstSelection = selection.TrySelectLetter(0);
            bool secondSelection = selection.TrySelectLetter(0);

            Assert.IsTrue(firstSelection);
            Assert.IsFalse(secondSelection);
            CollectionAssert.AreEqual(new[] { 0 }, selection.SelectedIndices);
        }

        [Test]
        public void TrySelectLetter_ReturnsFalse_WhenIndexIsOutOfRange()
        {
            LetterSelection selection = new LetterSelection(new[] { 'C', 'A', 'T' });

            Assert.IsFalse(selection.TrySelectLetter(-1));
            Assert.IsFalse(selection.TrySelectLetter(3));
        }

        [Test]
        public void Backspace_RemovesMostRecentSelectedLetter()
        {
            LetterSelection selection = new LetterSelection(new[] { 'C', 'A', 'T' });
            selection.TrySelectLetter(0);
            selection.TrySelectLetter(1);

            bool removed = selection.Backspace();

            Assert.IsTrue(removed);
            Assert.AreEqual("C", selection.CurrentWord);
            CollectionAssert.AreEqual(new[] { 0 }, selection.SelectedIndices);
        }

        [Test]
        public void Backspace_ReturnsFalse_WhenNothingIsSelected()
        {
            LetterSelection selection = new LetterSelection(new[] { 'C', 'A', 'T' });

            bool removed = selection.Backspace();

            Assert.IsFalse(removed);
        }

        [Test]
        public void Clear_RemovesAllSelectedLetters()
        {
            LetterSelection selection = new LetterSelection(new[] { 'C', 'A', 'T' });
            selection.TrySelectLetter(0);
            selection.TrySelectLetter(1);

            selection.Clear();

            Assert.AreEqual(string.Empty, selection.CurrentWord);
            CollectionAssert.IsEmpty(selection.SelectedIndices);
        }
    }
}
