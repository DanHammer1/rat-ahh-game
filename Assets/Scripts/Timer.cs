using UnityEngine;
using System;

public class Timer : MonoBehaviour
{
    private float duration;
    private float timer;

    private bool canFinish;
    private Func<bool> extraConditionsComplete;

    private Action onFinish;

    public enum OnFinish
    {
        DESTROY,
        REPEAT,
        CUSTOM
    }

    private OnFinish finishState;

    public static GameObject CreateTimer(float length, OnFinish finishState, 
        Action finishAction, string name, Func<bool> extraConditionsComplete = null) {
        
        GameObject newGameObject = new GameObject(name);
        newGameObject.transform.SetParent(GameObject.FindWithTag("TimerParent").transform);

        Timer newTimer = newGameObject.AddComponent<Timer>();
        newTimer.duration = length;
        newTimer.timer = length;
        
        if (extraConditionsComplete == null) {
            newTimer.extraConditionsComplete = () => true;
        } else {
            newTimer.extraConditionsComplete = extraConditionsComplete;
        }

        newTimer.onFinish += finishAction;
        switch(finishState) {
            case OnFinish.DESTROY:
                newTimer.onFinish += () => Destroy(newTimer.gameObject);   
                break;
            case OnFinish.REPEAT:
                newTimer.onFinish += () => CreateTimer(length, OnFinish.REPEAT, finishAction, name);
                newTimer.onFinish += () => Destroy(newTimer.gameObject);
                break;
            case OnFinish.CUSTOM:
                break;
        }

        return newGameObject;
    }

    public static GameObject CreateTimer(float length, OnFinish finishState, Action finishAction) {
        return CreateTimer(length, finishState, finishAction, "Unnamed Timer");
    }

    public static GameObject CreateTimer(float length, Action finishAction) {
        return CreateTimer(length, OnFinish.CUSTOM, finishAction);
    }

    public static GameObject CreateTimer(float length, Action finishAction, string name) {
        return CreateTimer(length, OnFinish.CUSTOM, finishAction, name);
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        canFinish = (extraConditionsComplete() || canFinish);
        if (timer <= 0 && canFinish) {
            onFinish?.Invoke();
        }
    }
}
