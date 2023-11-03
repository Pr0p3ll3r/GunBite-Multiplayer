using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    private Queue<GameObject> objects = new Queue<GameObject>();

    public GameObject Get()
    {
        if (objects.Count == 0)
        {
            AddObjects(1);
        }
        GameObject objectToSpawn = objects.Dequeue();
        return objectToSpawn;
    }

    void AddObjects(int amount)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.SetActive(false);
        objects.Enqueue(newObject);
        newObject.transform.parent = transform;
        newObject.GetComponent<IPooledObject>().Pool = this;
    }

    public void ReturnToPool(GameObject objectToReturn)
    {
        objectToReturn.gameObject.SetActive(false);
        objects.Enqueue(objectToReturn);
    }
}
