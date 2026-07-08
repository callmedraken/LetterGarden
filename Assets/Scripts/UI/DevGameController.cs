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

        private GameSession gameSession;

        private void Start()
        {
            PuzzleLevel level = new PuzzleLevel(
                "dev-cat",
                new[] { 'C', 'A', 'T' },
                new[] { "CAT", "AT" },
                1);

            gameSession = new GameSession(level);

            SetupLetterButtons(level);
            submitButton.onClick.AddListener(SubmitCurrentWord);
            clearButton.onClick.AddListener(ClearCurrentWord);
            backspaceButton.onClick.AddListener(BackspaceLetter);

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
            bool wordWasFound = gameSession.SubmitCurrentWord();
            statusText.text = wordWasFound ? "Found!" : "Not a word";

            if (gameSession.IsComplete)
            {
                statusText.text = "Level Complete!";
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

        private string GetFoundWordsText()
        {
            string[] foundWords = gameSession.CurrentLevel.ValidWords
                .Where(gameSession.FoundWords.Contains)
                .ToArray();

            if (foundWords.Length == 0)
            {
                return "Found Words:\n-";
            }

            return "Found Words:\n" + string.Join("\n", foundWords);
        }
    }
}
