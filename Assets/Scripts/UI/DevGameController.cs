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
        [SerializeField] private RectTransform letterButtonContainer;
        [SerializeField] private Button letterButtonPrefab;
        [SerializeField] private float letterButtonRadius = 170f;
        [SerializeField] private float letterButtonFontSize = 72f;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button backspaceButton;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private TMP_Text levelCompleteSummaryText;
        [SerializeField] private Button keepPlayingButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button nextLevelAvailableButton;

        private readonly List<PuzzleLevel> levels = new List<PuzzleLevel>();
        private readonly List<Button> spawnedLetterButtons = new List<Button>();
        private GameSession gameSession;
        private int currentLevelIndex;
        private bool hasShownLevelCompletePanel;
        private bool isLevelCompletePanelOpen;
        private bool isDraggingLetters;

        private void Start()
        {
            LoadLevels();

            if (clearButton != null)
            {
                clearButton.onClick.AddListener(ClearCurrentWord);
            }

            if (backspaceButton != null)
            {
                backspaceButton.onClick.AddListener(BackspaceLetter);
            }

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
            isLevelCompletePanelOpen = false;

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
            ClearLetterButtons();

            int letterCount = level.AvailableLetters.Count;

            for (int i = 0; i < letterCount; i++)
            {
                int letterIndex = i;
                Button button = Instantiate(letterButtonPrefab, letterButtonContainer);
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                SetLetterButtonPosition(button, letterIndex, letterCount);

                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = level.AvailableLetters[letterIndex].ToString();
                    buttonText.fontSize = letterButtonFontSize;
                    buttonText.alignment = TextAlignmentOptions.Center;
                }

                LetterButtonInput input = button.GetComponent<LetterButtonInput>();
                if (input == null)
                {
                    input = button.gameObject.AddComponent<LetterButtonInput>();
                }

                input.Initialize(letterIndex, this);
                spawnedLetterButtons.Add(button);
            }
        }

        private void ClearLetterButtons()
        {
            foreach (Button button in spawnedLetterButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            spawnedLetterButtons.Clear();
        }

        private void SetLetterButtonPosition(Button button, int index, int totalButtons)
        {
            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            float angle = 360f / totalButtons * index;
            float radians = angle * Mathf.Deg2Rad;
            Vector2 position = new Vector2(
                Mathf.Sin(radians) * letterButtonRadius,
                Mathf.Cos(radians) * letterButtonRadius);

            buttonTransform.anchoredPosition = position;
        }

        private void SelectLetter(int index)
        {
            gameSession.TrySelectLetter(index);
            UpdateUI();
        }

        public void BeginLetterDrag(int index)
        {
            if (isLevelCompletePanelOpen)
            {
                return;
            }

            isDraggingLetters = true;
            gameSession.ClearCurrentWord();
            gameSession.TrySelectLetter(index);
            UpdateUI();
        }

        public void ContinueLetterDrag(int index)
        {
            if (isLevelCompletePanelOpen || !isDraggingLetters)
            {
                return;
            }

            gameSession.TrySelectLetter(index);
            UpdateUI();
        }

        public void EndLetterDrag()
        {
            if (isLevelCompletePanelOpen || !isDraggingLetters)
            {
                return;
            }

            isDraggingLetters = false;
            SubmitCurrentWord();
            ResetLetterButtonVisuals();
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
            ResetLetterButtonVisuals();
        }

        private void BackspaceLetter()
        {
            gameSession.BackspaceLetter();
            UpdateUI();
        }

        private void KeepPlaying()
        {
            levelCompletePanel.SetActive(false);
            isLevelCompletePanelOpen = false;
            nextLevelAvailableButton.gameObject.SetActive(true);
            statusText.text = "Keep playing for bonus words.";
        }

        private void LoadNextLevel()
        {
            int nextLevelIndex = currentLevelIndex + 1;
            levelCompletePanel.SetActive(false);
            nextLevelAvailableButton.gameObject.SetActive(false);
            isLevelCompletePanelOpen = false;

            if (nextLevelIndex < levels.Count)
            {
                LoadLevel(nextLevelIndex);
                return;
            }

            statusText.text = "No more levels yet.";
        }

        private void ShowLevelCompletePanel()
        {
            if (hasShownLevelCompletePanel)
            {
                return;
            }

            hasShownLevelCompletePanel = true;
            isLevelCompletePanelOpen = true;
            levelCompleteSummaryText.text = GetLevelCompleteSummaryText();
            levelCompletePanel.SetActive(true);
        }

        private void UpdateUI()
        {
            currentWordText.text = string.IsNullOrEmpty(gameSession.CurrentWord) ? "_" : gameSession.CurrentWord;
            foundWordsText.text = GetFoundWordsText();

            for (int i = 0; i < spawnedLetterButtons.Count; i++)
            {
                Button button = spawnedLetterButtons[i];
                button.interactable = true;
                button.transform.localScale = gameSession.IsLetterSelected(i)
                    ? Vector3.one * 1.12f
                    : Vector3.one;
            }
        }

        private void ResetLetterButtonVisuals()
        {
            foreach (Button button in spawnedLetterButtons)
            {
                if (button != null)
                {
                    button.interactable = true;
                    button.transform.localScale = Vector3.one;
                }
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
