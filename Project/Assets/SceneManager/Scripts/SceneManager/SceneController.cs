using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// シーン制御
/// </summary>
public class SceneController : MonoBehaviour
{
    public bool IsInitialized { get; private set; } = false;
    public bool IsDestroyed { get; private set; } = false;

    /// <summary>
    /// スクリーンアウト開始
    /// </summary>
    public virtual void StartedSceneChange() { }

    /// <summary>
    /// スクリーンイン完了
    /// </summary>
    public virtual void FinishedSceneChange() { }

    /// <summary>
    /// シーン初期化
    /// </summary>
    public void Initialize()
    {
        StartCoroutine(WaitingForTheEndCoroutine(OnInitializeCoroutine(), () => {
            IsInitialized = true;
        }));
    }

    /// <summary>
    /// シーン削除
    /// </summary>
    public void Destory()
    {
        StartCoroutine(WaitingForTheEndCoroutine(OnDestroyCoroutine(), () => {
            IsDestroyed = true;
        }));
    }

    /// <summary>
    /// シーン初期化コルーチン
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator OnInitializeCoroutine()
    {
        yield return null;
    }

    /// <summary>
    /// シーン削除コルーチン
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator OnDestroyCoroutine()
    {
        yield return null;
    }

    /// <summary>
    /// コルーチン完了待ち
    /// </summary>
    /// <param name="enumerator"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator WaitingForTheEndCoroutine(IEnumerator enumerator, Action callback)
    {
        while (!Boot.IsInitialize) { yield return null; }
        yield return StartCoroutine(enumerator);
        if (callback != null) { callback(); }
    }
}