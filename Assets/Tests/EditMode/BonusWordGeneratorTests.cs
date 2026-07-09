using System.Linq;
using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class BonusWordGeneratorTests
    {
        [Test]
        public void GenerateBonusWords_IncludesFormableDictionaryWords_NotRequiredInCurrentLevel()
        {
            string[] bonusWords = BonusWordGenerator.GenerateBonusWords(
                    new[] { "ATE" },
                    "RATE".ToCharArray(),
                    new[] { "RATE" })
                .ToArray();

            CollectionAssert.Contains(bonusWords, "ATE");
        }

        [Test]
        public void GenerateBonusWords_AcceptsAteAsBonusOnRateLevel()
        {
            PuzzleLevel level = CreateLevelWithGeneratedBonusWords(
                "RATE",
                new[] { "RATE" },
                new[] { "ATE" });
            GameSession session = new GameSession(level);

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.TrySelectLetter(3);

            Assert.AreEqual(WordSubmitResult.BonusWordFound, session.SubmitCurrentWord());
        }

        [Test]
        public void GenerateBonusWords_IgnoresWordsLongerThanAvailableLetters()
        {
            string[] bonusWords = BonusWordGenerator.GenerateBonusWords(
                    new[] { "STREAM" },
                    "RATE".ToCharArray(),
                    new[] { "RATE" })
                .ToArray();

            CollectionAssert.DoesNotContain(bonusWords, "STREAM");
        }

        [Test]
        public void GenerateBonusWords_ExcludesCurrentRequiredWords()
        {
            string[] bonusWords = BonusWordGenerator.GenerateBonusWords(
                    new[] { "RATE", "ATE" },
                    "RATE".ToCharArray(),
                    new[] { "RATE" })
                .ToArray();

            CollectionAssert.DoesNotContain(bonusWords, "RATE");
            CollectionAssert.Contains(bonusWords, "ATE");
        }

        [Test]
        public void GenerateBonusWords_ExcludesDictionaryWordsThatCannotBeFormed()
        {
            string[] bonusWords = BonusWordGenerator.GenerateBonusWords(
                    new[] { "DOG" },
                    "CAT".ToCharArray(),
                    new[] { "CAT" })
                .ToArray();

            CollectionAssert.DoesNotContain(bonusWords, "DOG");
        }

        [Test]
        public void SubmitCurrentWord_ReturnsInvalid_WhenDictionaryWordsCannotBeFormedForLevel()
        {
            PuzzleLevel level = CreateLevelWithGeneratedBonusWords(
                "CAT",
                new[] { "ACT" },
                new[] { "DOG" });
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);

            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.Invalid, result);
            CollectionAssert.IsEmpty(session.FoundBonusWords);
        }

        [Test]
        public void GenerateBonusWords_IsNotLimitedToManualBonusWords()
        {
            PuzzleLevel level = CreateLevelWithGeneratedBonusWords(
                "RATE",
                new[] { "RATE" },
                new[] { "ATE" });
            GameSession session = new GameSession(level);

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.TrySelectLetter(3);

            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.BonusWordFound, result);
            CollectionAssert.Contains(session.FoundBonusWords, "ATE");
        }

        [Test]
        public void SubmitCurrentWord_ReturnsRequiredWordFound_WhenDictionaryWordIsRequiredInCurrentLevel()
        {
            PuzzleLevel level = CreateLevelWithGeneratedBonusWords(
                "RATE",
                new[] { "RATE" },
                new[] { "RATE", "ATE" });
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.TrySelectLetter(3);

            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.RequiredWordFound, result);
            CollectionAssert.Contains(session.FoundRequiredWords, "RATE");
            CollectionAssert.IsEmpty(session.FoundBonusWords);
        }

        [Test]
        public void SubmitCurrentWord_ReturnsAlreadyFoundRequired_WhenRequiredDictionaryWordIsSubmittedTwice()
        {
            PuzzleLevel level = CreateLevelWithGeneratedBonusWords(
                "RATE",
                new[] { "RATE" },
                new[] { "RATE", "ATE" });
            GameSession session = new GameSession(level);

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.TrySelectLetter(3);
            Assert.AreEqual(WordSubmitResult.RequiredWordFound, session.SubmitCurrentWord());

            session.TrySelectLetter(0);
            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.TrySelectLetter(3);
            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.AlreadyFoundRequired, result);
        }

        [Test]
        public void SubmitCurrentWord_ReturnsAlreadyFoundBonus_WhenGeneratedBonusWordIsSubmittedTwice()
        {
            PuzzleLevel level = CreateLevelWithGeneratedBonusWords(
                "RATE",
                new[] { "RATE" },
                new[] { "ATE" });
            GameSession session = new GameSession(level);

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.TrySelectLetter(3);
            Assert.AreEqual(WordSubmitResult.BonusWordFound, session.SubmitCurrentWord());

            session.TrySelectLetter(1);
            session.TrySelectLetter(2);
            session.TrySelectLetter(3);
            WordSubmitResult result = session.SubmitCurrentWord();

            Assert.AreEqual(WordSubmitResult.AlreadyFoundBonus, result);
        }

        private static PuzzleLevel CreateLevelWithGeneratedBonusWords(
            string letters,
            string[] requiredWords,
            string[] dictionaryWords)
        {
            return new PuzzleLevel(
                "test-level",
                letters.ToCharArray(),
                requiredWords,
                BonusWordGenerator.GenerateBonusWords(
                    dictionaryWords,
                    letters.ToCharArray(),
                    requiredWords),
                1);
        }
    }
}
