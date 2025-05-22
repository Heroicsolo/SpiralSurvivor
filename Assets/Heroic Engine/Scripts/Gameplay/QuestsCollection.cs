using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Utils.Data;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Gameplay
{
    public sealed class QuestsCollection : SOCollection<QuestInfo>
    {
        [SerializeField] private List<QuestInfo> initialQuests = new List<QuestInfo>();

        public List<QuestInfo> QuestInfos => Items;
        public List<QuestInfo> InitialQuests => initialQuests;

        public void RegisterQuest(QuestInfo questInfo, bool isInitial = false)
        {
            SetInitial(questInfo, isInitial);
            RegisterItem(questInfo);
        }

        public void SetInitial(QuestInfo questInfo, bool isInitial = false)
        {
            if (isInitial && !initialQuests.Contains(questInfo))
            {
                initialQuests.Add(questInfo);
            }
        }
    }
}