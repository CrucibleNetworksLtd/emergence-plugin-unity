using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public abstract class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
    {
        #region Fields

        private static T instance;
        private static readonly object Lock = new object();
        private static bool isCreatingDefaultComponent;

        #endregion

        #region Properties
        public static T Instance
        {
            get
            {
                lock (Lock)
                {
                    if (instance == null)
                    {
                        T[] objectsOfType = FindObjectsOfType(typeof(T)) as T[];
                        if (objectsOfType != null)
                        {
                            if (objectsOfType.Length > 0)
                            {
                                instance = objectsOfType[0];
                            }

                            if (objectsOfType.Length > 1)
                            {
                                return instance;
                            }
                        }

                        if (instance == null)
                        {
                            isCreatingDefaultComponent = true;
                            GameObject singletonGameObject = new GameObject { name = typeof(T).ToString() };
                            instance = singletonGameObject.AddComponent<T>();
                            instance.InitializeDefault();
                            isCreatingDefaultComponent = false;
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void InitializeDefault() { }

        public static bool IsInstanced
        {
            get
            {
                return instance != null;
            }
        }

        #endregion

        #region Initialization
        public virtual void Awake()
        {
            if (!isCreatingDefaultComponent && Instance != this)
            {
                var allcomponents = gameObject.GetComponents<Component>();
                if (allcomponents.Length == 2)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);

                }
            }
        }
        #endregion

        public static T Get()
        {
            return Instance;
        }


    }
}