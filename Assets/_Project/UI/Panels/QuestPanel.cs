using System;
using System.Collections.Generic;
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

        private readonly List<Toggle> _spawnedInfoToggles = new List<Toggle>();
        private readonly List<InfoData> _selectableInfos = new List<InfoData>();
        private readonly List<QuestDraftItemWidget> _spawnedDraftItems = new List<QuestDraftItemWidget>();

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
            SetMessage(string.Empty);
        }

        private void BuildInfoToggles()
        {
            for (var i = 0; i < _spawnedInfoToggles.Count; i++)
            {
                if (_spawnedInfoToggles[i] != null)
                {
                    Destroy(_spawnedInfoToggles[i].gameObject);
                }
            }

            _spawnedInfoToggles.Clear();
            _selectableInfos.Clear();

            if (_infoToggleListRoot == null || _infoTogglePrefab == null || _infoSystem == null)
            {
                return;
            }

            _infoToggleGroup = _infoToggleListRoot.GetComponent<ToggleGroup>();
            if (_infoToggleGroup == null)
            {
                _infoToggleGroup = _infoToggleListRoot.gameObject.AddComponent<ToggleGroup>();
            }

            _infoToggleGroup.allowSwitchOff = true;

            IReadOnlyList<InfoData> infos = _infoSystem.TodayInfos;
            for (var i = 0; i < infos.Count; i++)
            {
                if (infos[i].IsDiscarded)
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
            for (var i = 0; i < _spawnedDraftItems.Count; i++)
            {
                if (_spawnedDraftItems[i] != null)
                {
                    Destroy(_spawnedDraftItems[i].gameObject);
                }
            }

            _spawnedDraftItems.Clear();

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
                _spawnedDraftItems.Add(item);
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

            var reward = ParsePositiveInt(_rewardInput != null ? _rewardInput.text : string.Empty, GetRiskFromDropdown() * 100);
            var draft = _questSystem.CreateDraft(
                GetTypeFromDropdown(),
                selectedInfoIds,
                GetRiskFromDropdown(),
                reward,
                GetDeadlineFromDropdown());

            SetMessage($"Draft created: {draft.Id}");
            RefreshCreateDraftButtonState();
            BuildDraftList();
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
    }
}
