using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPoolItem {
	[Header ("Set in Inspector")]
	public GameObject 			objectToPool;
	public int 					amountToPool;
	public bool					shouldExpand;
}

public class Pool : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<ObjectPoolItem> itemsToPool;

	public Transform			poolAnchor;

	[Header("Set Dynamically")]
	public static Pool 	S; 
	public List<GameObject> 	pooledObjects;

	private static bool			alreadyPopulated; // Prevents repopulating ObjectPoolAnchor every level load

	void Start(){ // changing to "void Awake()" results in repopulating poolAnchor every scene change
		// Singleton
		S = this;

		// Pool List (on MainCamera)
		pooledObjects = new List<GameObject> ();
		foreach (ObjectPoolItem item in itemsToPool) {
			for (int i = 0; i < item.amountToPool; i++) {
				GameObject obj = (GameObject)Instantiate (item.objectToPool);
				obj.SetActive (false);
				pooledObjects.Add (obj);
				obj.transform.SetParent (poolAnchor);
			}
		}
	}
		
	public GameObject GetPooledObject(string tag){
		for (int i = 0; i < pooledObjects.Count; i++) {
			if (!pooledObjects [i].activeInHierarchy && pooledObjects[i].tag == tag) {
				return pooledObjects [i];
			}
		}
		foreach (ObjectPoolItem item in itemsToPool) {
			if(item.objectToPool.tag == tag){				
				if (item.shouldExpand) {
					GameObject obj = (GameObject)Instantiate (item.objectToPool);
					obj.SetActive (false);
					pooledObjects.Add (obj);
					return obj;
				}
			} 
		}return null;
	}
		
	public void PosAndEnableObj(GameObject tObj, Vector3 tPos, Vector3 tRot = default(Vector3), Transform tAnchor = null){
		if (tObj != null) {
			tObj.transform.localPosition = tPos;
			tObj.transform.rotation = Quaternion.Euler (tRot);
			if (tAnchor != null) {
				tObj.transform.SetParent (tAnchor);
			}
			tObj.SetActive (true);
		}
	}
}