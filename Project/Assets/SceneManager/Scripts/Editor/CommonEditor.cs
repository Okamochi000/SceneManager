using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// �G�f�B�^�[�̋��ʏ���
/// </summary>
public abstract class CommonEditor : Editor
{
    protected bool isChanged_ = false;

    private Dictionary<string, SerializedProperty> propertyDictionary_ = new Dictionary<string, SerializedProperty>();

    #region Unity���\�b�h

    /// <summary>
    /// �����ݒ�
    /// </summary>
    public virtual void OnEnable()
    {
        // ������
        isChanged_ = false;

        // �ێ����Ă���v���p�e�B��S�Đݒ�
        propertyDictionary_.Clear();
        SetProperty("m_Script");
        SetPropertyAll();

        // �p����̏���������
        OnInitialize();
    }

    /// <summary>
    /// �C���X�y�N�^�[�X�V
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (IsDefaultInspector())
        {
            // �S�ẴC���X�y�N�^�[��\��
            base.OnInspectorGUI();
        }
        else
        {
            // �X�N���v�g�Q�Ƃ�\������
            using (new EditorGUI.DisabledScope(true)) { ShowProperty("m_Script"); }
        }

        // �A�v�����s���ɔ�\���ɂ��邩
        if (IsPlayingDisabled()) { EditorGUI.BeginDisabledGroup(Application.isPlaying); }

        // �X�V�`�F�b�N�J�n
        BeginChangeCheck();

        // �{�b�N�X�͂݊J�n
        if (IsBoxCover()) { EditorGUILayout.BeginVertical(GUI.skin.box); }

        // �{�f�B�ɐݒ肷����e
        OnInspectorGUIBody();

        // �{�b�N�X�͂ݏI���@
        if (IsBoxCover()) { EditorGUILayout.EndVertical(); }

        // �X�V�`�F�b�N�I��
        EndChangeCheck();

        // �A�v�����s���ɔ�\���ɂ��邩
        if (IsPlayingDisabled()) { EditorGUI.BeginDisabledGroup(Application.isPlaying); }
    }

    #endregion

    #region �p�����\�b�h

    /// <summary>
    /// ������
    /// </summary>
    protected virtual void OnInitialize() { }

    /// <summary>
    /// �A�v�����s���͑���s�ɂ��邩
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsPlayingDisabled()
    {
        return true;
    }

    /// <summary>
    /// �f�t�H���g��Inspector���e��\�����邩
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsDefaultInspector()
    {
        return false;
    }

    /// <summary>
    /// Inspector���{�b�N�X�ň͂ނ�
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsBoxCover()
    {
        return false;
    }

    /// <summary>
    /// �p����̍X�V����
    /// </summary>
    protected abstract void OnInspectorGUIBody();

    #endregion

    #region �C���X�^���X

    /// <summary>
    /// �C���X�^���X�̎擾
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T Instance<T>() where T : MonoBehaviour
    {
        return (T)target;
    }

    #endregion

    #region �v���p�e�B

    /// <summary>
    /// �v���p�e�B��ݒ肷��
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected SerializedProperty SetProperty(string propertyName)
    {
        propertyDictionary_[propertyName] = serializedObject.FindProperty(propertyName);
        return propertyDictionary_[propertyName];
    }

    /// <summary>
    /// �v���p�e�B��ݒ肷��
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
    /// �v���p�e�B��ݒ肷��
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
    /// �v���p�e�B���v���p�e�B��ݒ肷��
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
    /// �v���p�e�B���擾����
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected SerializedProperty GetProperty(string propertyName)
    {
        if (propertyDictionary_.ContainsKey(propertyName)) { return propertyDictionary_[propertyName]; }
        return null;
    }

    /// <summary>
    /// �v���p�e�B��\������
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
    /// �v���p�e�B��\������
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
    /// �v���p�e�B�����x���w��ŕ\������
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    protected bool ShowProperty(string propertyName, string label)
    {
        return ShowProperty(GetProperty(propertyName), label);
    }

    /// <summary>
    /// �v���p�e�B�����x���w��ŕ\������
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
    /// �S�Ẵv���p�e�B��\������
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
    /// �I�u�W�F�N�g���X�g�̃v���p�e�B�ݒ�
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

    #region �ėp���C�A�E�g

    /// <summary>
    /// �ėp���C�A�E�g
    /// </summary>
    /// <param name="title"></param>
    /// <param name="propertys"></param>
    protected void ShowGeneralBoxLayout(string title, string[] propertys)
    {
        // �c���ъJ�n
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // �^�C�g���\��
        EditorGUILayout.LabelField(title);

        // �v�f�\��
        ShowProperty(propertys);

        // �c���яI��
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// �ėp�J���C�A�E�g
    /// </summary>
    /// <param name="title"></param>
    /// <param name="propertys"></param>
    /// <param name="isOpen"></param>
    protected void ShowGeneralBoxLayout(string title, string[] propertys, ref bool isOpen)
    {
        // �c���ъJ�n
        EditorGUILayout.BeginVertical(GUI.skin.box);

        // �܂肽���ݕ\��
        ShowFoldout(ref isOpen, title, () => { ShowProperty(propertys); });

        // �c���яI��
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// bool�^�̃`�F�b�N�ɂ�郌�C�A�E�g
    /// </summary>
    protected void ShowCheckFoldoutLayout(string titlePropety, string[] bodyPropertys, bool isHide)
    {
        // �^�C�g���\��
        bool isOpen = false;
        SerializedProperty property = GetProperty(titlePropety);
        if (property != null)
        {
            EditorGUILayout.PropertyField(property, true);
            isOpen = property.boolValue;
        }

        // �J���A�܂��͊J���Ă���Ƃ����v�f��\������ꍇ
        if (isOpen || !isHide)
        {
            // �C���f���g
            EditorGUI.indentLevel++;

            // ����s�ݒ�
            bool isDisabled = false;
            if (!isOpen && isHide) { isDisabled = true; }
            EditorGUI.BeginDisabledGroup(isDisabled);

            // �v�f�\��
            ShowProperty(bodyPropertys);

            // ����s�ݒ����
            EditorGUI.BeginDisabledGroup(isDisabled);

            // �C���f���g����
            EditorGUI.indentLevel--;
        }
    }

    /// <summary>
    /// ���X�g�̗v�f��Enum�̌����\������
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <param name="isOpen"></param>
    protected void ShowEnumListLayout(Type type, string propertyName, ref bool isOpen)
    {
        // �^�C�g���\��
        SerializedProperty property = GetProperty(propertyName);

        // Enum�̐��ɍ��킹��
        string[] enumNames = Enum.GetNames(type);
        property.arraySize = enumNames.Length;

        // �܂肽���ݕ\��
        ShowFoldout(ref isOpen, property.displayName, () => {
            // �^�C�g���\��
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
    /// �܂肽���ݕ\��
    /// </summary>
    /// <param name="title"></param>
    /// <param name="isOpen"></param>
    /// <param name="openAction"></param>
    protected void ShowFoldout(ref bool isOpen, string title, Action openAction)
    {
        // �C���f���g�J�n
        EditorGUI.indentLevel++;

        // �܂肽���ݕ\��(�܂肽���݂̐؂�ւ��ő���X�V�����Ȃ�)
        isChanged_ |= EditorGUI.EndChangeCheck();
        isOpen = EditorGUILayout.Foldout(isOpen, title);
        EditorGUI.BeginChangeCheck();

        // �J����Ă���Ƃ��̏���
        if (isOpen && openAction != null) { openAction(); }

        // �C���f���g�I��
        EditorGUI.indentLevel--;
    }

    #endregion

    #region �X�V�`�F�b�N

    /// <summary>
    /// �X�V�`�F�b�N�J�n
    /// </summary>
    private void BeginChangeCheck()
    {
        // �X�V�`�F�b�N�J�n
        EditorGUI.BeginChangeCheck();

        // �V���A���C�Y�I�u�W�F�N�g�X�V
        serializedObject.Update();
    }

    /// <summary>
    /// �X�V�`�F�b�N�I��
    /// </summary>
    private void EndChangeCheck()
    {
        // �V���A���C�Y�I�u�W�F�N�g�X�V
        serializedObject.ApplyModifiedProperties();

        // �A�v���N�����͎��s���Ȃ�
        if (Application.isPlaying) { return; }

        // �G�f�B�^�[�X�V���Ȃ��ꍇ�͎��s���Ȃ�
        isChanged_ |= EditorGUI.EndChangeCheck();
        if (!isChanged_) { return; }

        // �X�V���f
        isChanged_ = false;
        MonoBehaviour instance = Instance<MonoBehaviour>();
        string path = instance.gameObject.scene.path;
        if (path == null || !path.Contains(".unity")) { EditorUtility.SetDirty(instance); }
        else { EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); }
    }

    #endregion
}