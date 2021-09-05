using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// テスト実行
/// </summary>
public class TestRun : MonoBehaviour
{
    [SerializeField] private Button button = null;

    // Start is called before the first frame update
    void Start()
    {
        if (button == null) { return; }

        List<SceneType> sceneTypeList = new List<SceneType>();
        sceneTypeList.AddRange((SceneType[])Enum.GetValues(typeof(SceneType)));
        sceneTypeList.Remove(SceneType.None);
        foreach (SceneType sceneType in sceneTypeList)
        {
            Button copy = GameObject.Instantiate(button, button.transform.parent);
            copy.onClick.AddListener(() => { SceneManager.Instance.SetNextScene(sceneType); });
            copy.GetComponentInChildren<Text>().text = sceneType.ToString();
        }
        button.gameObject.SetActive(false);
    }
}
