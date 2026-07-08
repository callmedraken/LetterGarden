using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace LetterGarden.Core.Tests
{
    public class DevLevelsTests
    {
        [Test]
        public void DevLevels_IncludePlayableStreamLevel()
        {
            LevelCollection levelCollection = LoadDevLevels();

            Assert.GreaterOrEqual(levelCollection.levels.Length, 4);

            LevelData levelFour = levelCollection.levels[3];
            Assert.AreEqual("STREAM", levelFour.letters);
            Assert.IsTrue(levelFour.requiredWords.Any(word => word.Length == 6));

            foreach (string requiredWord in levelFour.requiredWords)
            {
                Assert.IsTrue(
                    CanFormWord(requiredWord, levelFour.letters),
                    requiredWord + " should be formable from " + levelFour.letters + ".");
            }
        }

        private static LevelCollection LoadDevLevels()
        {
            TextAsset levelsJson = Resources.Load<TextAsset>("Levels/dev_levels");
            Assert.IsNotNull(levelsJson);

            LevelCollection levelCollection = JsonUtility.FromJson<LevelCollection>(levelsJson.text);
            Assert.IsNotNull(levelCollection);
            Assert.IsNotNull(levelCollection.levels);

            return levelCollection;
        }

        private static bool CanFormWord(string word, string availableLetters)
        {
            if (string.IsNullOrWhiteSpace(word) || string.IsNullOrWhiteSpace(availableLetters))
            {
                return false;
            }

            Dictionary<char, int> letterCounts = new Dictionary<char, int>();

            foreach (char letter in availableLetters.ToUpperInvariant())
            {
                if (!letterCounts.ContainsKey(letter))
                {
                    letterCounts[letter] = 0;
                }

                letterCounts[letter]++;
            }

            foreach (char letter in word.ToUpperInvariant())
            {
                if (!letterCounts.ContainsKey(letter) || letterCounts[letter] == 0)
                {
                    return false;
                }

                letterCounts[letter]--;
            }

            return true;
        }

        [Serializable]
        private class LevelCollection
        {
            public LevelData[] levels;
        }

        [Serializable]
        private class LevelData
        {
            public string levelId;
            public string letters;
            public string[] requiredWords;
            public string[] bonusWords;
            public int difficulty;
        }
    }
}
