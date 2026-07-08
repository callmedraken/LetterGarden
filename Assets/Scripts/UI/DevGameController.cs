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

        private GameSession gameSession;
        private bool hasShownLevelCompletePanel;

        private void Start()
        {
            PuzzleLevel level = new PuzzleLevel(
                "dev-cat",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT", "ACT" },
                new[] { "AT" },
                1);

            gameSession = new GameSession(level);

            SetupLetterButtons(level);
            submitButton.onClick.AddListener(SubmitCurrentWord);
            clearButton.onClick.AddListener(ClearCurrentWord);
            backspaceButton.onClick.AddListener(BackspaceLetter);
            keepPlayingButton.onClick.AddListener(KeepPlaying);
            nextLevelButton.onClick.AddListener(ShowNextLevelPlaceholder);
            nextLevelAvailableButton.onClick.AddListener(ShowNextLevelPlaceholder);

            levelCompletePanel.SetActive(false);
            nextLevelAvailableButton.gameObject.SetActive(false);
            statusText.text = string.Empty;
            UpdateUI();
        }

        private void SetupLetterButtons(PuzzleLevel level)
        {
            for (int i = 0; i < letterButtons.Length; i++)
            {
                int letterIndex = i;
                Button button = letterButtons[i];

                if (letterIndex >= level.AvailableLetters.Count)
                {
                    button.interactable = false;
                    continue;
                }

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

        private void ShowNextLevelPlaceholder()
        {
            levelCompletePanel.SetActive(false);
            statusText.text = "Next level not implemented yet.";
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
    }
}
