using System;
using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public abstract class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
    {
        private static readonly Lazy<T> LazyInstance = new(() =>
        {
            if (FindObjectsOfType(typeof(T)) is T[] { Length: > 0 } objectsOfType)
            {
                return objectsOfType[0];
            }
            
            var singletonGameObject = new GameObject { name = typeof(T).ToString() };
            var instance = singletonGameObject.AddComponent<T>();
            instance.InitializeDefault();
            return instance;
        });



        public static T Instance => LazyInstance.Value;

        protected virtual void InitializeDefault() { }

        public static bool IsInstanced => LazyInstance.IsValueCreated;
        
        public virtual void Awake()
        {
            if (Instance == this) return;
            var allComponents = gameObject.GetComponents<Component>();
            if (allComponents.Length == 2)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }
        
        public static T Get()
        {
            return Instance;
        }
    }
}