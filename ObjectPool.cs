using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    private Stack<GameObject> primaryPool, secondaryPool;

    private List<GameObject> activePool;
    
    //this is for debugging pools, you can comment this out with the setlist() calls
    public List<GameObject> prim, sec;

    private GameObject goToPool;

    public int rangeOfFirstPool;

    //private GameObject primaryParentGO;
    //private GameObject secondaryParentGO;

    private int rangeOfSecondaryPool;

    private int numberOfInstancesToCreateWhenAllQueuesEmpty;

    private bool useActivePool;
    private bool hasRequestedObjectsSinceLastCleanup;
    private bool autoCleanUpEnabled;
    private int cleanAttempts;

    public ObjectPool(GameObject gotopool,
        int numberofinitialelements = 4,
        int startaddingtosecondarypoolat = 8,
        int rangeofsecodarypool = 8,
        int numberofinstancestocreatewhenallqueuesempty = 4,
        bool useactivepool = false,
        bool isautocleanupenabled = true)
    {
        InitializeStack(gotopool, numberofinitialelements, startaddingtosecondarypoolat,
            rangeofsecodarypool, numberofinstancestocreatewhenallqueuesempty, useactivepool, isautocleanupenabled);

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
        int startaddingtosecondarypoolat = 8,
        int rangeofsecodarypool = 8,
        int numberofinstancestocreatewhenallqueuesempty = 4,
        bool useactivepool = false,
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

        rangeOfFirstPool = startaddingtosecondarypoolat;

        rangeOfSecondaryPool = rangeofsecodarypool;

        numberOfInstancesToCreateWhenAllQueuesEmpty = numberofinstancestocreatewhenallqueuesempty;

        secondaryPool = new Stack<GameObject>();

        goToPool = gotopool;

        primaryPool = new Stack<GameObject>();

        // primaryParentGO = GameObject.Instantiate(
        //     new GameObject("primary " + goToPool.ToString()));

        // secondaryParentGO = GameObject.Instantiate(
        //    new GameObject("seconary " + goToPool.ToString()));

        for (int i = 0; i < numberofinitialelements; i++)
        {
            GameObject gotoadd = GenerateNewInstance();

            gotoadd.SetActive(false);

            //   gotoadd.transform.SetParent(primaryParentGO.transform);

            primaryPool.Push(gotoadd);
        }
        //only for debugging, comment this out if not needed
        setlists();
    }

    private void setlists()
    {
        prim = primaryPool.ToList();
        sec = secondaryPool.ToList();
    }

    private GameObject GenerateNewInstance()
    {
        var gotoadd = GameObject.Instantiate(goToPool, new Vector3(0, -100, 0), Quaternion.identity);

        gotoadd.GetComponent<IPoolable>().SetPool(this);
        setlists();
        return gotoadd;
    }

    public GameObject AddObjectAtPosition(Vector3 position, bool isactive = true)
    {
        hasRequestedObjectsSinceLastCleanup = true;

        //use secondary pool first
        if (secondaryPool.Count > 0)
        {
            var objecttoreturn = GetInstanceFromSecondaryPool(position, isactive);
            if (useActivePool)

                activePool.Add(objecttoreturn);
            return objecttoreturn;
        }

        //than primary
        else if (primaryPool.Count > 0)
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
        setlists();
        return gotoaddtolist;
    }

    private GameObject GetInstanceFromPrimaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = primaryPool.Pop();
        objecttoreturn = objecttoreturn == null ? GenerateInstances(position) : objecttoreturn;

        objecttoreturn.transform.position = position;

        objecttoreturn.SetActive(isactive);

        objecttoreturn.transform.parent = null;
        setlists();
        //Debug.Log("adding from 1. pool: " + objecttoreturn);
        return objecttoreturn;
    }

    private GameObject GetInstanceFromSecondaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = secondaryPool.Pop();

        objecttoreturn = objecttoreturn == null ? primaryPool.Pop() : objecttoreturn;

        objecttoreturn = objecttoreturn == null ? GenerateInstances(position) : objecttoreturn;

        objecttoreturn.transform.position = position;

        objecttoreturn.SetActive(isactive);

        objecttoreturn.transform.parent = null;
         //only for debugging, comment this out if not needed
        setlists();
        //Debug.Log("adding from 2. pool: " + objecttoreturn);
        return objecttoreturn;
    }

    private void EmptySecondaryPool()
    {
        if (secondaryPool.Count <= 0) return;

        for (int i = 0; i < secondaryPool.Count; i++)
        {
            GameObject.Destroy(secondaryPool.Pop());
            Debug.Log("Destroying: ");
        }
         //only for debugging, comment this out if not needed
        setlists();
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
        //if there are too many in the stack
        //add them to the secondary stack
        if (primaryPool.Count >= rangeOfFirstPool)
        {
            if (secondaryPool.Count >= rangeOfSecondaryPool)
            {
                Debug.Log("Destroying: " + objecttohide);
                GameObject.Destroy(objecttohide);
                if (secondaryPool.Count > 0)
                    GameObject.Destroy(secondaryPool.Pop());
                setlists();
                //EmptySecondaryPool();
                return;
            }

            AddInstanceToSecondary(objecttohide);
             //only for debugging, comment this out if not needed
            setlists();
            return;
        }
        setlists();
        AddInstanceToPrimary(objecttohide);
    }

    private void AddInstanceToSecondary(GameObject objecttohide)
    {
        objecttohide.SetActive(false);
        if (useActivePool)

            activePool.Remove(objecttohide);
        //objecttohide.transform.SetParent(secondaryParentGO.transform);

        secondaryPool.Push(objecttohide);
         //only for debugging, comment this out if not needed
        setlists();
    }

    private void AddInstanceToPrimary(GameObject objecttohide)
    {
        objecttohide.SetActive(false);
        if (useActivePool)
            activePool.Remove(objecttohide);
        //objecttohide.transform.SetParent(primaryParentGO.transform);

        primaryPool.Push(objecttohide);
         //only for debugging, comment this out if not needed
        setlists();
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
            cleanAttempts=0;
            return;
        }

        if (secondaryPool.Count > 0)
        {
            GameObject.Destroy(secondaryPool.Pop());
            //Debug.Log("cleaned from secondary: " + goToPool);
        }
        else if (cleanAttempts <= 4){

            cleanAttempts++;
            

        }
        else {

            //clean first pool
            if (primaryPool.Count >= rangeOfFirstPool / 2)
            {

                GameObject.Destroy(primaryPool.Pop());
                //Debug.Log("cleaned from primary: " + goToPool);

            }
            //else Debug.Log("no more cleaning needed: " + goToPool);

        }


        hasRequestedObjectsSinceLastCleanup = false;
          //only for debugging, comment this out if not needed
        setlists();
    }

    public void DisablePool() {

        Scheduler.OnAutoMemoryCleanRequest -= AutoCleanUp;
        if(primaryPool.Count>0)
        foreach (var item in primaryPool)
        {
            GameObject.Destroy(item);
            //Debug.Log("Disabled and destroyed in primary: "+item);
        }
        if (secondaryPool.Count > 0)
            foreach (var item in secondaryPool)
        {
            GameObject.Destroy(item);
            //Debug.Log("Disabled and destroyed in secondary: "+item);
        }

    }
}
