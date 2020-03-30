# puddle
A simple, dynamically growing and shrinking Object Pooling System.
  
Alpha version of my object pooling system implemented in Unity 2019.2

The pool dynamically grows and shrinks and GameObjects come and go.    
It automatically detects unused objects, and gets rid of them (has to be set up manually by adding a script to a GameObject in your scene.)    
Any GameObject can go in and out that implements the IPoolable interface, but not required to use it.

Instructions:

1. Create a script that implements the IPoolable interface. I.e:

       public class YourPoolableObject : MonoBehaviour,IPoolable
       {
    
    	    ObjectPool m_pool;
	    
    	    //it is called in the ObjectPool itself
    	    public void SetPool(ObjectPool objectPool){
	    
    	    	m_pool=objectPool;
	    	
    	    }
    	    //you call this to pool it back
    	    public void PoolBack(){
    	    
    	    	 m_pool.HideObject(gameObject);
	    	
    	    }
        }
     
  Add your new script to the GameObject that you wish to pool.

2. Create and initialize your pool(just call the constructor) in your Pool script.
	
         public class YourPool : MonoBehaviour
         {
	 
	          //assign in editor or load etc
	          [SerializeField]private GameObject yourPoolableGameObject;
	      
	          private ObjectPool yourPool;
	      
     	      void Awake(){
	      
	               yourPool = new ObjectPool(yourPoolableGameObject,10,20,10,false,true);
		         
	          }
	      }     
    
    
    
       
  
  		Parameters:
  		1, GameObject of your poolable object(with IPoolable).
		2, inital number of object in the pool.
		3, the number of elements in the pool, when the secondary pool kicks in.
		4, the max size of the secondary pool. If it is reached, the script empties the secondary pool.
		5, the grow size of the pool.
		6, whether you are using the activePool feature, which you can use to track active pool elements in your scene.
		7, whether you are using the autocleanup feature; to use this you must add a Scheduler script to a gameobject in your 			scene; cleanup intervals can be tweaked from there.
		
		
Only the first parameter is neccessary to call, otherwise preset values will be called(4,8,8,4,true,true).


3. To get one from the pool, just call AddObjectAtPosition().
You can also reference it and move it around etc.

	
	    void OnTriggerEnter(Collider other){
	
		   GameObject yournewobject = yourPool.AddObjectAtPosition(yourPosition); 
		   
		   yournewobject.transform.position = new Vector3();
	
	    }
  
 

4. If you want to get rid of your poolable object you can just call your PoolBack() function of your IPoolable:
	
       public class YourDestroyerObject : MonoBehaviour
       {	
           ...other code
	    
            void OnTriggerEnter(Collider other){
            			
                YourPoolableObject.PoolBack();
            	
            }
       		
       }
    
5. If you want to use the autoCleanUp feature, just add the Scheduler.cs to a GameObject in your scene. You can also set the cleanup interval in the Editor.	

6. If you want to get rid of all your pooled GameObjects from your pool, and the Pool itself, just call DisablePool() on your ObjectPool instance.

7. There is a prim and sec variable in the ObjectPool class. Use these to visualize your pools. If you not using them it's a good idea to comment them out.
