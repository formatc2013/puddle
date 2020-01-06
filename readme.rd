An alpha version of my object pooling system implemented in Unity 2019.2

The pool dynamically grows and shrinks and GameObjects come and go.
Any GameObject can go in and out that implements the IPoolable interface, but not required to use it.

Instructions:

1.Create a class that implements the IPoolable interface.
  Add your new script to the GameObject that you wish to pool.

2.Create an instance of the ObjectPool class and
  declare your "poolable" GameObject in the script wher you are implementing the pool,
  so you can reference it in the below method.
  Call the initalizeStack method.
  ->parameters: 1, GameObject of your poolable object(with IPoolable).
		2, inital number of object in the pool.
		3, the number of elements in the pool, when the secondary pool kicks in.
		4, the max size of the secondary pool. If it is reached, the script empties the secondary pool.
		5, the grow size of the pool.
Only the first parameter is neccessary to call, otherwise preset values will be called(4,8,8,4).

Something like this:

	yourPool = new ObjectPool();

        yourPool.InitializeStack(yourGameObject,10,20,10);        


3. Get one from the pool.

  yourPoolName.AddObjectAtPosition(yourPosition);

4. Get one back to the pool varies on your
 implementation of the IPoolable interface.(the interface is not neccessary to use yet),
 but you can use this method to start with...
    
  yourPoolName.AddObjectAtPosition(yourIPoolableGameObject);
		