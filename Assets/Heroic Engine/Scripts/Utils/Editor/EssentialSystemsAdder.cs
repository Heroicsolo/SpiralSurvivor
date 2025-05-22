using HeroicEngine.Gameplay;
using HeroicEngine.Systems;
using HeroicEngine.Systems.Audio;
using HeroicEngine.Systems.DI;
using HeroicEngine.Systems.Events;
using HeroicEngine.Systems.Gameplay;
using HeroicEngine.Systems.Inputs;
using HeroicEngine.Systems.Localization;
using HeroicEngine.Systems.ScenesManagement;
using HeroicEngine.Systems.UI;
using HeroicEngine.Utils.Pooling;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public static class EssentialSystemsAdder
    {
        private static readonly List<Type> _systemsTypes = new()
        {
            typeof(InjectionManager), typeof(PoolSystem),
            typeof(ScenesLoader), typeof(TimeManager),
            typeof(EventsManager), typeof(LocalizationManager),
            typeof(PlayerProgressionManager), typeof(HittablesManager),
            typeof(UIController), typeof(RandomEventsManager),
            typeof(CurrenciesManager), typeof(SoundsManager),
            typeof(MusicPlayer), typeof(InputManager),
            typeof(CameraController)
        };

        [MenuItem("Tools/HeroicEngine/Add Essential Systems to scene", false, 2)]
        public static void AddEssentialSystems()
        {
            _systemsTypes.ForEach(InstantiateSystem);
        }

        private static GameObject FindPrefabWithComponentType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            var prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

            foreach (var guid in prefabGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab && prefab.GetComponent(typeName) != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        private static void InstantiateSystem(Type systemType)
        {
            if (systemType != null)
            {
                var existingObject = UnityEngine.Object.FindAnyObjectByType(systemType);

                if (existingObject != null)
                {
                    return;
                }

                var prefab = FindPrefabWithComponentType(systemType.Name);

                if (prefab != null)
                {
                    var newObj = UnityEngine.Object.Instantiate(prefab);
                    newObj.name = systemType.Name;
                    return;
                }

                // Create a new GameObject and add the script as a component
                var newObject = new GameObject(systemType.Name);
                newObject.AddComponent(systemType);
                AssetDatabase.Refresh();
            }
        }
    }
}
