using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 遷移状態
/// </summary>
public enum TransitionState
{
    ScreenOut,
    ScreenIn,
    None
}

/// <summary>
/// 遷移ベース
/// </summary>
public abstract class TransitionBase : MonoBehaviour
{
    public TransitionState State { get; private set; } = TransitionState.None;

    private UnityAction callback_ = null;

    /// <summary>
    /// 遷移状態設定
    /// </summary>
    /// <param name="transitionState"></param>
    /// <returns></returns>
    public bool SetTransitionState(TransitionState transitionState, UnityAction callback)
    {
        // 遷移中でないときのみ有効
        if (State != TransitionState.None)
        {
            return false;
        }

        // 遷移開始
        callback_ = callback;
        State = transitionState;
        StartTransition(State);

        return true;
    }

    /// <summary>
    /// 遷移終了
    /// </summary>
    protected void FinishTransition()
    {
        State = TransitionState.None;
        if (callback_ != null)
        {
            UnityAction temp = callback_;
            callback_ = null;
            temp();
        }
    }

    /// <summary>
    /// 遷移開始
    /// </summary>
    /// <param name="transitionState"></param>
    protected abstract void StartTransition(TransitionState transitionState);
}