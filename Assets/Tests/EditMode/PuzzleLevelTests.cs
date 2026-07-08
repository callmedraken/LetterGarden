using System;
using System.Linq;
using LetterGarden.Core;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class PuzzleLevelTests
    {
        [Test]
        public void Constructor_NormalizesLevelData()
        {
            PuzzleLevel level = new PuzzleLevel(
                " level-1 ",
                new[] { 'c', 'a', 't' },
                new[] { " cat ", "CAT", "act" },
                1);

            CollectionAssert.AreEqual(new[] { 'C', 'A', 'T' }, level.AvailableLetters);
            CollectionAssert.AreEqual(new[] { "CAT", "ACT" }, level.ValidWords.ToArray());
            Assert.AreEqual("level-1", level.LevelId);
        }

        [Test]
        public void Constructor_Throws_WhenLevelIdIsMissing()
        {
            Assert.Throws<ArgumentException>(() =>
                new PuzzleLevel(" ", new[] { 'C' }, new[] { "CAT" }, 1));
        }

        [Test]
        public void Constructor_Throws_WhenAvailableLettersAreEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                new PuzzleLevel("level-1", Array.Empty<char>(), new[] { "CAT" }, 1));
        }

        [Test]
        public void Constructor_Throws_WhenAvailableLettersContainWhitespace()
        {
            Assert.Throws<ArgumentException>(() =>
                new PuzzleLevel("level-1", new[] { 'C', ' ' }, new[] { "CAT" }, 1));
        }

        [Test]
        public void Constructor_Throws_WhenValidWordsAreEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                new PuzzleLevel("level-1", new[] { 'C' }, Array.Empty<string>(), 1));
        }

        [Test]
        public void Constructor_Throws_WhenValidWordsContainEmptyEntry()
        {
            Assert.Throws<ArgumentException>(() =>
                new PuzzleLevel("level-1", new[] { 'C' }, new[] { "CAT", " " }, 1));
        }

        [Test]
        public void Constructor_Throws_WhenDifficultyIsNegative()
        {
            Assert.Throws<ArgumentException>(() =>
                new PuzzleLevel("level-1", new[] { 'C' }, new[] { "CAT" }, -1));
        }
    }
}
