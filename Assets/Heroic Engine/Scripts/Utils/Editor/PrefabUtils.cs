using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroicEngine.Utils.Editor
{
    public static class PrefabUtils
    {
        public static void Place(GameObject prefab, Vector3 pos)
        {
            prefab.transform.position = pos;
            StageUtility.PlaceGameObjectInCurrentStage(prefab);
            GameObjectUtility.EnsureUniqueNameForSibling(prefab);
            Undo.RegisterCreatedObjectUndo(prefab, $"Create GO {prefab.name}");
            Selection.activeGameObject = prefab;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
