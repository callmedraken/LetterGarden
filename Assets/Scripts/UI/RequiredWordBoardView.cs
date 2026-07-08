using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetterGarden.UI
{
    public class RequiredWordBoardView : MonoBehaviour
    {
        [SerializeField] private RectTransform boardContainer;
        [SerializeField] private float tileSize = 48f;
        [SerializeField] private float tileSpacing = 8f;
        [SerializeField] private float rowSpacing = 12f;
        [SerializeField] private int fontSize = 32;
        [SerializeField] private float minimumTileSize = 34f;
        [SerializeField] private float minimumSpacing = 4f;
        [SerializeField] private int minimumFontSize = 20;

        private readonly List<GameObject> generatedRows = new List<GameObject>();
        private float currentTileSize;
        private float currentTileSpacing;
        private float currentRowSpacing;
        private int currentFontSize;

        public void Render(IReadOnlyCollection<string> requiredWords, IReadOnlyCollection<string> foundRequiredWords)
        {
            ClearBoard();

            if (boardContainer == null || requiredWords == null)
            {
                return;
            }

            CalculateLayout(requiredWords);
            ApplyBoardLayout();

            HashSet<string> foundWords = new HashSet<string>(
                foundRequiredWords ?? new List<string>(),
                System.StringComparer.OrdinalIgnoreCase);

            foreach (string word in requiredWords)
            {
                GameObject row = CreateRow(word.Length);
                bool isFound = foundWords.Contains(word);

                foreach (char letter in word)
                {
                    CreateTile(row.transform, isFound ? letter.ToString() : "_");
                }
            }
        }

        private void ClearBoard()
        {
            foreach (GameObject row in generatedRows)
            {
                if (row != null)
                {
                    Destroy(row);
                }
            }

            generatedRows.Clear();
        }

        private void ApplyBoardLayout()
        {
            VerticalLayoutGroup layoutGroup = boardContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = boardContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layoutGroup.spacing = currentRowSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }

        private GameObject CreateRow(int letterCount)
        {
            GameObject row = new GameObject("Required Word Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(boardContainer, false);
            generatedRows.Add(row);

            RectTransform rowTransform = row.GetComponent<RectTransform>();
            float rowWidth = letterCount * currentTileSize + Mathf.Max(0, letterCount - 1) * currentTileSpacing;
            rowTransform.sizeDelta = new Vector2(rowWidth, currentTileSize);

            HorizontalLayoutGroup layoutGroup = row.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = currentTileSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            return row;
        }

        private void CreateTile(Transform parent, string text)
        {
            GameObject tile = new GameObject("Letter Tile", typeof(RectTransform), typeof(Image));
            tile.transform.SetParent(parent, false);

            RectTransform tileTransform = tile.GetComponent<RectTransform>();
            tileTransform.sizeDelta = new Vector2(currentTileSize, currentTileSize);

            Image background = tile.GetComponent<Image>();
            background.color = Color.white;

            GameObject textObject = new GameObject("Letter Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(tile.transform, false);

            RectTransform textTransform = textObject.GetComponent<RectTransform>();
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            textTransform.offsetMin = Vector2.zero;
            textTransform.offsetMax = Vector2.zero;

            TextMeshProUGUI tileText = textObject.GetComponent<TextMeshProUGUI>();
            tileText.text = text;
            tileText.fontSize = currentFontSize;
            tileText.alignment = TextAlignmentOptions.Center;
            tileText.color = Color.black;
        }

        private void CalculateLayout(IReadOnlyCollection<string> requiredWords)
        {
            float baseTileSize = Mathf.Max(1f, tileSize);
            float minimumAllowedTileSize = Mathf.Min(minimumTileSize, baseTileSize);
            int rowCount = requiredWords.Count;
            int longestWordLength = 0;

            foreach (string word in requiredWords)
            {
                if (!string.IsNullOrEmpty(word) && word.Length > longestWordLength)
                {
                    longestWordLength = word.Length;
                }
            }

            currentTileSpacing = Mathf.Max(minimumSpacing, Mathf.Min(tileSpacing, baseTileSize * 0.12f));
            currentRowSpacing = Mathf.Max(minimumSpacing, Mathf.Min(rowSpacing, baseTileSize * 0.16f));

            Vector2 availableSize = boardContainer.rect.size;
            if (availableSize.x <= 0f || availableSize.y <= 0f)
            {
                availableSize = boardContainer.sizeDelta;
            }

            float widthLimitedTileSize = baseTileSize;
            if (longestWordLength > 0 && availableSize.x > 0f)
            {
                widthLimitedTileSize =
                    (availableSize.x - Mathf.Max(0, longestWordLength - 1) * currentTileSpacing) / longestWordLength;
            }

            float heightLimitedTileSize = baseTileSize;
            if (rowCount > 0 && availableSize.y > 0f)
            {
                heightLimitedTileSize =
                    (availableSize.y - Mathf.Max(0, rowCount - 1) * currentRowSpacing) / rowCount;
            }

            currentTileSize = Mathf.Clamp(
                Mathf.Min(baseTileSize, widthLimitedTileSize, heightLimitedTileSize),
                minimumAllowedTileSize,
                baseTileSize);

            currentFontSize = Mathf.Max(
                minimumFontSize,
                Mathf.RoundToInt(fontSize * (currentTileSize / baseTileSize)));
        }
    }
}
