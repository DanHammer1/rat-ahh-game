using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Timer : MonoBehaviour
{
    private float duration;
    private float timer;

    private bool canFinish;
    private Func<bool> extraConditionsComplete;

    private Action onFinish;

    private List<GameObject> subscribedObjects;

    public enum OnFinish
    {
        DESTROY,
        REPEAT,
        CUSTOM
    }

    private OnFinish finishState;

    public void Subscribe(GameObject objectRef) {
        subscribedObjects.Add(objectRef);
    }

    public static GameObject CreateTimer(float length, OnFinish finishState, 
        Action finishAction, string name,
        Func<bool> extraConditionsComplete = null,
        params GameObject[] objectsToSubscribe) {
        
        
        GameObject newGameObject = new GameObject(name);
        newGameObject.transform.SetParent(GameObject.FindWithTag("TimerParent").transform);

        Timer newTimer = newGameObject.AddComponent<Timer>();
        newTimer.duration = length;
        newTimer.timer = length;

        newTimer.subscribedObjects = new List<GameObject>();
        
        foreach (GameObject objectRef in objectsToSubscribe) {
            newTimer.Subscribe(objectRef);
        }
        
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
        foreach (GameObject objectRef in subscribedObjects) {
            if (objectRef == null) Destroy(this.gameObject);
        }

        timer -= Time.deltaTime;

        canFinish = (extraConditionsComplete() || canFinish);
        if (timer <= 0 && canFinish) {
            onFinish?.Invoke();
        }
    }
}
