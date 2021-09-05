using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 遷移演出の種類
/// </summary>
public enum TransitionEffectType
{
    None,
    Fade
}

/// <summary>
/// 遷移管理
/// </summary>
public class TransitionManager : MonoBehaviourSingleton<TransitionManager>
{
    public TransitionEffectType EffectType { get; private set; } = TransitionEffectType.None;
    public UnityAction<TransitionState> startedCallback = null;
    public UnityAction<TransitionState> finishedCallback = null;
    public TransitionState State { get; private set; } = TransitionState.None;

    [SerializeField] private Fader fader = null;
    private UnityAction callback_ = null;

    /// <summary>
    /// 遷移開始
    /// </summary>
    /// <param name="transitionEffectType"></param>
    public bool StartTransition(TransitionEffectType transitionEffectType, TransitionState transitionState, UnityAction callback)
    {
        TransitionBase prevTransitionBase = GetTransitionBase(EffectType);
        if (prevTransitionBase != null && prevTransitionBase.State != TransitionState.None) { return false; }
        if (prevTransitionBase != null && EffectType != transitionEffectType) { prevTransitionBase.gameObject.SetActive(false); }

        EffectType = transitionEffectType;
        State = transitionState;
        TransitionBase transitionBase = GetTransitionBase(EffectType);
        if (startedCallback != null) { startedCallback(State); }
        if (transitionBase != null)
        {
            // エフェクト付き遷移
            callback_ = callback;
            return transitionBase.SetTransitionState(transitionState, CallFinishedTransitionCallback);
        }
        else
        {
            // エフェクトなし遷移
            callback_ = callback;
            CallFinishedTransitionCallback();
        }

        return true;
    }

    /// <summary>
    /// 遷移エフェクト取得
    /// </summary>
    /// <param name="transitionEffectType"></param>
    /// <returns></returns>
    public TransitionBase GetTransitionBase(TransitionEffectType transitionEffectType)
    {
        switch (transitionEffectType)
        {
            case TransitionEffectType.Fade: return fader;
            default: break;
        }

        return null;
    }

    /// <summary>
    /// 遷移完了
    /// </summary>
    private void CallFinishedTransitionCallback()
    {
        if (callback_ != null)
        {
            UnityAction temp = callback_;
            callback_ = null;
            temp();
        }

        if (finishedCallback != null) { finishedCallback(State); }
    }
}