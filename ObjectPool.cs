using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    private Stack<GameObject> primaryPool;//, secondaryPool;

    private List<GameObject> activePool;

    private GameObject goToPool;

    public int rangeOfFirstPool;

    private int numberOfInstancesToCreateWhenAllQueuesEmpty;

    private bool useActivePool;
    private bool hasRequestedObjectsSinceLastCleanup;
    private bool autoCleanUpEnabled;

    public ObjectPool(GameObject gotopool,
        int numberofinitialelements = 14,
        bool useactivepool = true,
        bool isautocleanupenabled = true)
    {
        InitializeStack(
            gotopool, 
            numberofinitialelements,
            useactivepool,
            isautocleanupenabled);

        Scheduler.OnAutoMemoryCleanRequest += AutoCleanUp;
    }
   ~ObjectPool() { Scheduler.OnAutoMemoryCleanRequest -= AutoCleanUp; }

    /// <summary>
    /// </summary>
    /// <param name="gotopool">Must implement IPoolable</param>
    /// <param name="numberofinitialelements"></param>
    /// <param name="startaddingtosecondarypoolat"></param>
    private void InitializeStack(GameObject gotopool,
        int numberofinitialelements = 4,
        bool useactivepool = true,
        bool isautocleanupenabled = true)
    {
        autoCleanUpEnabled = isautocleanupenabled;

        if (gotopool == null)
        {
            Debug.LogError("Gotopool is null!");
            return;
        }

        if (gotopool.GetComponent<IPoolable>() == null)
        {
            Debug.LogError("Gotopool must have a component that implements IPoolable!" +
                " Aborting initialization!");
            return;
        }

        useActivePool = useactivepool;

        activePool = new List<GameObject>();

        rangeOfFirstPool = numberofinitialelements;
            
        goToPool = gotopool;

        primaryPool = new Stack<GameObject>();

        for (int i = 0; i < numberofinitialelements; i++)
        {
            GameObject gotoadd = GenerateNewInstance();

            gotoadd.SetActive(false);

            primaryPool.Push(gotoadd);
        }

        //setlists();
    }

    /*private void setlists()
    {
        prim = primaryPool.ToList();
        sec = secondaryPool.ToList();
    }*/

    private GameObject GenerateNewInstance()
    {
        var gotoadd = GameObject.Instantiate(goToPool, new Vector3(0, -100, 0), Quaternion.identity);

        gotoadd.GetComponent<IPoolable>().SetPool(this);
        //setlists();
        return gotoadd;
    }

    public GameObject AddObjectAtPosition(Vector3 position, bool isactive = true)
    {
        hasRequestedObjectsSinceLastCleanup = true;

       if (primaryPool.Count > 0)
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

    private GameObject GenerateInstances(Vector3 position, bool isactive = false)
    {
        GameObject gotoaddtolist = null;

        //add new instances to the list
        for (int i = 0; i < numberOfInstancesToCreateWhenAllQueuesEmpty; i++)
        {
            gotoaddtolist = GenerateNewInstance();
            primaryPool.Push(gotoaddtolist);
            gotoaddtolist.SetActive(false);
            // gotoaddtolist.transform.SetParent(primaryParentGO.transform);

            //Debug.Log("Adding element to the stack: " + gotoaddtolist + i);
        }

        Debug.Log("Adding elements: " + numberOfInstancesToCreateWhenAllQueuesEmpty + ", to the stack: " + gotoaddtolist);

        gotoaddtolist = GenerateNewInstance();
        gotoaddtolist.transform.position = position;
        gotoaddtolist.SetActive(isactive);

        //setlists();
        return gotoaddtolist;
    }

    private GameObject GetInstanceFromPrimaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = primaryPool.Pop();

        objecttoreturn = objecttoreturn == null ? GenerateInstances(position) : objecttoreturn;

        objecttoreturn.transform.position = position;

        objecttoreturn.SetActive(isactive);

        objecttoreturn.transform.parent = null;
        //setlists();
        //Debug.Log("adding from 1. pool: " + objecttoreturn);
        return objecttoreturn;
    }

    /// <summary>
    /// Pooling back.
    /// </summary>
    /// <param name="objecttohide"></param>
    public void HideObject(GameObject objecttohide)
    {
        if (!objecttohide)
        {
            Debug.LogError("Objecttohide is null! Abort pooling!");
            return;
        }

        IPoolable objectpoolable = objecttohide.GetComponent<IPoolable>();

        if (objectpoolable == null)
        {
            Debug.LogError("A non-poolable object is trying to get into the pooling system." +
                "Add an IPoolable script to your gameobject! " +
                "Abort pooling! ");
            return;
        }
   
        //setlists();
        AddInstanceToPrimary(objecttohide);
    }
   
    private void AddInstanceToPrimary(GameObject objecttohide)
    {
        objecttohide.SetActive(false);
        if (useActivePool)
            activePool.Remove(objecttohide);
        //objecttohide.transform.SetParent(primaryParentGO.transform);

        primaryPool.Push(objecttohide);
        //setlists();
    }

    public void HideActiveOnes()
    {
        if (activePool == null || activePool.Count == 0)
            if (!useActivePool) { Debug.LogError("No activePool enabled!"); return; }
        foreach (var activeitem in activePool.ToList())
        {
            if (activeitem)
                HideObject(activeitem);
        }
    }

    private void AutoCleanUp()
    {
        if (!autoCleanUpEnabled) return;
        //if not in use start emptying secondary pool
        if (hasRequestedObjectsSinceLastCleanup)
        {
            //Debug.Log("not cleaned: " + goToPool);
            hasRequestedObjectsSinceLastCleanup = false;
            //cleanAttempts=0;
            return;
        }

        
            //clean first pool
            if (primaryPool.Count >= rangeOfFirstPool / 2)
            {

                GameObject.Destroy(primaryPool.Pop());
                //Debug.Log("cleaned from primary: " + goToPool);

            }
            //else Debug.Log("no more cleaning needed: " + goToPool);

        //}


        hasRequestedObjectsSinceLastCleanup = false;
    }

    public void DisablePool() {

        Scheduler.OnAutoMemoryCleanRequest -= AutoCleanUp;
        if(primaryPool.Count>0)
        foreach (var item in primaryPool)
        {
            GameObject.Destroy(item);
            //Debug.Log("Disabled and destroyed in primary: "+item);
        }

    }


}