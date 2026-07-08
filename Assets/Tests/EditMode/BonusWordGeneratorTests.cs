using System.Linq;
using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class BonusWordGeneratorTests
    {
        [Test]
        public void CanFormWord_ReturnsTrue_WhenAteCanBeFormedFromRate()
        {
            bool canFormWord = BonusWordGenerator.CanFormWord("ATE", new[] { 'R', 'A', 'T', 'E' });

            Assert.IsTrue(canFormWord);
        }

        [Test]
        public void CanFormWord_ReturnsFalse_WhenWordUsesLetterMoreTimesThanAvailable()
        {
            bool canFormWord = BonusWordGenerator.CanFormWord("TART", new[] { 'R', 'A', 'T', 'E' });

            Assert.IsFalse(canFormWord);
        }

        [Test]
        public void GenerateBonusWords_DoesNotIncludeRequiredWords()
        {
            string[] dictionaryWords = { "CAT", "ACT", "AT" };
            string[] requiredWords = { "CAT", "ACT" };

            string[] bonusWords = BonusWordGenerator
                .GenerateBonusWords(dictionaryWords, new[] { 'C', 'A', 'T' }, requiredWords)
                .ToArray();

            CollectionAssert.AreEqual(new[] { "AT" }, bonusWords);
        }
    }
}
