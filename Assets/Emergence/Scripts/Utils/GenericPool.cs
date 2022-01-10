using UnityEngine;
using System.Collections.Generic;

namespace EmergenceSDK
{
	public class GenericPool<T> where T : new()
	{
		private List<T> freeObjects;
		private List<T> usedObjects;

		private int incrementSize;

		public GenericPool(int initialSize = 10, int incrementSize = 5)
		{
			this.incrementSize = incrementSize;

			freeObjects = new List<T>();
			usedObjects = new List<T>();

			for (int i = 0; i < initialSize; i++)
			{
				T go = new T();

				freeObjects.Add(go);
			}
		}

		public T GetNewObject()
		{
			T go = default(T);

			if (!(freeObjects.Count > 0))
			{
				for (int i = 0; i < incrementSize; i++)
				{
					go = new T();
					freeObjects.Add(go);
				}
			}

			go = freeObjects[0];
			freeObjects.RemoveAt(0);
			usedObjects.Add(go);

			return go;
		}

		public void ReturnUsedObject(ref T go)
		{
			if (usedObjects.Contains(go))
			{
				usedObjects.Remove(go);
				freeObjects.Add(go);
			}
			else
			{
				Debug.LogError("Object " + go + " not used");
			}
		}
	}
}
