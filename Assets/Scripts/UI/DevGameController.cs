using System;
using System.Collections.Generic;
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
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private RequiredWordBoardView requiredWordBoardView;
        [SerializeField] private RectTransform letterButtonContainer;
        [SerializeField] private Button letterButtonPrefab;
        [SerializeField] private float letterButtonRadius = 170f;
        [SerializeField] private float letterButtonFontSize = 72f;
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

            if (keepPlayingButton != null)
            {
                keepPlayingButton.onClick.AddListener(KeepPlaying);
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(LoadNextLevel);
            }

            if (nextLevelAvailableButton != null)
            {
                nextLevelAvailableButton.onClick.AddListener(LoadNextLevel);
            }

            HideLevelCompletePanel();
            HideNextLevelAvailableButton();

            if (levels.Count == 0)
            {
                SetStatusText("No levels found.");
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
            isDraggingLetters = false;

            HideLevelCompletePanel();
            HideNextLevelAvailableButton();
            ShowLetterButtonContainer();
            SetStatusText(string.Empty);
            SetCurrentWordText(string.Empty);
            SetFoundWordsText(string.Empty);
            SetLevelText("Level " + (currentLevelIndex + 1) + " / " + levels.Count);

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

            if (letterButtonContainer == null || letterButtonPrefab == null)
            {
                return;
            }

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
            if (buttonTransform == null)
            {
                return;
            }

            float angle = 360f / totalButtons * index;
            float radians = angle * Mathf.Deg2Rad;
            Vector2 position = new Vector2(
                Mathf.Sin(radians) * letterButtonRadius,
                Mathf.Cos(radians) * letterButtonRadius);

            buttonTransform.anchoredPosition = position;
        }

        public void BeginLetterDrag(int index)
        {
            if (isLevelCompletePanelOpen || gameSession == null)
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
            if (isLevelCompletePanelOpen || !isDraggingLetters || gameSession == null)
            {
                return;
            }

            gameSession.TrySelectLetter(index);
            UpdateUI();
        }

        public void EndLetterDrag()
        {
            if (isLevelCompletePanelOpen || !isDraggingLetters || gameSession == null)
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

            SetStatusText(GetStatusMessage(result));

            if (!couldAdvanceBeforeSubmit
                && gameSession.CanAdvanceToNextLevel
                && result == WordSubmitResult.RequiredWordFound)
            {
                SetStatusText("Level Complete! Keep playing for bonus words.");
                ShowLevelCompletePanel();
            }

            UpdateUI();
        }

        private void KeepPlaying()
        {
            HideLevelCompletePanel();
            ShowLetterButtonContainer();
            isLevelCompletePanelOpen = false;
            ShowNextLevelAvailableButton();
            SetStatusText("Keep playing for bonus words.");
        }

        private void LoadNextLevel()
        {
            int nextLevelIndex = currentLevelIndex + 1;
            HideLevelCompletePanel();
            HideNextLevelAvailableButton();
            isLevelCompletePanelOpen = false;
            isDraggingLetters = false;

            if (nextLevelIndex < levels.Count)
            {
                LoadLevel(nextLevelIndex);
                return;
            }

            HideLetterButtonContainer();
            SetStatusText("No more levels yet.");
        }

        private void ShowLevelCompletePanel()
        {
            if (hasShownLevelCompletePanel)
            {
                return;
            }

            hasShownLevelCompletePanel = true;
            isLevelCompletePanelOpen = true;
            isDraggingLetters = false;

            if (levelCompleteSummaryText != null)
            {
                levelCompleteSummaryText.text = GetLevelCompleteSummaryText();
            }

            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }

            HideLetterButtonContainer();
        }

        private void UpdateUI()
        {
            if (gameSession == null)
            {
                return;
            }

            SetCurrentWordText(string.IsNullOrEmpty(gameSession.CurrentWord) ? "_" : gameSession.CurrentWord);
            RenderRequiredWordBoard();
            SetFoundWordsText(GetFoundWordsText());

            for (int i = 0; i < spawnedLetterButtons.Count; i++)
            {
                Button button = spawnedLetterButtons[i];
                if (button == null)
                {
                    continue;
                }

                bool isSelected = gameSession.IsLetterSelected(i);
                button.interactable = true;
                button.transform.localScale = isSelected ? Vector3.one * 1.12f : Vector3.one;
            }
        }

        private void ResetLetterButtonVisuals()
        {
            foreach (Button button in spawnedLetterButtons)
            {
                if (button == null)
                {
                    continue;
                }

                button.interactable = true;
                button.transform.localScale = Vector3.one;
            }
        }

        private void RenderRequiredWordBoard()
        {
            if (requiredWordBoardView == null || gameSession == null)
            {
                return;
            }

            requiredWordBoardView.Render(gameSession.CurrentLevel.RequiredWords, gameSession.FoundRequiredWords);
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

        private void HideLevelCompletePanel()
        {
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(false);
            }
        }

        private void ShowLetterButtonContainer()
        {
            if (letterButtonContainer != null)
            {
                letterButtonContainer.gameObject.SetActive(true);
            }
        }

        private void HideLetterButtonContainer()
        {
            if (letterButtonContainer != null)
            {
                letterButtonContainer.gameObject.SetActive(false);
            }
        }

        private void ShowNextLevelAvailableButton()
        {
            if (nextLevelAvailableButton != null)
            {
                nextLevelAvailableButton.gameObject.SetActive(true);
            }
        }

        private void HideNextLevelAvailableButton()
        {
            if (nextLevelAvailableButton != null)
            {
                nextLevelAvailableButton.gameObject.SetActive(false);
            }
        }

        private void SetCurrentWordText(string text)
        {
            if (currentWordText != null)
            {
                currentWordText.text = text;
            }
        }

        private void SetFoundWordsText(string text)
        {
            if (foundWordsText != null)
            {
                foundWordsText.text = text;
            }
        }

        private void SetStatusText(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        private void SetLevelText(string text)
        {
            if (levelText != null)
            {
                levelText.text = text;
            }
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
