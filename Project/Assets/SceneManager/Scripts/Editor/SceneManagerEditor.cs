using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(SceneManager))]
    public class SceneManagerEditor : CommonEditor
    {
        private bool isOpen_ = false;

        /// <summary>
        /// åpè≥êÊÇÃçXêVèàóù
        /// </summary>
        protected override void OnInspectorGUIBody()
        {
            ShowProperty("isScenePrefab");
            if (GetProperty("isScenePrefab").boolValue) { ShowEnumListLayout(typeof(SceneType), "scenePrefabList", ref isOpen_); }
            else { ShowEnumListLayout(typeof(SceneType), "sceneNameList", ref isOpen_); }
        }
    }
}