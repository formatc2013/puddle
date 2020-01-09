using UnityEngine;
using System.Collections.Generic;


public class ObjectPool
{
    private Stack<GameObject> primaryPool;

    private GameObject goToPool;

    private int rangeOfFirstPool;

    private Stack<GameObject> secondaryPool;

    private GameObject primaryParentGO;
    private GameObject secondaryParentGO;


    private int rangeOfSecondaryPool;

    private int numberOfInstancesToCreateWhenAllQueuesEmpty;


    public ObjectPool(GameObject gotopool,
        int numberofinitialelements = 4,
        int startaddingtosecondarypoolat = 8,
        int rangeofsecodarypool = 8,
        int numberofinstancestocreatewhenallqueuesempty = 4) {

        InitializeStack(gotopool,numberofinitialelements,startaddingtosecondarypoolat,
            rangeofsecodarypool,numberofinstancestocreatewhenallqueuesempty);
    }

    /// <summary>
    /// </summary>
    /// <param name="gotopool">Must implement IPoolable</param>
    /// <param name="numberofinitialelements"></param>
    /// <param name="startaddingtosecondarypoolat"></param>
    private void InitializeStack(GameObject gotopool, 
        int numberofinitialelements = 4,
        int startaddingtosecondarypoolat = 8,
        int rangeofsecodarypool = 8,
        int numberofinstancestocreatewhenallqueuesempty=4) {

        if (gotopool.GetComponent<IPoolable>() == null)
        {
            Debug.LogError("gotopool must have a component that implements IPoolable!" +
                " Aborting initialization!");
            return;
        }

        rangeOfFirstPool = startaddingtosecondarypoolat;

        rangeOfSecondaryPool = rangeofsecodarypool;

        numberOfInstancesToCreateWhenAllQueuesEmpty = numberofinstancestocreatewhenallqueuesempty;

        secondaryPool = new Stack<GameObject>();

        goToPool = gotopool;

        primaryPool = new Stack<GameObject>();

        primaryParentGO = GameObject.Instantiate(
            new GameObject("primary " + goToPool.ToString()));

        secondaryParentGO = GameObject.Instantiate(
            new GameObject("seconary " + goToPool.ToString()));

        for (int i = 0; i < numberofinitialelements; i++)
        {
            GameObject gotoadd = GenerateNewInstance();

            gotoadd.SetActive(false);

            gotoadd.transform.SetParent(primaryParentGO.transform);

            primaryPool.Push(gotoadd);
        }
    }

    private GameObject GenerateNewInstance()
    {
        var gotoadd = GameObject.Instantiate(goToPool, new Vector3(0, -100, 0), Quaternion.identity);

        gotoadd.GetComponent<IPoolable>().SetPooler(this);
        return gotoadd;
    }

    public GameObject AddObjectAtPosition(Vector3 position,bool isactive=true) {

        //use secondary pool first
        if (secondaryPool.Count > 0)
        {
            return GetInstanceFromSecondaryPool(position, isactive);
        }

        //than primary
        else if (primaryPool.Count > 0)
        {
            return GetInstanceFromPrimaryPool(position, isactive); 
        }

        //if both empty instantiate
        else
        {
            return GenerateInstances(position,isactive);
        }

    }

    private GameObject GenerateInstances(Vector3 position,bool isactive=false)
    {
        GameObject gotoaddtolist = null;

        //add new instances to the list
        for (int i = 0; i < numberOfInstancesToCreateWhenAllQueuesEmpty; i++)
        {
            gotoaddtolist = GenerateNewInstance();
            primaryPool.Push(gotoaddtolist);
            gotoaddtolist.SetActive(false);          
            gotoaddtolist.transform.SetParent(primaryParentGO.transform);

            Debug.Log("Adding element to the stack: " + gotoaddtolist+i);

        }

        gotoaddtolist = GenerateNewInstance();
        gotoaddtolist.transform.position = position;
        gotoaddtolist.SetActive(isactive);

        return gotoaddtolist;
    }

    private GameObject GetInstanceFromPrimaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = primaryPool.Pop();
        objecttoreturn = objecttoreturn == null ? GenerateInstances(position) : objecttoreturn;

        objecttoreturn.transform.position = position;

        objecttoreturn.SetActive(isactive);

        objecttoreturn.transform.parent = null;

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


        //Debug.Log("adding from 2. pool: " + objecttoreturn);
        return objecttoreturn;
    }

    private void EmptySecondaryPool()
    {
        if(secondaryPool.Count<=0)return;

        for (int i = 0; i < secondaryPool.Count; i++)
        {
            GameObject.Destroy(secondaryPool.Pop());
            Debug.Log("Destroying: ");
        }
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

                //EmptySecondaryPool();
                return;
            }

            AddInstanceToSecondary(objecttohide);
           
            return;

        }

        AddInstanceToPrimary(objecttohide);

    }

    private void AddInstanceToSecondary(GameObject objecttohide)
    {
        objecttohide.SetActive(false);

        objecttohide.transform.SetParent(secondaryParentGO.transform);

        secondaryPool.Push(objecttohide);
    }

    private void AddInstanceToPrimary(GameObject objecttohide)
    {
        objecttohide.SetActive(false);

        objecttohide.transform.SetParent(primaryParentGO.transform);

        primaryPool.Push(objecttohide);
    }
}
