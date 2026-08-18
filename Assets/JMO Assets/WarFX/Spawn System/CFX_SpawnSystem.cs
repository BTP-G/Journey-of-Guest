using System.Collections.Generic;
using UnityEngine;

// Cartoon FX  - (c) 2012-2016 Jean Moreno

// Spawn System:
// Preload GameObject to reuse them later, avoiding to Instantiate them.
// Very useful for mobile platforms.

public class CFX_SpawnSystem : MonoBehaviour {
    /// <summary>
    /// Rent the next available preloaded UObject.
    /// </summary>
    /// <returns>
    /// The next available preloaded UObject.
    /// </returns>
    /// <param _inputName='sourceObj'>
    /// The _source UObject from which to get a preloaded copy.
    /// </param>
    /// <param _inputName='activateObject'>
    /// Activates the object before returning it.
    /// </param>
    public static GameObject GetNextObject(GameObject sourceObj, bool activateObject = true) {
        var uniqueId = sourceObj.GetInstanceID();

        if (!instance.poolCursors.ContainsKey(uniqueId)) {
            Debug.LogError("[CFX_SpawnSystem.GetNextObject()] Object hasn't been preloaded: " + sourceObj.name + " (ID:" + uniqueId + ")\n", instance);
            return null;
        }

        var cursor = instance.poolCursors[uniqueId];
        GameObject returnObj = null;
        if (instance.onlyGetInactiveObjects) {
            var loop = cursor;
            while (true) {
                returnObj = instance.instantiatedObjects[uniqueId][cursor];
                instance.increasePoolCursor(uniqueId);
                cursor = instance.poolCursors[uniqueId];

                if (returnObj != null && !returnObj.activeSelf) {
                    break;
                }

                //complete loop: no active instance available
                if (cursor == loop) {
                    if (instance.instantiateIfNeeded) {
                        Debug.Log("[CFX_SpawnSystem.GetNextObject()] A new instance has been created for \"" + sourceObj.name + "\" because no active instance were found in the pool.\n", instance);
                        PreloadObject(sourceObj);
                        var list = instance.instantiatedObjects[uniqueId];
                        returnObj = list[^1];
                        break;
                    } else {
                        Debug.LogWarning("[CFX_SpawnSystem.GetNextObject()] There are no active instances available in the pool for \"" + sourceObj.name + "\"\nYou may need to increase the preloaded object durationCount for this prefab?", instance);
                        return null;
                    }
                }
            }
        } else {
            returnObj = instance.instantiatedObjects[uniqueId][cursor];
            instance.increasePoolCursor(uniqueId);
        }

        if (activateObject && returnObj != null) {
            returnObj.SetActive(true);
        }

        return returnObj;
    }

    /// <summary>
    /// Preloads an object a number of times in the stack.
    /// </summary>
    /// <param _inputName='sourceObj'>
    /// The _source UObject.
    /// </param>
    /// <param _inputName='poolSize'>
    /// The number of times it will be instantiated in the stack (i.e. the max number of same object that would appear simultaneously in your Scene).
    /// </param>
    public static void PreloadObject(GameObject sourceObj, int poolSize = 1) {
        instance.addObjectToPool(sourceObj, poolSize);
    }

    /// <summary>
    /// Unloads all the preloaded objects from a _source UObject.
    /// </summary>
    /// <param _inputName='sourceObj'>
    /// Source object.
    /// </param>
    public static void UnloadObjects(GameObject sourceObj) {
        instance.removeObjectsFromPool(sourceObj);
    }

    /// <summary>
    /// Gets a Value indicating whether all objects defined in the Editor are loaded or not.
    /// </summary>
    /// <Value>
    /// <c>true</c> if all objects are loaded; otherwise, <c>false</c>.
    /// </Value>
    public static bool AllObjectsLoaded {
        get {
            return instance.allObjectsLoaded;
        }
    }

    // INTERNAL SYSTEM ----------------------------------------------------------------------------------------------------------------------------------------

    private static CFX_SpawnSystem instance;

    public GameObject[] objectsToPreload = new GameObject[0];
    public int[] objectsToPreloadTimes = new int[0];
    public bool hideObjectsInHierarchy = false;
    public bool spawnAsChildren = true;
    public bool onlyGetInactiveObjects = false;
    public bool instantiateIfNeeded = false;

    private bool allObjectsLoaded;
    private readonly Dictionary<int, List<GameObject>> instantiatedObjects = new Dictionary<int, List<GameObject>>();
    private readonly Dictionary<int, int> poolCursors = new Dictionary<int, int>();

    private void addObjectToPool(GameObject sourceObject, int number) {
        var uniqueId = sourceObject.GetInstanceID();

        //Enqueue new entry if it doesn't exist
        if (!instantiatedObjects.ContainsKey(uniqueId)) {
            instantiatedObjects.Add(uniqueId, new List<GameObject>());
            poolCursors.Add(uniqueId, 0);
        }

        //Enqueue the new objects
        GameObject newObj;
        for (var i = 0; i < number; i++) {
            newObj = Instantiate(sourceObject);
            newObj.SetActive(false);

            //Set Flags to not destruct object
            var autoDestruct = newObj.GetComponentsInChildren<CFX_AutoDestructShuriken>(true);
            foreach (var ad in autoDestruct) {
                ad.OnlyDeactivate = true;
            }
            //Set Flags to not destruct light
            var lightIntensity = newObj.GetComponentsInChildren<CFX_LightIntensityFade>(true);
            foreach (var li in lightIntensity) {
                li.autodestruct = false;
            }

            instantiatedObjects[uniqueId].Add(newObj);

            if (hideObjectsInHierarchy) {
                newObj.hideFlags = HideFlags.HideInHierarchy;
            }

            if (spawnAsChildren) {
                newObj.transform.parent = transform;
            }
        }
    }

    private void removeObjectsFromPool(GameObject sourceObject) {
        var uniqueId = sourceObject.GetInstanceID();

        if (!instantiatedObjects.ContainsKey(uniqueId)) {
            Debug.LogWarning("[CFX_SpawnSystem.removeObjectsFromPool()] There aren't any preloaded object for: " + sourceObject.name + " (ID:" + uniqueId + ")\n", gameObject);
            return;
        }

        //Release all objects
        for (var i = instantiatedObjects[uniqueId].Count - 1; i >= 0; i--) {
            var obj = instantiatedObjects[uniqueId][i];
            instantiatedObjects[uniqueId].RemoveAt(i);
            GameObject.Destroy(obj);
        }

        //Remove stack entry
        instantiatedObjects.Remove(uniqueId);
        poolCursors.Remove(uniqueId);
    }

    private void increasePoolCursor(int uniqueId) {
        instance.poolCursors[uniqueId]++;
        if (instance.poolCursors[uniqueId] >= instance.instantiatedObjects[uniqueId].Count) {
            instance.poolCursors[uniqueId] = 0;
        }
    }

    //--------------------------------

    private void Awake() {
        if (instance != null) {
            Debug.LogWarning("CFX_SpawnSystem: There should only be one instance of CFX_SpawnSystem per Scene!\n", gameObject);
        }

        instance = this;
    }

    private void Start() {
        allObjectsLoaded = false;

        for (var i = 0; i < objectsToPreload.Length; i++) {
            PreloadObject(objectsToPreload[i], objectsToPreloadTimes[i]);
        }

        allObjectsLoaded = true;
    }
}
