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
        [SerializeField] private DragPathView dragPathView;
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
        private readonly List<string> dictionaryWords = new List<string>();
        private readonly List<Button> spawnedLetterButtons = new List<Button>();
        private GameSession gameSession;
        private int currentLevelIndex;
        private bool hasShownLevelCompletePanel;
        private bool isLevelCompletePanelOpen;
        private bool isDraggingLetters;

        private void Awake()
        {
            ResolveDragPathView();
        }

        private void Start()
        {
            ResolveDragPathView();
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
            ClearLetterDragPath();
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
            LoadDictionary();

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
            char[] availableLetters = levelData.letters.ToCharArray();
            string[] requiredWords = levelData.requiredWords ?? Array.Empty<string>();
            List<string> acceptedBonusSource = new List<string>(dictionaryWords);

            if (levelData.bonusWords != null)
            {
                acceptedBonusSource.AddRange(levelData.bonusWords);
            }

            IReadOnlyCollection<string> bonusWords = BonusWordGenerator.GenerateBonusWords(
                acceptedBonusSource,
                availableLetters,
                requiredWords);

            return new PuzzleLevel(
                levelData.levelId,
                availableLetters,
                requiredWords,
                bonusWords,
                levelData.difficulty);
        }

        private void LoadDictionary()
        {
            dictionaryWords.Clear();

            TextAsset dictionaryText = Resources.Load<TextAsset>("Dictionaries/common_words");
            if (dictionaryText == null)
            {
                return;
            }

            string[] words = dictionaryText.text.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    dictionaryWords.Add(word.Trim().ToUpperInvariant());
                }
            }
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
                ClearLetterDragPath();
                return;
            }

            isDraggingLetters = true;
            gameSession.ClearCurrentWord();
            ClearLetterDragPath();

            gameSession.TrySelectLetter(index);
            UpdateLetterDragPath();
            UpdateUI();
        }

        public void ContinueLetterDrag(int index)
        {
            if (isLevelCompletePanelOpen || !isDraggingLetters || gameSession == null)
            {
                if (isLevelCompletePanelOpen)
                {
                    ClearLetterDragPath();
                }

                return;
            }

            if (gameSession.TrySelectLetter(index))
            {
                UpdateLetterDragPath();
            }

            UpdateUI();
        }

        public void EndLetterDrag()
        {
            if (isLevelCompletePanelOpen || !isDraggingLetters || gameSession == null)
            {
                if (isLevelCompletePanelOpen)
                {
                    ClearLetterDragPath();
                }

                return;
            }

            isDraggingLetters = false;
            SubmitCurrentWord();
            ResetLetterButtonVisuals();
            ClearLetterDragPath();
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
            ClearLetterDragPath();
            ShowNextLevelAvailableButton();
            SetStatusText("Keep playing for bonus words.");
        }

        private void LoadNextLevel()
        {
            int nextLevelIndex = currentLevelIndex + 1;
            HideLevelCompletePanel();
            isLevelCompletePanelOpen = false;
            isDraggingLetters = false;
            ClearLetterDragPath();

            if (nextLevelIndex < levels.Count)
            {
                HideNextLevelAvailableButton();
                LoadLevel(nextLevelIndex);
                return;
            }

            ShowLetterButtonContainer();
            if (nextLevelAvailableButton != null)
            {
                nextLevelAvailableButton.interactable = false;
            }

            UpdateUI();
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
            ClearLetterDragPath();

            if (levelCompleteSummaryText != null)
            {
                levelCompleteSummaryText.text = GetLevelCompleteSummaryText();
            }

            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }

            HideNextLevelAvailableButton();
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

        private void UpdateLetterDragPath()
        {
            ResolveDragPathView();

            if (dragPathView == null || gameSession == null || isLevelCompletePanelOpen)
            {
                if (isLevelCompletePanelOpen)
                {
                    ClearLetterDragPath();
                }

                return;
            }

            RectTransform dragPathTransform = dragPathView.GetComponent<RectTransform>();
            if (dragPathTransform == null)
            {
                return;
            }

            Camera dragPathCamera = GetCanvasCamera(dragPathView.transform);
            List<Vector2> pathPoints = new List<Vector2>();
            IReadOnlyList<int> selectedIndices = gameSession.SelectedLetterIndices;

            for (int i = 0; i < selectedIndices.Count; i++)
            {
                int selectedIndex = selectedIndices[i];
                if (selectedIndex < 0 || selectedIndex >= spawnedLetterButtons.Count)
                {
                    continue;
                }

                Button selectedButton = spawnedLetterButtons[selectedIndex];
                if (selectedButton == null)
                {
                    continue;
                }

                RectTransform selectedTransform = selectedButton.GetComponent<RectTransform>();
                if (selectedTransform != null)
                {
                    Vector3 worldCenter = selectedTransform.TransformPoint(selectedTransform.rect.center);
                    Camera selectedButtonCamera = GetCanvasCamera(selectedTransform);
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(selectedButtonCamera, worldCenter);

                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        dragPathTransform,
                        screenPoint,
                        dragPathCamera,
                        out Vector2 localPoint))
                    {
                        pathPoints.Add(localPoint);
                    }
                }
            }

            dragPathView.SetPath(pathPoints);
        }

        private void ClearLetterDragPath()
        {
            ResolveDragPathView();

            if (dragPathView != null)
            {
                dragPathView.Clear();
            }
        }

        private void ResolveDragPathView()
        {
            if (dragPathView == null)
            {
                dragPathView = FindFirstObjectByType<DragPathView>();
            }
        }

        private Camera GetCanvasCamera(Transform uiTransform)
        {
            if (uiTransform == null)
            {
                return null;
            }

            Canvas parentCanvas = uiTransform.GetComponentInParent<Canvas>();
            if (parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return parentCanvas.worldCamera != null ? parentCanvas.worldCamera : Camera.main;
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
                + "All required words found.\n\n"
                + "Required Words: "
                + gameSession.FoundRequiredWords.Count
                + " / "
                + gameSession.CurrentLevel.RequiredWords.Count
                + "\n"
                + "Bonus Words: "
                + gameSession.FoundBonusWords.Count
                + " / "
                + gameSession.CurrentLevel.BonusWords.Count
                + "\n\n"
                + "Keep playing for bonus words or continue.";
        }

        private string GetFoundWordsText()
        {
            return "Bonus Words: "
                + gameSession.FoundBonusWords.Count
                + " / "
                + gameSession.CurrentLevel.BonusWords.Count;
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
                nextLevelAvailableButton.interactable = true;
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
