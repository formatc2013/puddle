using System;
using System.Collections;
using UnityEngine;

public class MemoryPoolAutoCleanUpScheduler : MonoBehaviour
{
    [SerializeField] private float cleanTimeInterval = 3;
    private WaitForSeconds cleanTimeIntervalWFS;

    public Action OnAutoMemoryCleanRequest = delegate { };

    public float CleanTimeInterval { get => cleanTimeInterval; set { if (value > 0) cleanTimeInterval = value; } }

    private void Awake()
    {
        CleanTimeInterval = CleanTimeInterval == 0 ? 3f : CleanTimeInterval;
        cleanTimeIntervalWFS = new WaitForSeconds(CleanTimeInterval);
    }

    private void OnEnable()
    {
        StartScheduling();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void StartScheduling()
    {
        StartCoroutine(ScheduleMemoryPoolAutoClean());
    }

    private IEnumerator ScheduleMemoryPoolAutoClean()
    {
        yield return cleanTimeIntervalWFS;
        OnAutoMemoryCleanRequest();
        StartCoroutine(ScheduleMemoryPoolAutoClean());
    }
}