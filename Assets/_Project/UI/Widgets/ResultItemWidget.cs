using System.Collections.Generic;
using Project.Domain.Quest;
using TMPro;
using UnityEngine;

namespace Project.UI.Widgets
{
    public class ResultItemWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _resultText;

        public void Init(QuestResult result)
        {
            if (_resultText == null || result == null)
            {
                return;
            }

            var reasons = BuildReasonsText(result.TopReasons);
            var delta = result.Delta;

            _resultText.text =
                $"{GetQuestLabel(result)} | {result.Result} | Chance {result.FinalSuccessChance}%\n" +
                $"Reasons: {reasons}\n" +
                $"Δ Rep {delta.Reputation:+#;-#;0} / Stab {delta.Stability:+#;-#;0} / Bud {delta.Budget:+#;-#;0} / Inf {delta.Influence:+#;-#;0} / Cas {delta.Casualties:+#;-#;0}";
        }

        private static string GetQuestLabel(QuestResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.QuestId))
            {
                return result.QuestId;
            }

            return "UnknownQuest";
        }

        private static string BuildReasonsText(List<string> reasons)
        {
            if (reasons == null || reasons.Count == 0)
            {
                return "-";
            }

            var count = Mathf.Min(3, reasons.Count);
            return string.Join(", ", reasons.GetRange(0, count));
        }
    }
}
