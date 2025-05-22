using HeroicEngine.Enums;
using HeroicEngine.Gameplay;
using HeroicEngine.Systems.Gameplay;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HeroicEngine.Utils.Data;

namespace HeroicEngine.Utils.Editor
{
    public sealed class QuestsMenu : EditorWindow
    {
        private const string TASK_TYPES_PATH = "Assets/Heroic Engine/Scripts/Enums/";

        private const string QUESTS_COLLECTION_PATH = "Assets/Heroic Engine/Scriptables/Quests/QuestsCollection.asset";
        private const string TASK_TYPES_FILE_NAME = "QuestTaskType";

        private GUIStyle _italicStyle;
        
        private QuestsCollection _questsCollection;
        private string _questTitle;
        private string _questDesc;
        private Sprite _questSprite;
        private string _statusText;
        private QuestInfo[] _nextQuests = Array.Empty<QuestInfo>();
        private QuestTask[] _tasks = Array.Empty<QuestTask>();
        private int _expReward;
        private bool _isInitial;
        private Vector2 _scrollPosition;
        private Vector2 _scrollPositionTasks;
        private string _newTaskType;
        private List<string> _taskTypes = new();

        [MenuItem("Tools/HeroicEngine/Create Quest")]
        public static void ShowWindow()
        {
            GetWindow<QuestsMenu>("Quest Creator");
        }

        private bool IsTaskTypeNameValid()
        {
            return !string.IsNullOrEmpty(_newTaskType) && char.IsLetter(_newTaskType[0]) && !_newTaskType.Contains(" ");
        }

        private void OnGUI()
        {
            _questsCollection ??= AssetDatabase.LoadAssetAtPath<QuestsCollection>(QUESTS_COLLECTION_PATH);
            _taskTypes = new List<string>(Enum.GetNames(typeof(QuestTaskType)));
            
            _italicStyle ??= new GUIStyle
            {
                fontStyle = FontStyle.Italic, richText = true
            };

            EditorGUILayout.LabelField("Register new quest");
            EditorGUILayout.LabelField("Quest title:");
            _questTitle = EditorGUILayout.TextField(_questTitle);
            EditorGUILayout.LabelField("Quest desc:");
            _questDesc = EditorGUILayout.TextField(_questDesc);

            _questSprite = (Sprite)EditorGUILayout.ObjectField("Quest icon", _questSprite, typeof(Sprite), false);

            EditorGUILayout.LabelField("Quest Tasks");

            _scrollPositionTasks = EditorGUILayout.BeginScrollView(_scrollPositionTasks);

            // Display each prefab in the list
            for (var i = 0; i < _tasks.Length; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.LabelField("Task type:");
                var taskTypes = new List<string>(Enum.GetNames(typeof(QuestTaskType)));
                var mainTaskType = (QuestTaskType)EditorGUILayout.Popup((int)_tasks[i].TaskType, taskTypes.ToArray());

                var mainTaskAmount = EditorGUILayout.IntField("Task needed amount", _tasks[i].NeededAmount);
                mainTaskAmount = Mathf.Clamp(mainTaskAmount, 1, mainTaskAmount);

                // Display the prefab field
                _tasks[i] = new QuestTask
                {
                    TaskType = mainTaskType, NeededAmount = mainTaskAmount
                };

                // Add a remove button
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    ArrayUtility.RemoveAt(ref _tasks, i);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Quest Task"))
            {
                ArrayUtility.Add(ref _tasks, default);
            }

            if (GUILayout.Button("Clear Quest Tasks"))
            {
                _tasks = Array.Empty<QuestTask>();
            }

            _newTaskType = EditorGUILayout.TextField("New task type", _newTaskType);

            if (GUILayout.Button("Register new Task type"))
            {
                if (!IsTaskTypeNameValid())
                {
                    _statusText = "Invalid task type name!".ToColorizedString(Color.red);
                }
                else if (_taskTypes.Contains(_newTaskType))
                {
                    _statusText = "This task type already exists!".ToColorizedString(Color.yellow);
                }
                else
                {
                    _taskTypes.Add(_newTaskType);
                    EnumUtils.WriteToEnum(TASK_TYPES_PATH, TASK_TYPES_FILE_NAME, _taskTypes);
                    _newTaskType = "";
                }
            }

            EditorGUILayout.Space(20);

            _expReward = EditorGUILayout.IntField("Experience reward", _expReward);
            _expReward = Mathf.Clamp(_expReward, 0, _expReward);

            _isInitial = EditorGUILayout.Toggle("Is available from start", _isInitial);

            EditorGUILayout.LabelField("Next Quests");

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Display each prefab in the list
            for (var i = 0; i < _nextQuests.Length; i++)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                // Display the prefab field
                _nextQuests[i] = (QuestInfo)EditorGUILayout.ObjectField($"Quest {i + 1}", _nextQuests[i], typeof(QuestInfo), false);

                // Add a remove button
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    ArrayUtility.RemoveAt(ref _nextQuests, i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Next Quest"))
            {
                ArrayUtility.Add(ref _nextQuests, null);
            }

            if (GUILayout.Button("Clear Next Quests"))
            {
                _nextQuests = Array.Empty<QuestInfo>();
            }

            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Register"))
            {
                if (!string.IsNullOrEmpty(_questTitle))
                {
                    if (!File.Exists($"{Application.dataPath}/Heroic Engine/Scriptables/Quests/{_questTitle}.asset"))
                    {
                        if (_questsCollection == null)
                        {
                            var collection = CreateInstance<QuestsCollection>();
                            var questInfo = collection.CreateItem($"Assets/Heroic Engine/Scriptables/Quests/{_questTitle}.asset", _questTitle, _questDesc, _questSprite, _expReward,
                                _tasks, _nextQuests);
                            collection.SetInitial(questInfo, _isInitial);
                            _questsCollection = collection;
                            AssetDatabase.CreateAsset(collection, QUESTS_COLLECTION_PATH);
                            AssetDatabase.SaveAssets();
                        }
                        else
                        {
                            var questInfo = _questsCollection.CreateItem($"Assets/Heroic Engine/Scriptables/Quests/{_questTitle}.asset", _questTitle, _questDesc, _questSprite, _expReward,
                                _tasks, _nextQuests);
                            _questsCollection.SetInitial(questInfo, _isInitial);
                        }

                        _statusText = "Quest registered.".ToColorizedString(Color.green);
                    }
                    else
                    {
                        _statusText = "This quest was already registered!".ToColorizedString(Color.red);
                    }
                }
            }

            EditorGUILayout.LabelField(_statusText, _italicStyle);
        }
    }
}
