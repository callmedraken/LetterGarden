using System.Linq;
using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class WordValidatorTests
    {
        [Test]
        public void IsValidWord_ReturnsTrue_ForValidWord()
        {
            WordValidator validator = new WordValidator(new[] { "CAT", "DOG" });

            bool isValid = validator.IsValidWord("cat");

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidWord_ReturnsFalse_ForInvalidWord()
        {
            WordValidator validator = new WordValidator(new[] { "CAT", "DOG" });

            bool isValid = validator.IsValidWord("BIRD");

            Assert.IsFalse(isValid);
        }

        [Test]
        public void TryFindWord_ReturnsFalse_WhenWordWasAlreadyFound()
        {
            WordValidator validator = new WordValidator(new[] { "CAT" });

            bool firstAttempt = validator.TryFindWord("CAT");
            bool secondAttempt = validator.TryFindWord("cat");

            Assert.IsTrue(firstAttempt);
            Assert.IsFalse(secondAttempt);
            Assert.AreEqual(1, validator.GetFoundWords().Count);
        }
    }
}
