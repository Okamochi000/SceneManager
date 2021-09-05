using System.Collections;
using UnityEngine;

/// <summary>
/// 初期設定
/// </summary>
public class Boot : MonoBehaviour
{
    public static bool IsInitialize { get; private set; } = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // 共通データ
        if (!SceneManager.IsExist())
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (GameObject gameObject in scene.GetRootGameObjects())
            {
                if (gameObject.GetComponent<Boot>() != null)
                {
                    GameObject resource = Resources.Load<GameObject>("Common");
                    GameObject common = GameObject.Instantiate(resource);
                    common.name = "Common";
                    break;
                }
            }
        }
    }

    private void Awake()
    {
        if (!IsInitialize) { StartCoroutine(InitializeCoroutine()); }
        else { Destroy(this.gameObject); }
    }

    /// <summary>
    /// コンフィグ設定
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitializeCoroutine()
    {
        while (!SceneManager.IsExist()) { yield return null; }

        IsInitialize = true;
    }
}