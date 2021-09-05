using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーンタイプ
/// </summary>
public enum SceneType
{
    None,
    Scene1,
    Scene2,
    Scene3,
}

/// <summary>
/// シーン切替
/// </summary>
public class SceneManager : MonoBehaviourSingleton<SceneManager>
{
    /// <summary>
    /// シーン読み込み状態
    /// </summary>
    private enum Phase
    {
        None,
        ScreenOutWait,
        DestroyWait,
        LoadSceneWait,
        InitializeWait,
        ScreenInWait,
    }

    public bool IsInitialized { get; private set; } = false;
    public bool IsChanging { get; private set; } = false;
    public SceneController ActiveScene { get; private set; } = null;
    public TransitionEffectType EffectType { get; set; } = TransitionEffectType.None;
    public SceneType CurrentSceneType { get; private set; } = SceneType.None;
    public SceneType NextSceneType { get; private set; } = SceneType.None;

    [SerializeField] private bool isScenePrefab = false;
    [SerializeField] private List<string> sceneNameList = new List<string>();
    [SerializeField] private List<GameObject> scenePrefabList = new List<GameObject>();

    private Phase phase_ = Phase.None;
    private System.Object passData_ = null;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);

        // 起動時のシーンからシーンコントローラーを探す
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            ActiveScene = rootObject.GetComponent<SceneController>();
            if (ActiveScene != null) { break; }
        }

        SceneType nextSceneType = SceneType.None;
        if (isScenePrefab)
        {
            // プレハブ名が同じシーンを探す
            foreach (SceneType sceneType in Enum.GetValues(typeof(SceneType)))
            {
                GameObject prefab = scenePrefabList[(int)sceneType];
                if (prefab != null && prefab.name == ActiveScene.name)
                {
                    nextSceneType = sceneType;
                    break;
                }
            }
        }
        else
        {
            // シーン名が同じシーンを探す
            foreach (SceneType sceneType in Enum.GetValues(typeof(SceneType)))
            {
                if (sceneNameList[(int)sceneType] == scene.name)
                {
                    nextSceneType = sceneType;
                    break;
                }
            }
        }

        if (nextSceneType != SceneType.None)
        {
            // シーン初期化
            SetNextScene(nextSceneType, TransitionEffectType.None);
        }
        else
        {
            // シーン読み込み失敗
            Debug.Log("SceneManagerに登録されていないシーンが再生されています");
        }
    }

    /// <summary>
    /// 切替先シーン設定
    /// </summary>
    /// <param name="sceneType"></param>
    /// <param name="transitionEffectType"></param>
    /// <returns></returns>
    public bool SetNextScene(SceneType sceneType, TransitionEffectType transitionEffectType = TransitionEffectType.Fade)
    {
        // 切替中に切替先を変更させない
        if (IsChanging) { return false; }

        // シーン指定なしは無効
        if (sceneType == SceneType.None) { return false; }

        // シーン遷移開始
        StartCoroutine(ChangeScene(sceneType, transitionEffectType));

        return true;
    }

    /// <summary>
    /// 別シーンに渡すデータを設定する
    /// </summary>
    public void SetPassData(System.Object data)
    {
        passData_ = data;
    }

    /// <summary>
    /// 別シーンが保存したデータを取得する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetPassData<T>()
    {
        if (passData_ != null && (passData_ is T))
        {
            return (T)passData_;
        }

        return default(T);
    }

    /// <summary>
    /// シーンプレハブを取得する
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private GameObject GetPrefab(SceneType type)
    {
        if ((int)type < scenePrefabList.Count)
        {
            return scenePrefabList[(int)type];
        }

        return null;
    }

    /// <summary>
    /// シーン名を取得する
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string GetSceneName(SceneType type)
    {
        if ((int)type < sceneNameList.Count)
        {
            return sceneNameList[(int)type];
        }

        return "";
    }

    /// <summary>
    /// シーン読み込み
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private IEnumerator LoadScene(SceneType sceneType)
    {
        // 次のシーンを読み込む
        yield return null;
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GetSceneName(sceneType));
        while (!asyncOperation.isDone) { yield return null; }

        // 次のシーンのSceneControllerを初期化する
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            ActiveScene = rootObject.GetComponent<SceneController>();
            if (ActiveScene != null) { break; }
        }
    }

    /// <summary>
    /// シーンを切り替える
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeScene(SceneType nextSceneType, TransitionEffectType transitionEffectType)
    {
        // 切替開始
        IsChanging = true;

        EffectType = transitionEffectType;
        NextSceneType = nextSceneType;

        if (CurrentSceneType != SceneType.None)
        {
            // スクリーンアウト
            phase_ = Phase.ScreenOutWait;
            TransitionManager.Instance.StartTransition(transitionEffectType, TransitionState.ScreenOut, () => { phase_ = Phase.DestroyWait; });
            if (ActiveScene != null) { ActiveScene.StartedSceneChange(); }
            while (phase_ == Phase.ScreenOutWait) { yield return null; }

            // シーン削除処理待ち
            if (ActiveScene != null)
            {
                // 削除待ち
                ActiveScene.Destory();
                while (!ActiveScene.IsDestroyed) { yield return null; }
                // 削除
                if (isScenePrefab) { Destroy(ActiveScene.gameObject); }
                ActiveScene = null;
            }

            // 次のシーンを読み込む
            phase_ = Phase.LoadSceneWait;
            if (isScenePrefab)
            {
                // プレハブ読み込み
                GameObject resources = GetPrefab(NextSceneType);
                if (resources != null)
                {
                    GameObject scene = GameObject.Instantiate(resources);
                    ActiveScene = scene.GetComponent<SceneController>();
                }
                else
                {
                    Debug.LogError(String.Format("{0}シーンが存在しません", NextSceneType));
                    yield break;
                }
            }
            else
            {
                // シーン読み込み
                yield return LoadScene(NextSceneType);
            }
        }

        CurrentSceneType = NextSceneType;
        NextSceneType = SceneType.None;

        // 初期化待ち
        if (ActiveScene != null)
        {
            phase_ = Phase.InitializeWait;
            ActiveScene.Initialize();
            while (!ActiveScene.IsInitialized) { yield return null; }
        }

        // スクリーンイン
        phase_ = Phase.ScreenInWait;
        TransitionManager.Instance.StartTransition(EffectType, TransitionState.ScreenIn, () => { phase_ = Phase.None; });
        while (phase_ == Phase.ScreenInWait) { yield return null; }

        // スクリーンイン完了
        if (ActiveScene != null) { ActiveScene.FinishedSceneChange(); }

        // 切替完了
        IsChanging = false;
    }
}