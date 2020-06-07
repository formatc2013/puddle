using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ObjectPool
{
    private Stack<AbstractPoolable> pool;//, secondaryPool;

    private List<AbstractPoolable> activePool;

    private AbstractPoolable goToPool;

    public int rangeOfPool;

    private int numberOfInstancesToCreateWhenAllQueuesEmpty;

    private bool useActivePool;

    private bool autoCleanUpEnabled;

    private MemoryPoolAutoCleanUpScheduler m_scheduler;

    public ObjectPool(AbstractPoolable gotopool,
        int numberofinitialelements = 14,
        bool useactivepool = false,
        bool isautocleanupenabled = true,
        float cleantimeinterval = 3f)
    {
        InitializeStack(
            gotopool,
            numberofinitialelements,
            useactivepool,
            isautocleanupenabled);

        if (isautocleanupenabled)
        {
            m_scheduler = new GameObject(gotopool.ToString() + "_memorycleanscheduler").AddComponent<MemoryPoolAutoCleanUpScheduler>();
            m_scheduler.CleanTimeInterval = cleantimeinterval;
            m_scheduler.OnAutoMemoryCleanRequest += AutoCleanUp;
        }
    }

    ~ObjectPool()
    {
        m_scheduler.OnAutoMemoryCleanRequest -= AutoCleanUp;
    }

    /// <summary>
    /// </summary>
    /// <param name="gotopool">Must implement IPoolable</param>
    /// <param name="numberofinitialelements"></param>
    /// <param name="startaddingtosecondarypoolat"></param>
    private void InitializeStack(AbstractPoolable gotopool,
        int numberofinitialelements = 4,
        bool useactivepool = false,
        bool isautocleanupenabled = true)
    {
        autoCleanUpEnabled = isautocleanupenabled;

        if (gotopool == null)
        {
            Debug.LogError("Gotopool is null!");
            return;
        }

        useActivePool = useactivepool;

        activePool = new List<AbstractPoolable>();

        rangeOfPool = numberofinitialelements;

        goToPool = gotopool;

        pool = new Stack<AbstractPoolable>();

        for (int i = 0; i < numberofinitialelements; i++)
        {
            var gotoadd = GenerateNewInstance();

            gotoadd.gameObject.SetActive(false);

            pool.Push(gotoadd);
        }

        //setlists();
    }

    /*private void setlists()
    {
        prim = primaryPool.ToList();
        sec = secondaryPool.ToList();
    }*/

    private AbstractPoolable GenerateNewInstance()
    {
        var gotoadd = GameObject.Instantiate(goToPool, new Vector3(0, -100, 0), Quaternion.identity);

        gotoadd.SetPool(this);
        //setlists();
        return gotoadd;
    }

    public AbstractPoolable AddObjectAtPosition(Vector3 position, bool isactive = true)
    {
        if (pool == null)
        {
            Debug.LogError("Pool is null! Something wrong with your implementation");
            return null;
        }
        if (pool.Count > 0)
        {
            var objecttoreturn = GetInstanceFromPrimaryPool(position, isactive);
            if (useActivePool)

                activePool.Add(objecttoreturn);
            return objecttoreturn;
        }

        //if both empty instantiate
        else
        {
            var objecttoreturn = GenerateInstances(position, isactive);
            if (useActivePool)

                activePool.Add(objecttoreturn);
            return objecttoreturn;
        }
    }

    public GameObject AddGameObjectAtPosition(Vector3 position, bool isactive = true)
    {
        if (pool == null)
        {
            Debug.LogError("Pool is null! Something wrong with your implementation");
            return null;
        }
        if (pool.Count > 0)
        {
            var objecttoreturn = GetInstanceFromPrimaryPool(position, isactive);
            if (useActivePool)

                activePool.Add(objecttoreturn);
            return objecttoreturn.gameObject;
        }

        //if both empty instantiate
        else
        {
            var objecttoreturn = GenerateInstances(position, isactive);
            if (useActivePool)

                activePool.Add(objecttoreturn);
            return objecttoreturn.gameObject;
        }
    }

    private AbstractPoolable GenerateInstances(Vector3 position, bool isactive = false)
    {
        AbstractPoolable gotoaddtolist = null;
        if (numberOfInstancesToCreateWhenAllQueuesEmpty == 0) numberOfInstancesToCreateWhenAllQueuesEmpty = 3;
        //add new instances to the list
        for (int i = 0; i < numberOfInstancesToCreateWhenAllQueuesEmpty; i++)
        {
            gotoaddtolist = GenerateNewInstance();
            pool.Push(gotoaddtolist);
            gotoaddtolist.gameObject.SetActive(isactive);
            // gotoaddtolist.transform.SetParent(primaryParentGO.transform);

            //Debug.Log("Adding element to the stack: " + gotoaddtolist + i);
        }

        Debug.Log("Adding elements: " + numberOfInstancesToCreateWhenAllQueuesEmpty + ", to the stack: " + gotoaddtolist);

        gotoaddtolist = GenerateNewInstance();
        gotoaddtolist.transform.position = position;
        gotoaddtolist.gameObject.SetActive(isactive);

        //setlists();
        return gotoaddtolist;
    }

    private AbstractPoolable GetInstanceFromPrimaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = pool.Pop();

        if (!objecttoreturn)
        {
            Debug.Log("null in pool!");
            return GenerateInstances(position); ;
        }

        objecttoreturn.transform.position = position;

        objecttoreturn.gameObject.SetActive(isactive);

        objecttoreturn.transform.parent = null;
        //setlists();
        //Debug.Log("adding from 1. pool: " + objecttoreturn);
        return objecttoreturn;
    }

    /// <summary>
    /// Pooling back.
    /// </summary>
    /// <param name="objecttohide"></param>
    public void HideObject(AbstractPoolable objecttohide)
    {
        if (!objecttohide)
        {
            Debug.LogError("Objecttohide is null! Abort pooling!");
            return;
        }

        AbstractPoolable objectpoolable = objecttohide.GetComponent<AbstractPoolable>();

        if (objectpoolable == null)
        {
            Debug.LogError("A non-poolable object is trying to get into the pooling system." +
                "Add an IPoolable script to your gameobject! " +
                "Destroying! ");
            DestroyGO(objecttohide);
            return;
        }

        //setlists();
        AddInstanceToPool(objecttohide);
    }

    private void AddInstanceToPool(AbstractPoolable objecttohide)
    {
        objecttohide.gameObject.SetActive(false);

        if (useActivePool)
            activePool.Remove(objecttohide);
        if (pool != null)
            pool.Push(objecttohide);
        //setlists();
    }

    private void AutoCleanUp()
    {
        if (!autoCleanUpEnabled || pool.Count <= 1) return;

        //clean first pool
        if (pool.Count > rangeOfPool)
        {
            var cleanedone = pool.Pop();

            DestroyGO(cleanedone);

            Debug.Log("Cleaning: " + cleanedone + ", elements left in pool: " + pool.Count);
        }
    }

    public void DisablePool()
    {
        activePool.ForEach(x => DestroyGO(x));

        activePool.Clear();

        if (m_scheduler)
            m_scheduler.OnAutoMemoryCleanRequest -= AutoCleanUp;

        if (pool.Count > 0)
            foreach (var pooleditem in pool)
            {
                DestroyGO(pooleditem);
            }
    }

    public bool IsEmpty() => pool == null || pool.Count == 0;

    public void DestroyGO<T>(T objecttokill) where T : MonoBehaviour
    {
        if (objecttokill == null) return;

        var tokill = objecttokill as GameObject;

        if (tokill) GameObject.Destroy(objecttokill);
        else GameObject.Destroy(objecttokill.gameObject);
    }
}