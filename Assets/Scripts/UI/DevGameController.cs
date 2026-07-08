using System;
using System.Collections.Generic;
using System.Linq;
using LetterGarden.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetterGarden.UI
{
    public class DevGameController : MonoBehaviour
    {
        [SerializeField] private TMP_Text currentWordText;
        [SerializeField] private TMP_Text foundWordsText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button[] letterButtons;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button backspaceButton;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private TMP_Text levelCompleteSummaryText;
        [SerializeField] private Button keepPlayingButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button nextLevelAvailableButton;

        private readonly List<PuzzleLevel> levels = new List<PuzzleLevel>();
        private GameSession gameSession;
        private int currentLevelIndex;
        private bool hasShownLevelCompletePanel;

        private void Start()
        {
            LoadLevels();

            submitButton.onClick.AddListener(SubmitCurrentWord);
            clearButton.onClick.AddListener(ClearCurrentWord);
            backspaceButton.onClick.AddListener(BackspaceLetter);
            keepPlayingButton.onClick.AddListener(KeepPlaying);
            nextLevelButton.onClick.AddListener(LoadNextLevel);
            nextLevelAvailableButton.onClick.AddListener(LoadNextLevel);

            levelCompletePanel.SetActive(false);
            nextLevelAvailableButton.gameObject.SetActive(false);

            if (levels.Count == 0)
            {
                statusText.text = "No levels found.";
                return;
            }

            LoadLevel(0);
        }

        public void LoadLevel(int index)
        {
            if (index < 0 || index >= levels.Count)
            {
                return;
            }

            currentLevelIndex = index;
            gameSession = new GameSession(levels[currentLevelIndex]);
            hasShownLevelCompletePanel = false;

            levelCompletePanel.SetActive(false);
            nextLevelAvailableButton.gameObject.SetActive(false);
            statusText.text = string.Empty;
            currentWordText.text = string.Empty;
            foundWordsText.text = string.Empty;

            SetupLetterButtons(gameSession.CurrentLevel);
            UpdateUI();
        }

        private void LoadLevels()
        {
            levels.Clear();

            TextAsset levelsJson = Resources.Load<TextAsset>("Levels/dev_levels");
            if (levelsJson == null)
            {
                return;
            }

            LevelCollection levelCollection = JsonUtility.FromJson<LevelCollection>(levelsJson.text);
            if (levelCollection == null || levelCollection.levels == null)
            {
                return;
            }

            foreach (LevelData levelData in levelCollection.levels)
            {
                levels.Add(CreatePuzzleLevel(levelData));
            }
        }

        private PuzzleLevel CreatePuzzleLevel(LevelData levelData)
        {
            return new PuzzleLevel(
                levelData.levelId,
                levelData.letters.ToCharArray(),
                levelData.requiredWords ?? Array.Empty<string>(),
                levelData.bonusWords ?? Array.Empty<string>(),
                levelData.difficulty);
        }

        private void SetupLetterButtons(PuzzleLevel level)
        {
            for (int i = 0; i < letterButtons.Length; i++)
            {
                int letterIndex = i;
                Button button = letterButtons[i];
                button.onClick.RemoveAllListeners();

                if (letterIndex >= level.AvailableLetters.Count)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                button.gameObject.SetActive(true);
                button.interactable = true;

                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = level.AvailableLetters[letterIndex].ToString();
                }

                button.onClick.AddListener(() => SelectLetter(letterIndex));
            }
        }

        private void SelectLetter(int index)
        {
            gameSession.TrySelectLetter(index);
            UpdateUI();
        }

        private void SubmitCurrentWord()
        {
            bool couldAdvanceBeforeSubmit = gameSession.CanAdvanceToNextLevel;
            WordSubmitResult result = gameSession.SubmitCurrentWord();

            statusText.text = GetStatusMessage(result);

            if (!couldAdvanceBeforeSubmit
                && gameSession.CanAdvanceToNextLevel
                && result == WordSubmitResult.RequiredWordFound)
            {
                statusText.text = "Level Complete! Keep playing for bonus words.";
                ShowLevelCompletePanel();
            }

            UpdateUI();
        }

        private void ClearCurrentWord()
        {
            gameSession.ClearCurrentWord();
            UpdateUI();
        }

        private void BackspaceLetter()
        {
            gameSession.BackspaceLetter();
            UpdateUI();
        }

        private void KeepPlaying()
        {
            levelCompletePanel.SetActive(false);
            statusText.text = "Keep playing for bonus words.";
        }

        private void LoadNextLevel()
        {
            int nextLevelIndex = currentLevelIndex + 1;

            if (nextLevelIndex < levels.Count)
            {
                LoadLevel(nextLevelIndex);
                return;
            }

            levelCompletePanel.SetActive(false);
            statusText.text = "No more levels yet.";
        }

        private void ShowLevelCompletePanel()
        {
            if (hasShownLevelCompletePanel)
            {
                return;
            }

            hasShownLevelCompletePanel = true;
            levelCompleteSummaryText.text = GetLevelCompleteSummaryText();
            levelCompletePanel.SetActive(true);
            nextLevelAvailableButton.gameObject.SetActive(true);
        }

        private void UpdateUI()
        {
            currentWordText.text = string.IsNullOrEmpty(gameSession.CurrentWord) ? "_" : gameSession.CurrentWord;
            foundWordsText.text = GetFoundWordsText();

            for (int i = 0; i < letterButtons.Length; i++)
            {
                bool hasMatchingLetter = i < gameSession.CurrentLevel.AvailableLetters.Count;
                letterButtons[i].interactable = hasMatchingLetter && !gameSession.IsLetterSelected(i);
            }
        }

        private string GetStatusMessage(WordSubmitResult result)
        {
            switch (result)
            {
                case WordSubmitResult.RequiredWordFound:
                    return "Found!";
                case WordSubmitResult.BonusWordFound:
                    return "Bonus Word!";
                case WordSubmitResult.AlreadyFoundRequired:
                    return "Already found";
                case WordSubmitResult.AlreadyFoundBonus:
                    return "Bonus already found";
                default:
                    return "Not a word";
            }
        }

        private string GetLevelCompleteSummaryText()
        {
            return "Level Complete!\n\n"
                + "Required Words: "
                + gameSession.FoundRequiredWords.Count
                + " / "
                + gameSession.CurrentLevel.RequiredWords.Count
                + "\n"
                + "Bonus Words: "
                + gameSession.FoundBonusWords.Count
                + " / "
                + gameSession.CurrentLevel.BonusWords.Count;
        }

        private string GetFoundWordsText()
        {
            List<string> lines = new List<string>();
            lines.Add("Required Words:");

            foreach (string word in gameSession.CurrentLevel.RequiredWords)
            {
                lines.Add(GetVisibleRequiredWord(word));
            }

            lines.Add(string.Empty);
            lines.Add("Bonus Words Found:");

            if (gameSession.FoundBonusWords.Count == 0)
            {
                lines.Add("-");
            }
            else
            {
                foreach (string word in gameSession.FoundBonusWords)
                {
                    lines.Add(word);
                }
            }

            return string.Join("\n", lines);
        }

        private string GetVisibleRequiredWord(string word)
        {
            if (gameSession.FoundRequiredWords.Contains(word))
            {
                return word;
            }

            return string.Join(" ", new string('_', word.Length).ToCharArray());
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
