using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class WordFormationTests
    {
        [Test]
        public void CanFormWord_ReturnsTrue_WhenWordUsesAvailableLetters()
        {
            Assert.IsTrue(WordFormation.CanFormWord("ATE", "RATE".ToCharArray()));
            Assert.IsTrue(WordFormation.CanFormWord("CAT", "ACT".ToCharArray()));
        }

        [Test]
        public void CanFormWord_ReturnsFalse_WhenWordNeedsUnavailableLetters()
        {
            Assert.IsFalse(WordFormation.CanFormWord("DOG", "CAT".ToCharArray()));
        }

        [Test]
        public void CanFormWord_ReturnsFalse_WhenWordUsesALetterTooManyTimes()
        {
            Assert.IsFalse(WordFormation.CanFormWord("EATER", "RATE".ToCharArray()));
        }
    }
}
