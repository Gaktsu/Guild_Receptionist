using System;
using Project.Domain.Quest;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Widgets
{
    public class QuestDraftItemWidget : MonoBehaviour
    {
        [SerializeField] private Text _draftText;
        [SerializeField] private Button _submitButton;

        private QuestDraft _draft;
        private Action<string> _onSubmitClicked;

        public void Init(QuestDraft draft, bool canSubmit, Action<string> onSubmitClicked)
        {
            _draft = draft;
            _onSubmitClicked = onSubmitClicked;

            if (_submitButton != null)
            {
                _submitButton.onClick.RemoveAllListeners();
                _submitButton.onClick.AddListener(HandleSubmitClicked);
                _submitButton.gameObject.SetActive(canSubmit);
            }

            RefreshText();
        }

        public void Refresh(bool canSubmit)
        {
            if (_submitButton != null)
            {
                _submitButton.gameObject.SetActive(canSubmit);
            }

            RefreshText();
        }

        private void RefreshText()
        {
            if (_draftText == null || _draft == null)
            {
                return;
            }

            var sourceCount = _draft.SourceInfoIds != null ? _draft.SourceInfoIds.Count : 0;
            _draftText.text =
                $"{_draft.Id} | {_draft.Type} | Risk {_draft.Risk} | Reward {_draft.Reward} | Deadline {_draft.DeadlineDays} | Source {sourceCount}";
        }

        private void HandleSubmitClicked()
        {
            if (_draft == null)
            {
                return;
            }

            _onSubmitClicked?.Invoke(_draft.Id);
        }
    }
}
