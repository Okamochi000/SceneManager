using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// エディターの共通処理
/// </summary>
public abstract class CommonEditor : Editor
{
    protected bool isChanged_ = false;

    private Dictionary<string, SerializedProperty> propertyDictionary_ = new Dictionary<string, SerializedProperty>();

    #region Unityメソッド

    /// <summary>
    /// 初期設定
    /// </summary>
    public virtual void OnEnable()
    {
        // 初期化
        isChanged_ = false;

        // 保持しているプロパティを全て設定
        propertyDictionary_.Clear();
        SetProperty("m_Script");
        SetPropertyAll();

        // 継承先の初期化処理
        OnInitialize();
    }

    /// <summary>
    /// インスペクター更新
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (IsDefaultInspector())
        {
            // 全てのインスペクターを表示
            base.OnInspectorGUI();
        }
        else
        {
            // スクリプト参照を表示する
            using (new EditorGUI.DisabledScope(true)) { ShowProperty("m_Script"); }
        }

        // アプリ実行中に非表示にするか
        if (IsPlayingDisabled()) { EditorGUI.BeginDisabledGroup(Application.isPlaying); }

        // 更新チェック開始
        BeginChangeCheck();

        // ボックス囲み開始
        if (IsBoxCover()) { EditorGUILayout.BeginVertical(GUI.skin.box); }

        // ボディに設定する内容
        OnInspectorGUIBody();

        // ボックス囲み終了　
        if (IsBoxCover()) { EditorGUILayout.EndVertical(); }

        // 更新チェック終了
        EndChangeCheck();

        // アプリ実行中に非表示にするか
        if (IsPlayingDisabled()) { EditorGUI.BeginDisabledGroup(Application.isPlaying); }
    }

    #endregion

    #region 継承メソッド

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void OnInitialize() { }

    /// <summary>
    /// アプリ実行中は操作不可にするか
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsPlayingDisabled()
    {
        return true;
    }

    /// <summary>
    /// デフォルトのInspector内容を表示するか
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsDefaultInspector()
    {
        return false;
    }

    /// <summary>
    /// Inspectorをボックスで囲むか
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsBoxCover()
    {
        return false;
    }

    /// <summary>
    /// 継承先の更新処理
    /// </summary>
    protected abstract void OnInspectorGUIBody();

    #endregion

    #region インスタンス

    /// <summary>
    /// インスタンスの取得
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T Instance<T>() where T : MonoBehaviour
    {
        return (T)target;
    }

    #endregion

    #region プロパティ

    /// <summary>
    /// プロパティを設定する
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected SerializedProperty SetProperty(string propertyName)
    {
        propertyDictionary_[propertyName] = serializedObject.FindProperty(propertyName);
        return propertyDictionary_[propertyName];
    }

    /// <summary>
    /// プロパティを設定する
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected void SetProperty(string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            SetProperty(propertyName);
        }
    }

    /// <summary>
    /// プロパティを設定する
    /// </summary>
    protected void SetPropertyAll()
    {
        var iter = serializedObject.GetIterator();
        iter.NextVisible(true);
        while (iter.NextVisible(false))
        {
            string propertyName = iter.name;
            propertyDictionary_[propertyName] = serializedObject.FindProperty(propertyName);
        }
    }

    /// <summary>
    /// プロパティ内プロパティを設定する
    /// </summary>
    /// <param name="targetProperty"></param>
    /// <param name="key"></param>
    protected void SetRelativePropertyAll(string propertyName, string[] relativeNames, string key)
    {
        SerializedProperty relativeProperty = GetProperty(propertyName);
        foreach (string relativeName in relativeNames)
        {
            string dicKey = (key + relativeName);
            propertyDictionary_[dicKey] = relativeProperty.FindPropertyRelative(relativeName);
        }
    }

    /// <summary>
    /// プロパティを取得する
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected SerializedProperty GetProperty(string propertyName)
    {
        if (propertyDictionary_.ContainsKey(propertyName)) { return propertyDictionary_[propertyName]; }
        return null;
    }

    /// <summary>
    /// プロパティを表示する
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected bool ShowProperty(string propertyName)
    {
        bool result = false;
        SerializedProperty property = GetProperty(propertyName);
        if (property != null)
        {
            if (property.isArray) { EditorGUI.indentLevel++; }
            result = EditorGUILayout.PropertyField(property, true);
            if (property.isArray) { EditorGUI.indentLevel--; }
        }
        return result;
    }

    /// <summary>
    /// プロパティを表示する
    /// </summary>
    /// <param name="propertyNames"></param>
    /// <returns></returns>
    protected void ShowProperty(string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            ShowProperty(propertyName);
        }
    }

    /// <summary>
    /// プロパティをラベル指定で表示する
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    protected bool ShowProperty(string propertyName, string label)
    {
        return ShowProperty(GetProperty(propertyName), label);
    }

    /// <summary>
    /// プロパティをラベル指定で表示する
    /// </summary>
    /// <param name="property"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    protected bool ShowProperty(SerializedProperty property, string label)
    {
        bool result = false;
        if (property != null)
        {
            GUIContent content = new GUIContent();
            content.text = label;
            if (property.isArray) { EditorGUI.indentLevel++; }
            result = EditorGUILayout.PropertyField(property, content, true);
            if (property.isArray) { EditorGUI.indentLevel--; }
        }
        return result;
    }

    /// <summary>
    /// 全てのプロパティを表示する
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <param name="key"></param>
    protected void ShowPropertyAll(SerializedObject serializedObject, string key)
    {
        var iter = serializedObject.GetIterator();
        iter.NextVisible(true);
        while (iter.NextVisible(false))
        {
            string dicKey = (key + iter.name);
            ShowProperty(dicKey);
        }
    }

    /// <summary>
    /// オブジェクトリストのプロパティ設定
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="values"></param>
    protected void SetObjectListProperty(string propertyName, UnityEngine.Object[] values)
    {
        SerializedProperty property = GetProperty(propertyName);
        int valueCount = 0;
        if (values != null) { valueCount = values.Length; }
        property.arraySize = valueCount;
        for (int i = 0; i < valueCount; i++)
        {
            SerializedProperty indexProperty = property.GetArrayElementAtIndex(i);
            UnityEngine.Object value = values[i];
            indexProperty.objectReferenceValue = value;
        }
    }

    #endregion

    #region 汎用レイアウト

    /// <summary>
    /// 汎用レイアウト
    /// </summary>
    /// <param name="title"></param>
    /// <param name="propertys"></param>
    protected void ShowGeneralBoxLayout(string title, string[] propertys)
    {
        // 縦並び開始
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // タイトル表示
        EditorGUILayout.LabelField(title);

        // 要素表示
        ShowProperty(propertys);

        // 縦並び終了
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 汎用開閉レイアウト
    /// </summary>
    /// <param name="title"></param>
    /// <param name="propertys"></param>
    /// <param name="isOpen"></param>
    protected void ShowGeneralBoxLayout(string title, string[] propertys, ref bool isOpen)
    {
        // 縦並び開始
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // 折りたたみ表示
        ShowFoldout(ref isOpen, title, () => { ShowProperty(propertys); });

        // 縦並び終了
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// bool型のチェックによるレイアウト
    /// </summary>
    protected void ShowCheckFoldoutLayout(string titlePropety, string[] bodyPropertys, bool isHide)
    {
        // タイトル表示
        bool isOpen = false;
        SerializedProperty property = GetProperty(titlePropety);
        if (property != null)
        {
            EditorGUILayout.PropertyField(property, true);
            isOpen = property.boolValue;
        }

        // 開閉中、または開いているときも要素を表示する場合
        if (isOpen || !isHide)
        {
            // インデント
            EditorGUI.indentLevel++;

            // 操作不可設定
            bool isDisabled = false;
            if (!isOpen && isHide) { isDisabled = true; }
            EditorGUI.BeginDisabledGroup(isDisabled);

            // 要素表示
            ShowProperty(bodyPropertys);

            // 操作不可設定解除
            EditorGUI.BeginDisabledGroup(isDisabled);

            // インデント解除
            EditorGUI.indentLevel--;
        }
    }

    /// <summary>
    /// リストの要素をEnumの個数分表示する
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <param name="isOpen"></param>
    protected void ShowEnumListLayout(Type type, string propertyName, ref bool isOpen)
    {
        // タイトル表示
        SerializedProperty property = GetProperty(propertyName);

        // Enumの数に合わせる
        string[] enumNames = Enum.GetNames(type);
        property.arraySize = enumNames.Length;

        // 折りたたみ表示
        ShowFoldout(ref isOpen, property.displayName, () => {
            // タイトル表示
            bool indent = true;
            if (property.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.ObjectReference) { indent = false; }
            if (indent) { EditorGUI.indentLevel--; }
            for (int i = 0; i < enumNames.Length; i++)
            {
                string label = enumNames[i];
                SerializedProperty indexProperty = property.GetArrayElementAtIndex(i);
                ShowProperty(indexProperty, label);
            }
            if (indent) { EditorGUI.indentLevel++; }
        });
    }

    /// <summary>
    /// 折りたたみ表示
    /// </summary>
    /// <param name="title"></param>
    /// <param name="isOpen"></param>
    /// <param name="openAction"></param>
    protected void ShowFoldout(ref bool isOpen, string title, Action openAction)
    {
        // インデント開始
        EditorGUI.indentLevel++;

        // 折りたたみ表示(折りたたみの切り替えで操作更新をしない)
        isChanged_ |= EditorGUI.EndChangeCheck();
        isOpen = EditorGUILayout.Foldout(isOpen, title);
        EditorGUI.BeginChangeCheck();

        // 開かれているときの処理
        if (isOpen && openAction != null) { openAction(); }

        // インデント終了
        EditorGUI.indentLevel--;
    }

    #endregion

    #region 更新チェック

    /// <summary>
    /// 更新チェック開始
    /// </summary>
    private void BeginChangeCheck()
    {
        // 更新チェック開始
        EditorGUI.BeginChangeCheck();

        // シリアライズオブジェクト更新
        serializedObject.Update();
    }

    /// <summary>
    /// 更新チェック終了
    /// </summary>
    private void EndChangeCheck()
    {
        // シリアライズオブジェクト更新
        serializedObject.ApplyModifiedProperties();

        // アプリ起動中は実行しない
        if (Application.isPlaying) { return; }

        // エディター更新がない場合は実行しない
        isChanged_ |= EditorGUI.EndChangeCheck();
        if (!isChanged_) { return; }

        // 更新反映
        isChanged_ = false;
        MonoBehaviour instance = Instance<MonoBehaviour>();
        string path = instance.gameObject.scene.path;
        if (path == null || !path.Contains(".unity")) { EditorUtility.SetDirty(instance); }
        else { EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); }
    }

    #endregion
}