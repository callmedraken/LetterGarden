using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetterGarden.UI
{
    public sealed class BonusWordsPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text bonusWordsSummaryText;
        [SerializeField] private TMP_Text bonusWordsText;
        [SerializeField] private Button closeButton;

        private Action closeRequested;

        public bool IsOpen
        {
            get
            {
                GameObject root = GetPopupRoot();
                return root != null && root.activeSelf;
            }
        }

        public void Initialize(Action onCloseRequested)
        {
            closeRequested = onCloseRequested;

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(RequestClose);
                closeButton.onClick.AddListener(RequestClose);
            }

            Hide();
        }

        public void ShowFoundOnly(IReadOnlyCollection<string> foundBonusWords)
        {
            RenderFoundOnly(foundBonusWords);

            GameObject root = GetPopupRoot();
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void ShowProgress(IReadOnlyCollection<string> foundBonusWords, int foundCount, int totalCount)
        {
            RenderProgress(foundBonusWords, foundCount, totalCount);

            GameObject root = GetPopupRoot();
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Hide()
        {
            GameObject root = GetPopupRoot();
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public void RenderFoundOnly(IReadOnlyCollection<string> foundBonusWords)
        {
            SetSummaryText(string.Empty);
            SetListText(BuildFoundWordsText(foundBonusWords));
        }

        public void RenderProgress(IReadOnlyCollection<string> foundBonusWords, int foundCount, int totalCount)
        {
            SetSummaryText(BuildProgressSummaryText(foundCount, totalCount));
            SetListText(BuildFoundWordsText(foundBonusWords));
        }

        public static string BuildFoundWordsText(IReadOnlyCollection<string> foundBonusWords)
        {
            List<string> lines = new List<string>
            {
                "Bonus Words Found",
                string.Empty
            };

            if (foundBonusWords == null || foundBonusWords.Count == 0)
            {
                lines.Add("No bonus words found yet.");
            }
            else
            {
                foreach (string word in foundBonusWords)
                {
                    lines.Add(word);
                }
            }

            return string.Join("\n", lines);
        }

        public static string BuildProgressSummaryText(int foundCount, int totalCount)
        {
            return "Found: " + foundCount + " / " + totalCount;
        }

        public static BonusWordsPopupView CreateDefault(Transform parent)
        {
            GameObject root = new GameObject("BonusWordsPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(parent, false);

            RectTransform rootTransform = root.GetComponent<RectTransform>();
            rootTransform.anchorMin = Vector2.zero;
            rootTransform.anchorMax = Vector2.one;
            rootTransform.offsetMin = Vector2.zero;
            rootTransform.offsetMax = Vector2.zero;

            Image dimBackground = root.GetComponent<Image>();
            dimBackground.color = new Color(0f, 0f, 0f, 0.55f);
            dimBackground.raycastTarget = true;

            BonusWordsPopupView popupView = root.AddComponent<BonusWordsPopupView>();
            popupView.popupRoot = root;

            GameObject card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(root.transform, false);

            RectTransform cardTransform = card.GetComponent<RectTransform>();
            cardTransform.anchorMin = new Vector2(0.5f, 0.5f);
            cardTransform.anchorMax = new Vector2(0.5f, 0.5f);
            cardTransform.pivot = new Vector2(0.5f, 0.5f);
            cardTransform.anchoredPosition = Vector2.zero;
            cardTransform.sizeDelta = new Vector2(560f, 520f);

            Image cardImage = card.GetComponent<Image>();
            cardImage.color = new Color(0.08f, 0.1f, 0.13f, 0.96f);
            cardImage.raycastTarget = true;

            TMP_Text summaryText = CreateText("BonusWordsSummaryText", card.transform);
            RectTransform summaryTransform = summaryText.GetComponent<RectTransform>();
            summaryTransform.anchorMin = new Vector2(0.08f, 0.78f);
            summaryTransform.anchorMax = new Vector2(0.92f, 0.9f);
            summaryTransform.offsetMin = Vector2.zero;
            summaryTransform.offsetMax = Vector2.zero;
            summaryText.fontSize = 30f;
            summaryText.alignment = TextAlignmentOptions.Center;
            popupView.bonusWordsSummaryText = summaryText;

            TMP_Text listText = CreateText("BonusWordsListText", card.transform);
            RectTransform listTransform = listText.GetComponent<RectTransform>();
            listTransform.anchorMin = new Vector2(0.08f, 0.22f);
            listTransform.anchorMax = new Vector2(0.92f, 0.75f);
            listTransform.offsetMin = Vector2.zero;
            listTransform.offsetMax = Vector2.zero;
            listText.fontSize = 34f;
            listText.alignment = TextAlignmentOptions.Top;
            listText.enableWordWrapping = true;
            popupView.bonusWordsText = listText;

            Button close = CreateButton("CloseButton", card.transform, "Close");
            RectTransform closeTransform = close.GetComponent<RectTransform>();
            closeTransform.anchorMin = new Vector2(0.5f, 0f);
            closeTransform.anchorMax = new Vector2(0.5f, 0f);
            closeTransform.pivot = new Vector2(0.5f, 0f);
            closeTransform.anchoredPosition = new Vector2(0f, 44f);
            closeTransform.sizeDelta = new Vector2(220f, 72f);
            popupView.closeButton = close;

            root.SetActive(false);
            return popupView;
        }

        private static TMP_Text CreateText(string name, Transform parent)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.9f, 0.92f, 0.96f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;

            TMP_Text buttonText = CreateText("Text", buttonObject.transform);
            RectTransform textTransform = buttonText.GetComponent<RectTransform>();
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            textTransform.offsetMin = Vector2.zero;
            textTransform.offsetMax = Vector2.zero;
            buttonText.text = label;
            buttonText.color = new Color(0.08f, 0.1f, 0.13f, 1f);
            buttonText.fontSize = 30f;
            buttonText.alignment = TextAlignmentOptions.Center;

            return button;
        }

        private GameObject GetPopupRoot()
        {
            return popupRoot != null ? popupRoot : gameObject;
        }

        private void SetSummaryText(string text)
        {
            if (bonusWordsSummaryText != null)
            {
                bonusWordsSummaryText.text = text;
            }
        }

        private void SetListText(string text)
        {
            if (bonusWordsText != null)
            {
                bonusWordsText.text = text;
            }
        }

        private void RequestClose()
        {
            closeRequested?.Invoke();
        }
    }
}
