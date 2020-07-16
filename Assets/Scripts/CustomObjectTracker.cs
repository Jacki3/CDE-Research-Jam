using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CustomObjectTracker : MonoBehaviour
{

    public ARTrackedObjectManager manager;
    public GameObject labelObj;
    public string[] labelNames;

    [SerializeField]
    private TrackedPrefab[] prefabList;

    private Dictionary<string, GameObject> currentPrefab;

    private void Awake()
    {
        currentPrefab = new Dictionary<string, GameObject>();
    }

    void OnEnable()
    {
        manager.trackedObjectsChanged += OnObjectDetected;
    }

    private void OnDisable()
    {
        manager.trackedObjectsChanged -= OnObjectDetected;
    }

    void OnObjectDetected(ARTrackedObjectsChangedEventArgs eventArgs)
    {
        foreach (ARTrackedObject trackedObject in eventArgs.added)
            CreateGameObject(trackedObject);

        foreach (ARTrackedObject updatedObject in eventArgs.updated)
        {
            if (updatedObject.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
                UpdateTrackingObject(updatedObject);
            else if (updatedObject.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited)
                UpdateLimitedObject(updatedObject);
            else
                UpdateNoObject(updatedObject);
        }

        foreach (ARTrackedObject removedObject in eventArgs.removed)
            DestroyTrackedObject(removedObject);
    }

    void CreateGameObject(ARTrackedObject trackedObject)
    {
        for (int i = 0; i < prefabList.Length; i++)
        {
            if (trackedObject.referenceObject.name == prefabList[i].name)
            {
                GameObject prefab = Instantiate<GameObject>(prefabList[i].prefab, transform.parent);
                prefab.transform.position = trackedObject.transform.position;
                prefab.transform.rotation = trackedObject.transform.rotation;

                currentPrefab.Add(trackedObject.referenceObject.name, prefab);
            }
        }
    }

    void UpdateTrackingObject(ARTrackedObject updatedObject)
    {
        for (int i = 0; i < currentPrefab.Count; i++)
        {
            if (currentPrefab.TryGetValue(updatedObject.referenceObject.name, out GameObject prefab))
            {
                prefab.transform.position = updatedObject.transform.position;
                prefab.transform.rotation = updatedObject.transform.rotation;
                prefab.SetActive(true);
            }
        }
    }

    void UpdateLimitedObject(ARTrackedObject limitedObject)
    {
        for (int i = 0; i < currentPrefab.Count; i++)
        {
            if (currentPrefab.TryGetValue(limitedObject.referenceObject.name, out GameObject prefab))
            {
                if (!prefab.GetComponent<ARTrackedObject>().destroyOnRemoval)
                {
                    prefab.transform.position = limitedObject.transform.position;
                    prefab.transform.rotation = limitedObject.transform.rotation;
                    prefab.SetActive(true);
                }
                else
                    prefab.SetActive(false);
            }
        }
    }

    void UpdateNoObject(ARTrackedObject nullObject)
    {
        for (int i = 0; i < currentPrefab.Count; i++)
        {
            if (currentPrefab.TryGetValue(nullObject.referenceObject.name, out GameObject prefab))
            {
                prefab.SetActive(false);
            }
        }
    }

    void DestroyTrackedObject(ARTrackedObject objectToDestroy)
    {
        for (int i = 0; i < currentPrefab.Count; i++)
        {
            if (currentPrefab.TryGetValue(objectToDestroy.referenceObject.name, out GameObject prefab))
            {
                currentPrefab.Remove(objectToDestroy.referenceObject.name);
                Destroy(prefab);
            }
        }
    }

    [System.Serializable]
    public struct TrackedPrefab
    {
        public string name;
        public GameObject prefab;
    }
}
