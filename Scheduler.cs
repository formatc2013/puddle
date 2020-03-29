using System;
using System.Collections;
using UnityEngine;

public class Scheduler : MonoBehaviour
{
    [SerializeField] private float cleanTimeInterval;
    private WaitForSeconds cleanTimeIntervalWFS;

    public static Action OnAutoMemoryCleanRequest = delegate { };

    private void Awake()
    {
        cleanTimeInterval = cleanTimeInterval == 0 ? 10f : cleanTimeInterval;
        cleanTimeIntervalWFS = new WaitForSeconds(cleanTimeInterval);
        StartCoroutine(ScheduleMemoryPoolAutoClean());
    }

    private IEnumerator ScheduleMemoryPoolAutoClean()
    {
        yield return cleanTimeIntervalWFS;
        OnAutoMemoryCleanRequest();
        StartCoroutine(ScheduleMemoryPoolAutoClean());
    }
}