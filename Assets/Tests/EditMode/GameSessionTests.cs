using System.Linq;
using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class GameSessionTests
    {
        [Test]
        public void TrySelectLetter_ReturnsFalse_WhenSameIndexIsSelectedTwice()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT" },
                1);
            GameSession session = new GameSession(level);

            bool firstSelection = session.TrySelectLetter(0);
            bool secondSelection = session.TrySelectLetter(0);

            Assert.IsTrue(firstSelection);
            Assert.IsFalse(secondSelection);
            Assert.IsTrue(session.IsLetterSelected(0));
            CollectionAssert.AreEqual(new[] { 0 }, session.SelectedLetterIndices);
            Assert.AreEqual("C", session.CurrentWord);
        }

        [Test]
        public void SubmitCurrentWord_FindsWordsAndTracksCompletion()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT", "AT" },
                1);
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);

            bool foundFirstWord = session.SubmitCurrentWord();

            Assert.IsTrue(foundFirstWord);
            Assert.AreEqual(string.Empty, session.CurrentWord);
            CollectionAssert.Contains(session.FoundWords.ToArray(), "CAT");
            Assert.IsFalse(session.IsComplete);

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);

            bool foundSecondWord = session.SubmitCurrentWord();

            Assert.IsTrue(foundSecondWord);
            CollectionAssert.AreEquivalent(new[] { "CAT", "AT" }, session.FoundWords);
            Assert.IsTrue(session.IsComplete);
        }
    }
}
