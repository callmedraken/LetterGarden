using System;
using System.Reflection;
using NUnit.Framework;

namespace LetterGarden.Core.Tests
{
    public class BonusWordsPopupViewTests
    {
        [Test]
        public void FoundOnlyText_DoesNotIncludeProgressCount()
        {
            string text = InvokeStringMethod("BuildFoundWordsText", new object[] { Array.Empty<string>() });

            StringAssert.DoesNotContain("/", text);
            StringAssert.Contains("No bonus words found yet.", text);
        }

        [Test]
        public void ProgressSummary_IncludesFoundAndTotalCounts()
        {
            string text = InvokeStringMethod("BuildProgressSummaryText", 2, 5);

            StringAssert.Contains("2 / 5", text);
        }

        [Test]
        public void FoundOnlyText_IncludesFoundBonusWords()
        {
            string text = InvokeStringMethod("BuildFoundWordsText", new object[] { new[] { "ATE", "TEA" } });

            StringAssert.Contains("ATE", text);
            StringAssert.Contains("TEA", text);
        }

        private static string InvokeStringMethod(string methodName, params object[] arguments)
        {
            Type popupType = Type.GetType("LetterGarden.UI.BonusWordsPopupView, Assembly-CSharp");
            Assert.IsNotNull(popupType, "BonusWordsPopupView type should be available to Edit Mode tests.");

            MethodInfo method = popupType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, methodName + " should be public and static.");

            object result = method.Invoke(null, arguments);
            return (string)result;
        }
    }
}
