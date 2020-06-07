using UnityEngine;

public abstract class AbstractPoolable : MonoBehaviour
{
    protected ObjectPool m_pool { get; private set; }

    public virtual void PoolBack()
    {
        if (gameObject && m_pool != null)
            m_pool.HideObject(this);
    }

    public virtual void SetPool(ObjectPool objectPool)
    {
        if (objectPool != null)
            m_pool = objectPool;
        else
            print("No pool found or pool is null!");
    }
}