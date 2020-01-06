using UnityEngine;
using System.Collections.Generic;


public class ObjectPool
{
    private Stack<GameObject> stackOfGameObjectsToPool;

    private GameObject goToPool;

    private int startAddingToSecondaryPoolAt;

    private Stack<GameObject> secondaryPool;

    private GameObject parentGO;

    private int rangeOfSecondaryPool;

    private int numberOfInstancesToCreateWhenAllQueuesEmpty;

    /// <summary>
    /// </summary>
    /// <param name="gotopool">Must implement IPoolable</param>
    /// <param name="numberofinitialelements"></param>
    /// <param name="startaddingtosecondarypoolat"></param>
    public void InitializeStack(GameObject gotopool, 
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

        startAddingToSecondaryPoolAt = startaddingtosecondarypoolat;

        rangeOfSecondaryPool = rangeofsecodarypool;

        numberOfInstancesToCreateWhenAllQueuesEmpty = numberofinstancestocreatewhenallqueuesempty;

        secondaryPool = new Stack<GameObject>();

        goToPool = gotopool;

        stackOfGameObjectsToPool = new Stack<GameObject>();

        parentGO = GameObject.Instantiate(new GameObject(goToPool.ToString()));

        for (int i = 0; i < numberofinitialelements; i++)
        {
            var gotoadd=GameObject.Instantiate(gotopool,new Vector3(0,-100,0),Quaternion.identity);

            gotoadd.SetActive(false);

            gotoadd.transform.SetParent(parentGO.transform);

            stackOfGameObjectsToPool.Push(gotoadd);
        }
    }

    public GameObject AddObjectAtPosition(Vector3 position,bool isactive=true) {

        //use secondary pool first
        if (secondaryPool.Count > 0)
        {
            return GetInstanceFromSecondaryPool(position, isactive);
        }

        //than primary
        else if (stackOfGameObjectsToPool.Count > 0)
        {
            return GetInstanceFromPrimaryPool(position, isactive); 
        }

        //if both empty instantiate
        else
        {
            return GenerateInstances(position);
        }

    }

    private GameObject GenerateInstances(Vector3 position)
    {
        GameObject gotoaddtolist = null;

        //add new instances to the list
        for (int i = 0; i < numberOfInstancesToCreateWhenAllQueuesEmpty; i++)
        {
            gotoaddtolist = GameObject.Instantiate(goToPool);

            gotoaddtolist.SetActive(false);

            stackOfGameObjectsToPool.Push(gotoaddtolist);

            Debug.Log("Adding element to the stack: " + gotoaddtolist);

        }

        gotoaddtolist = stackOfGameObjectsToPool.Pop();

        gotoaddtolist.transform.position = position;

        gotoaddtolist.transform.SetParent(parentGO.transform);

        gotoaddtolist.SetActive(true);

        return gotoaddtolist;
    }

    private GameObject GetInstanceFromPrimaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = stackOfGameObjectsToPool.Pop();

        objecttoreturn.transform.position = position;

        objecttoreturn.SetActive(isactive);

        //Debug.Log("adding from 1. pool: " + objecttoreturn);
        return objecttoreturn;
    }

    private GameObject GetInstanceFromSecondaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = secondaryPool.Pop();

        objecttoreturn.transform.position = position;

        objecttoreturn.SetActive(isactive);

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
        IPoolable objectpoolable = objecttohide.GetComponent<IPoolable>();

        if (objectpoolable == null) {

            Debug.LogError("A non-poolable object is trying to get into the pooling system." +
                "Add an IPoolable script to your gameobject! "+
                "Abort pooling! ");
            return;
        
        }
        //if there are too many in the stack
        //add them to the secondary stack
         if (stackOfGameObjectsToPool.Count >= startAddingToSecondaryPoolAt) {

           // Debug.Log("Caching for shrinking: "+StartShirnkingAt);
            //deactivate
            objecttohide.SetActive(false);
            
            //add it to the secondary pool
            secondaryPool.Push(objecttohide);

            objecttohide.transform.SetParent(parentGO.transform);

            //if it is full empty it
            if (rangeOfSecondaryPool <= secondaryPool.Count)
            {
                Debug.Log("Destroying: "+objecttohide);
                EmptySecondaryPool();
                return;
            }
            return;

         }
        //Debug.Log("adding back to 1. pool: "+ objecttohide);

        objecttohide.SetActive(false);

        objecttohide.transform.SetParent(parentGO.transform);

        stackOfGameObjectsToPool.Push(objecttohide);

    }
}
