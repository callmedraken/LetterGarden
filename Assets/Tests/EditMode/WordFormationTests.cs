using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class WordFormationTests
    {
        [Test]
        public void CanFormWord_ReturnsTrue_WhenAteCanBeFormedFromRate()
        {
            Assert.IsTrue(WordFormation.CanFormWord("ATE", new[] { 'R', 'A', 'T', 'E' }));
        }

        [Test]
        public void CanFormWord_ReturnsFalse_WhenEaterUsesTooManyEsFromRate()
        {
            Assert.IsFalse(WordFormation.CanFormWord("EATER", new[] { 'R', 'A', 'T', 'E' }));
        }

        [Test]
        public void CanFormWord_ReturnsTrue_WhenCatCanBeFormedFromAct()
        {
            Assert.IsTrue(WordFormation.CanFormWord("CAT", new[] { 'A', 'C', 'T' }));
        }

        [Test]
        public void CanFormWord_ReturnsFalse_WhenDogCannotBeFormedFromCat()
        {
            Assert.IsFalse(WordFormation.CanFormWord("DOG", new[] { 'C', 'A', 'T' }));
        }

        [Test]
        public void CanFormWord_ReturnsFalse_ForNullEmptyOrWhitespaceWord()
        {
            Assert.IsFalse(WordFormation.CanFormWord(null, new[] { 'C', 'A', 'T' }));
            Assert.IsFalse(WordFormation.CanFormWord(string.Empty, new[] { 'C', 'A', 'T' }));
            Assert.IsFalse(WordFormation.CanFormWord(" ", new[] { 'C', 'A', 'T' }));
        }

        [Test]
        public void CanFormWord_ReturnsFalse_ForNullOrEmptyAvailableLetters()
        {
            Assert.IsFalse(WordFormation.CanFormWord("CAT", null));
            Assert.IsFalse(WordFormation.CanFormWord("CAT", new char[0]));
        }
    }
}
