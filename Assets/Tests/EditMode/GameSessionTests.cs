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
        public void SubmitCurrentWord_ReturnsRequiredWordFound_ForNewRequiredWord()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT" },
                new[] { "AT" },
                1);
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);

            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.RequiredWordFound, result);
            Assert.AreEqual(string.Empty, session.CurrentWord);
            CollectionAssert.AreEqual(new[] { "CAT" }, session.FoundRequiredWords);
        }

        [Test]
        public void SubmitCurrentWord_ReturnsBonusWordFound_ForNewBonusWord()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT" },
                new[] { "AT" },
                1);
            GameSession session = new GameSession(level);

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);

            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.BonusWordFound, result);
            CollectionAssert.AreEqual(new[] { "AT" }, session.FoundBonusWords);
        }

        [Test]
        public void SubmitCurrentWord_ReturnsInvalid_ForInvalidWord()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT" },
                new[] { "AT" },
                1);
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(2);

            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.Invalid, result);
            CollectionAssert.IsEmpty(session.FoundRequiredWords);
            CollectionAssert.IsEmpty(session.FoundBonusWords);
        }

        [Test]
        public void SubmitCurrentWord_ReturnsAlreadyFoundRequired_ForDuplicateRequiredWord()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT" },
                new[] { "AT" },
                1);
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.SubmitCurrentWord();

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.AlreadyFoundRequired, result);
        }

        [Test]
        public void SubmitCurrentWord_ReturnsAlreadyFoundBonus_ForDuplicateBonusWord()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT" },
                new[] { "AT" },
                1);
            GameSession session = new GameSession(level);

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.SubmitCurrentWord();

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.AlreadyFoundBonus, result);
        }

        [Test]
        public void IsRequiredComplete_IsTrue_WhenAllRequiredWordsAreFound()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT" },
                new[] { "AT" },
                1);
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.SubmitCurrentWord();

            Assert.IsTrue(session.IsRequiredComplete);
            Assert.IsTrue(session.CanAdvanceToNextLevel);
            Assert.IsTrue(session.IsComplete);
            Assert.IsFalse(session.AreAllBonusWordsFound);
            Assert.IsFalse(session.IsFullyComplete);
        }

        [Test]
        public void IsFullyComplete_IsTrue_WhenRequiredAndBonusWordsAreFound()
        {
            PuzzleLevel level = new PuzzleLevel(
                "level-1",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT" },
                new[] { "AT" },
                1);
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.SubmitCurrentWord();

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.SubmitCurrentWord();

            Assert.IsTrue(session.IsRequiredComplete);
            Assert.IsTrue(session.AreAllBonusWordsFound);
            Assert.IsTrue(session.IsFullyComplete);
            CollectionAssert.AreEquivalent(new[] { "CAT", "AT" }, session.FoundWords);
        }
    }
}
