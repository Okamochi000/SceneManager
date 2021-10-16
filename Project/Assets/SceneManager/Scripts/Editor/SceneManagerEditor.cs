using UnityEditor;

[CustomEditor(typeof(SceneManager))]
public class SceneManagerEditor : CommonEditor
{
    private bool isOpen_ = false;

    /// <summary>
    /// 継承先の更新処理
    /// </summary>
    protected override void OnInspectorGUIBody()
    {
        ShowProperty("isScenePrefab");
        if (GetProperty("isScenePrefab").boolValue) { ShowEnumListLayout(typeof(SceneType), "scenePrefabList", ref isOpen_); }
        else { ShowEnumListLayout(typeof(SceneType), "sceneNameList", ref isOpen_); }
    }
}