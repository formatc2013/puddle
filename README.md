Features: auto clean up on set intervals.


Usage: 
!!!You have to create a class that inherits from AbstractPoolable, and add the component to your poolable gameobject. After that you can proceed.!!!


Init:
Awake()=>
myPool = new ObjectPool(myObjectToPool, initialElements, useActivePool, autoCleanup, autoCleanUpInterval);

Pool out:
OnTriggerEnter()=>
myPool.AddObjectAtPosition(enemyPosition);

Pool back:


OnDeath()=>
Poolback();

!!! - this is the tricky part from your side; calling it is easy, just got to find the right function, my general advice is not to use OnDisable()!!!

