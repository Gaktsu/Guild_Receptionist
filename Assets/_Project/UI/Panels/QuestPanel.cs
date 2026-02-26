using System;
using System.Collections.Generic;
using Project.Core;
using Project.Domain.Info;
using Project.Domain.Quest;
using Project.Systems.Day;
using Project.Systems.Game;
using Project.Systems.Info;
using Project.Systems.Quest;
using Project.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Panels
{
    public class QuestPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;

        [Header("Draft Phase")]
        [SerializeField] private GameObject _draftSectionRoot;
        [SerializeField] private Transform _infoToggleListRoot;
        [SerializeField] private Toggle _infoTogglePrefab;
        [SerializeField] private TMP_Dropdown _typeDropdown;
        [SerializeField] private TMP_Dropdown _riskDropdown;
        [SerializeField] private TMP_InputField _rewardInput;
        [SerializeField] private TMP_Dropdown _deadlineDropdown;
        [SerializeField] private Button _createDraftButton;

        [Header("Submission Phase")]
        [SerializeField] private GameObject _submissionSectionRoot;

        [Header("Shared")]
        [SerializeField] private Transform _draftListRoot;
        [SerializeField] private QuestDraftItemWidget _draftItemPrefab;
        [SerializeField] private TextMeshProUGUI _submittedText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _toSubmissionButton;
        [SerializeField] private Button _toResolutionButton;

        private readonly List<Toggle> _spawnedInfoToggles = new List<Toggle>();
        private readonly List<InfoData> _selectableInfos = new List<InfoData>();

        private ToggleGroup _infoToggleGroup;

        private QuestSystem _questSystem;
        private InfoSystem _infoSystem;
        private DaySystem _daySystem;
        private GameSession _session;

        public void Init(QuestSystem questSystem, InfoSystem infoSystem, DaySystem daySystem, GameSession session)
        {
            _questSystem = questSystem;
            _infoSystem = infoSystem;
            _session = session;

            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }

            _daySystem = daySystem;
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged += HandleStateChanged;
            }

            BindInputs();
            SetupDropdowns();
            UpdateRewardDefault();
            HandleStateChanged(_daySystem != null ? _daySystem.CurrentState : DayState.DayStart);
        }

        private void OnDestroy()
        {
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }
        }

        private void BindInputs()
        {
            if (_createDraftButton != null)
            {
                _createDraftButton.onClick.RemoveAllListeners();
                _createDraftButton.onClick.AddListener(CreateDraft);
            }

            if (_riskDropdown != null)
            {
                _riskDropdown.onValueChanged.RemoveAllListeners();
                _riskDropdown.onValueChanged.AddListener(_ => UpdateRewardDefault());
            }

            if (_toSubmissionButton != null)
            {
                _toSubmissionButton.onClick.RemoveAllListeners();
                _toSubmissionButton.onClick.AddListener(HandleToSubmissionClicked);
            }

            if (_toResolutionButton != null)
            {
                _toResolutionButton.onClick.RemoveAllListeners();
                _toResolutionButton.onClick.AddListener(HandleToResolutionClicked);
            }
        }

        private void SetupDropdowns()
        {
            if (_typeDropdown != null)
            {
                _typeDropdown.ClearOptions();
                var options = new List<string>();
                var values = Enum.GetValues(typeof(QuestTemplateType));
                for (var i = 0; i < values.Length; i++)
                {
                    options.Add(values.GetValue(i).ToString());
                }

                _typeDropdown.AddOptions(options);
            }

            if (_riskDropdown != null)
            {
                _riskDropdown.ClearOptions();
                _riskDropdown.AddOptions(new List<string> { "1", "2", "3", "4", "5" });
                _riskDropdown.value = 0;
            }

            if (_deadlineDropdown != null)
            {
                _deadlineDropdown.ClearOptions();
                _deadlineDropdown.AddOptions(new List<string> { "1", "2", "3", "4", "5" });
                _deadlineDropdown.value = 2;
            }
        }

        private void HandleStateChanged(DayState state)
        {
            var isQuestPhase = state == DayState.QuestDraftPhase || state == DayState.SubmissionPhase;
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(isQuestPhase);
            }

            if (!isQuestPhase)
            {
                ClearSpawnedItems();
                return;
            }

            var isDraftPhase = state == DayState.QuestDraftPhase;

            if (_draftSectionRoot != null)
            {
                _draftSectionRoot.SetActive(isDraftPhase);
            }

            if (_submissionSectionRoot != null)
            {
                _submissionSectionRoot.SetActive(!isDraftPhase);
            }

            RefreshAll();
        }

        private void RefreshAll()
        {
            BuildInfoToggles();
            BuildDraftList();
            RefreshSubmittedText();
            RefreshCreateDraftButtonState();
            RefreshPhaseButtons();
            SetMessage(string.Empty);
        }

        private void BuildInfoToggles()
        {
            UIHelper.DestroyAllChildren(_infoToggleListRoot);
            _spawnedInfoToggles.Clear();
            _selectableInfos.Clear();

            if (_infoToggleListRoot == null || _infoTogglePrefab == null || _infoSystem == null)
            {
                return;
            }

            IReadOnlyList<InfoData> infos = _infoSystem.TodayInfos;
            for (var i = 0; i < infos.Count; i++)
            {
                if (infos[i].IsDiscarded || infos[i].IsUsedInDraft)
                {
                    continue;
                }

                _selectableInfos.Add(infos[i]);

                var toggle = Instantiate(_infoTogglePrefab, _infoToggleListRoot);
                var label = toggle.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    var archivedTag = infos[i].IsArchived ? " [Archived]" : string.Empty;
                    label.text = $"{infos[i].Id} | {infos[i].Title}{archivedTag} | Cred {infos[i].Credibility}";
                }

                toggle.group = _infoToggleGroup;
                toggle.isOn = false;
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(_ => RefreshCreateDraftButtonState());
                _spawnedInfoToggles.Add(toggle);
            }

            RefreshCreateDraftButtonState();
        }

        private void BuildDraftList()
        {
            UIHelper.DestroyAllChildren(_draftListRoot);

            if (_questSystem == null || _draftListRoot == null || _draftItemPrefab == null)
            {
                return;
            }

            var canSubmit = _daySystem != null && _daySystem.CurrentState == DayState.SubmissionPhase;
            var drafts = _questSystem.Drafts;
            for (var i = 0; i < drafts.Count; i++)
            {
                var item = Instantiate(_draftItemPrefab, _draftListRoot);
                item.Init(drafts[i], canSubmit, HandleSubmitDraft);
            }
        }

        private void RefreshSubmittedText()
        {
            if (_submittedText == null || _questSystem == null)
            {
                return;
            }

            var lines = "Submitted Today:";
            var submitted = _questSystem.SubmittedToday;
            if (submitted.Count == 0)
            {
                lines += "\n- none";
            }
            else
            {
                for (var i = 0; i < submitted.Count; i++)
                {
                    var draft = submitted[i].Draft;
                    var draftId = draft != null ? draft.Id : "(null)";
                    lines += $"\n- {draftId}";
                }
            }

            _submittedText.text = lines;
        }

        private void UpdateRewardDefault()
        {
            if (_rewardInput == null)
            {
                return;
            }

            var risk = GetRiskFromDropdown();
            _rewardInput.text = (risk * 100).ToString();
        }

        private void CreateDraft()
        {
            if (_questSystem == null || _infoSystem == null)
            {
                return;
            }

            var selectedInfoIds = CollectSelectedInfoIds();
            if (selectedInfoIds.Count == 0)
            {
                SetMessage("Select at least one info before creating a draft.");
                RefreshCreateDraftButtonState();
                return;
            }

            if (!CanMarkAllSelectedInfos(selectedInfoIds))
            {
                SetMessage("Create failed: one or more infos are unavailable.");
                BuildInfoToggles();
                RefreshCreateDraftButtonState();
                return;
            }

            for (var i = 0; i < selectedInfoIds.Count; i++)
            {
                if (_infoSystem.TryMarkUsed(selectedInfoIds[i]))
                {
                    continue;
                }

                SetMessage("Create failed: one or more infos are unavailable.");
                BuildInfoToggles();
                RefreshCreateDraftButtonState();
                return;
            }

            var reward = ParsePositiveInt(_rewardInput != null ? _rewardInput.text : string.Empty, GetRiskFromDropdown() * 100);
            var draft = _questSystem.CreateDraft(
                GetTypeFromDropdown(),
                selectedInfoIds,
                GetRiskFromDropdown(),
                reward,
                GetDeadlineFromDropdown());

            SetMessage($"Draft created: {draft.Id}");
            BuildInfoToggles();
            RefreshCreateDraftButtonState();
            BuildDraftList();
        }

        private bool CanMarkAllSelectedInfos(IReadOnlyList<string> selectedInfoIds)
        {
            if (_infoSystem == null || selectedInfoIds == null)
            {
                return false;
            }

            IReadOnlyList<InfoData> infos = _infoSystem.TodayInfos;
            for (var i = 0; i < selectedInfoIds.Count; i++)
            {
                var id = selectedInfoIds[i];
                var found = false;

                for (var j = 0; j < infos.Count; j++)
                {
                    var info = infos[j];
                    if (!string.Equals(info.Id, id, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (info.IsArchived || info.IsDiscarded || info.IsUsedInDraft)
                    {
                        return false;
                    }

                    found = true;
                    break;
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        private List<string> CollectSelectedInfoIds()
        {
            var selectedInfoIds = new List<string>();
            if (_infoSystem == null)
            {
                return selectedInfoIds;
            }

            var uniqueIds = new HashSet<string>();
            for (var i = 0; i < _spawnedInfoToggles.Count && i < _selectableInfos.Count; i++)
            {
                if (_spawnedInfoToggles[i] == null || !_spawnedInfoToggles[i].isOn)
                {
                    continue;
                }

                if (uniqueIds.Add(_selectableInfos[i].Id))
                {
                    selectedInfoIds.Add(_selectableInfos[i].Id);
                }
            }

            return selectedInfoIds;
        }

        private void RefreshCreateDraftButtonState()
        {
            if (_createDraftButton == null)
            {
                return;
            }

            _createDraftButton.interactable = CollectSelectedInfoIds().Count > 0;
        }

        private void HandleSubmitDraft(string draftId)
        {
            if (_questSystem == null || _session == null)
            {
                return;
            }

            var ok = _questSystem.TrySubmit(draftId, _session.CurrentDay);
            if (!ok)
            {
                SetMessage("Submit failed: max 4 per day or duplicate/invalid draft.");
            }
            else
            {
                SetMessage($"Submitted: {draftId}");
            }

            BuildDraftList();
            RefreshSubmittedText();
        }

        private QuestTemplateType GetTypeFromDropdown()
        {
            if (_typeDropdown == null)
            {
                return QuestTemplateType.Combat;
            }

            var count = Enum.GetValues(typeof(QuestTemplateType)).Length;
            var idx = Mathf.Clamp(_typeDropdown.value, 0, count - 1);
            return (QuestTemplateType)idx;
        }

        private int GetRiskFromDropdown()
        {
            if (_riskDropdown == null)
            {
                return 1;
            }

            return Mathf.Clamp(_riskDropdown.value + 1, 1, 5);
        }

        private int GetDeadlineFromDropdown()
        {
            if (_deadlineDropdown == null)
            {
                return 1;
            }

            return Mathf.Clamp(_deadlineDropdown.value + 1, 1, 5);
        }

        private static int ParsePositiveInt(string input, int fallback)
        {
            if (int.TryParse(input, out var value) && value > 0)
            {
                return value;
            }

            return fallback;
        }

        private void SetMessage(string message)
        {
            if (_messageText != null)
            {
                _messageText.text = message;
            }
        }

        private void ClearSpawnedItems()
        {
            UIHelper.DestroyAllChildren(_infoToggleListRoot);
            _spawnedInfoToggles.Clear();
            _selectableInfos.Clear();

            UIHelper.DestroyAllChildren(_draftListRoot);
        }

        private void RefreshPhaseButtons()
        {
            var isDraft = _daySystem != null && _daySystem.CurrentState == DayState.QuestDraftPhase;
            var isSubmission = _daySystem != null && _daySystem.CurrentState == DayState.SubmissionPhase;

            if (_toSubmissionButton != null)
            {
                _toSubmissionButton.gameObject.SetActive(isDraft);
            }

            if (_toResolutionButton != null)
            {
                _toResolutionButton.gameObject.SetActive(isSubmission);
            }
        }

        private void HandleToSubmissionClicked()
        {
            if (_daySystem == null)
            {
                return;
            }

            _daySystem.TrySetState(DayState.SubmissionPhase);
        }

        private void HandleToResolutionClicked()
        {
            if (_daySystem == null)
            {
                return;
            }

            _daySystem.TrySetState(DayState.ResolutionPhase);
        }
    }
}
