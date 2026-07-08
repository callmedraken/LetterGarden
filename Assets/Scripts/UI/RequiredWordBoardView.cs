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

        private readonly List<GameObject> generatedRows = new List<GameObject>();

        public void Render(IReadOnlyCollection<string> requiredWords, IReadOnlyCollection<string> foundRequiredWords)
        {
            ClearBoard();

            if (boardContainer == null || requiredWords == null)
            {
                return;
            }

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

            layoutGroup.spacing = rowSpacing;
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
            float rowWidth = letterCount * tileSize + Mathf.Max(0, letterCount - 1) * tileSpacing;
            rowTransform.sizeDelta = new Vector2(rowWidth, tileSize);

            HorizontalLayoutGroup layoutGroup = row.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = tileSpacing;
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
            tileTransform.sizeDelta = new Vector2(tileSize, tileSize);

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
            tileText.fontSize = fontSize;
            tileText.alignment = TextAlignmentOptions.Center;
            tileText.color = Color.black;
        }
    }
}
