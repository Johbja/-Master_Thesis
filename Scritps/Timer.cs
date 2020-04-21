using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{

    private float currentTime;
    private float duration;
    private bool isStarted;
    private bool isCompleted;

    public void InitializeTimer() {
        isStarted = false;
        isCompleted = true;
    }

    public void StarTimer(float duration) {
        if(!isStarted) {
            this.duration = duration;
            currentTime = 0;
            isStarted = true;
            isCompleted = false;
            StartCoroutine(Count());
        }
    }

    public bool IsCompleted() {
        return isCompleted;
    }

    private IEnumerator Count() {
        while(currentTime < duration) {
            currentTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isCompleted = true;
        isStarted = false;
    }

}
