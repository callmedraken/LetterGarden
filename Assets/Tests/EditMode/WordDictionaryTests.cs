using System.Linq;
using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class WordDictionaryTests
    {
        [Test]
        public void FromWords_NormalizesAndDeduplicatesWords()
        {
            WordDictionary dictionary = WordDictionary.FromWords(new[] { " ate ", "ATE", "tea" });

            CollectionAssert.AreEquivalent(new[] { "ATE", "TEA" }, dictionary.Words.ToArray());
            Assert.IsTrue(dictionary.Contains("ate"));
        }

        [Test]
        public void FromWords_IgnoresEmptyShortAndNonLetterEntries()
        {
            WordDictionary dictionary = WordDictionary.FromWords(new[] { "", "  ", "AT", "A-TE", "E4T", "RATE" });

            CollectionAssert.AreEquivalent(new[] { "RATE" }, dictionary.Words.ToArray());
        }
    }
}
