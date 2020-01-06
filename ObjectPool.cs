using UnityEngine;
using System.Collections.Generic;


public class ObjectPool
{
    private Stack<GameObject> StackOfGameObjectsToPool;

    private GameObject GOToPool;

    private int StartShirnkingAt;

    private Stack<GameObject> stackOfUnNecesseryOnes;

    private GameObject ParentGO;

    private int maxUnnecesseryOnes;

    private int numberOfInstancesToCreateWhenAllQueuesEmpty;

    /// <summary>
    /// </summary>
    /// <param name="gotopool">Must implement IPoolable</param>
    /// <param name="numofelements"></param>
    /// <param name="startshrinkingat"></param>
    public void InitializeStack(GameObject gotopool, int numofelements = 4, int startshrinkingat = 8, int numberofunnecesseryones = 8) {

        if (gotopool.GetComponent<IPoolable>() == null)
        {
            Debug.LogError("gotopool must have a component that implements IPoolable!" +
                " Aborting initialization!");
            return;
        }

        StartShirnkingAt = startshrinkingat;

        maxUnnecesseryOnes = numberofunnecesseryones;

        numberOfInstancesToCreateWhenAllQueuesEmpty = maxUnnecesseryOnes;

        stackOfUnNecesseryOnes = new Stack<GameObject>();

        GOToPool = gotopool;

        StackOfGameObjectsToPool = new Stack<GameObject>();

        ParentGO = GameObject.Instantiate(new GameObject(GOToPool.ToString()));

        for (int i = 0; i < numofelements; i++)
        {
            var gotoadd=GameObject.Instantiate(gotopool,new Vector3(0,-100,0),Quaternion.identity);

            gotoadd.SetActive(false);

            gotoadd.transform.SetParent(ParentGO.transform);

            StackOfGameObjectsToPool.Push(gotoadd);
        }
    }

    public GameObject AddObjectAtPosition(Vector3 position,bool isactive=true) {

        //use secondary pool first
        if (stackOfUnNecesseryOnes.Count > 0)
        {
            return GetInstanceFromSecondaryPool(position, isactive);
        }

        //than primary
        else if (StackOfGameObjectsToPool.Count > 0)
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
            gotoaddtolist = GameObject.Instantiate(GOToPool);

            gotoaddtolist.SetActive(false);

            StackOfGameObjectsToPool.Push(gotoaddtolist);

            Debug.Log("Adding element to the stack: " + gotoaddtolist);

        }

        gotoaddtolist = StackOfGameObjectsToPool.Pop();

        gotoaddtolist.transform.position = position;

        gotoaddtolist.transform.SetParent(ParentGO.transform);

        gotoaddtolist.SetActive(true);

        return gotoaddtolist;
    }

    private GameObject GetInstanceFromPrimaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = StackOfGameObjectsToPool.Pop();

        objecttoreturn.transform.position = position;

        objecttoreturn.SetActive(isactive);

        //Debug.Log("adding from 1. pool: " + objecttoreturn);
        return objecttoreturn;
    }

    private GameObject GetInstanceFromSecondaryPool(Vector3 position, bool isactive)
    {
        var objecttoreturn = stackOfUnNecesseryOnes.Pop();

        objecttoreturn.transform.position = position;

        objecttoreturn.SetActive(isactive);

        //Debug.Log("adding from 2. pool: " + objecttoreturn);
        return objecttoreturn;
    }

    private void DestroyUnNecesseryOnes()
    {
        if(stackOfUnNecesseryOnes.Count<=0)return;

        for (int i = 0; i < stackOfUnNecesseryOnes.Count; i++)
        {
            GameObject.Destroy(stackOfUnNecesseryOnes.Pop());
            Debug.Log("Destroying: ");
        }
    }
    
    /// <summary>
    /// Pooling back.
    /// </summary>
    /// <param name="objecttohide"></param>
    public void HideObject(GameObject objecttohide)
    {
        //if there are too many in the stack
        //add them to the secondary stack
         if (StackOfGameObjectsToPool.Count >= StartShirnkingAt) {

           // Debug.Log("Caching for shrinking: "+StartShirnkingAt);
            //deactivate
            objecttohide.SetActive(false);
            
            //add it to the secondary pool
            stackOfUnNecesseryOnes.Push(objecttohide);

            objecttohide.transform.SetParent(ParentGO.transform);

            //if it is full empty it
            if (maxUnnecesseryOnes <= stackOfUnNecesseryOnes.Count)
            {
                Debug.Log("Destroying: "+objecttohide);
                DestroyUnNecesseryOnes();
                return;
            }
            return;

         }
        //Debug.Log("adding back to 1. pool: "+ objecttohide);

        objecttohide.SetActive(false);

        objecttohide.transform.SetParent(ParentGO.transform);

        StackOfGameObjectsToPool.Push(objecttohide);

    }
}
