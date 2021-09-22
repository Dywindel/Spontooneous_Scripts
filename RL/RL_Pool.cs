using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used to generate pool queues for common
// Gameobjects, without the need to repeatedly instantiate
// And destroy them.

public class RL_Pool : MonoBehaviour
{
    // Read up about what a singleton is and does
    // I am not sure what this is exactly doing, but it means
    // I can reference RL_Pool without always needing to use GetComponent
    #region Singleton

    public static RL_Pool Instance;
    private void Awake()
    {
        Instance = this;
    }

    #endregion

    // Here, a user can define in the inspector, what type of pool
    // Info the game uses
    [System.Serializable] public class Pool
    {
        public BridgeType bridgeType;
        public GameObject perfab;
        public int size;
    }

    public List<Pool> pools;
    // This is a way to use a reference string to load a particular
    // Object pool by name
    public Dictionary<BridgeType, Queue<GameObject>> dictPool;

    void Start()
    {
        dictPool = new Dictionary<BridgeType, Queue<GameObject>>();

        // Go through every pool the user has listed
        foreach (Pool pool in pools)
        {
            Queue<GameObject> poolQueue = new Queue<GameObject>();

            // Create a dictionary reference to each queue
            dictPool.Add(pool.bridgeType, poolQueue);

            // Instantiate every GameObject inside the pool specifications
            for (int i = 0; i < pool.size; i++)
            {
                // Instantiate the object with the GM as its parent
                // Deactivate it and add it to the Queue
                GameObject obj = Instantiate(pool.perfab, transform);
                obj.SetActive(false);
                poolQueue.Enqueue(obj);
            }
        }
    }

    // This function allows us to make an object from the object pool
    // Appear where we need it
    // We also return the GameObject for use by other scripts
    public GameObject SpawnFromPool(BridgeType pBridgeType, Vector3 pPos, Quaternion pRotation, Transform pTransform)
    {
        // Check if the object at the top of the queue is being used
        // We only want to pull an object that isn't active
        GameObject obj = null;
        for (int i = 0; i < dictPool[pBridgeType].Count; i++)
        {
            // Grab an obj from the queue
            obj = dictPool[pBridgeType].Dequeue();
            // If the item is inactive, we can use it, otherwise, put it back
            if (obj.activeSelf)
            {
                dictPool[pBridgeType].Enqueue(obj);
            }
            else
            {
                break;
            }
        }

        // Grab an obj from the queue
        //GameObject obj = dictPool[pPoolType].Dequeue();

        // Activate and place it
        obj.SetActive(true);
        obj.transform.position = pPos;
        obj.transform.rotation = pRotation;

        // Set it's new parent
        obj.transform.parent = pTransform;

        // Make sure we put it back in the queue afterwards
        dictPool[pBridgeType].Enqueue(obj);

        return obj;
    }

    // Return an object to the pool
    public void ReturnToPool(BridgeType pBridgeType, GameObject obj)
    {
        // Deactivate the object
        obj.SetActive(false);
        // Reset it's parent
        obj.transform.parent = transform;
        // Put it back in its related queue
        dictPool[pBridgeType].Enqueue(obj);
    }
}
