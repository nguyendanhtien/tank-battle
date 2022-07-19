using System;
using System.Collections.Generic;
using UnityEngine;

internal class MainThread : MonoBehaviour
{
    internal static MainThread singleton;
    Queue<Action> jobs = new Queue<Action>();

    void Awake() {
        singleton = this;
    }

    void Update() {
        while (jobs.Count > 0) 
            jobs.Dequeue().Invoke();
    }

    internal void AddJob(Action newJob) {
        jobs.Enqueue(newJob);
    }
}