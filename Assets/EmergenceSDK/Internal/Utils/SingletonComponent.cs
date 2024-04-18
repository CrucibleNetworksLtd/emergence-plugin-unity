using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public abstract class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
    {
        #region Fields

        private static T _instance;
        private static readonly object Lock = new object();
        private static bool _isCreatingDefaultComponent;

        #endregion

        #region Properties
        public static T Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        T[] objectsOfType = FindObjectsOfType(typeof(T)) as T[];
                        if (objectsOfType != null)
                        {
                            if (objectsOfType.Length > 0)
                            {
                                _instance = objectsOfType[0];
                            }

                            if (objectsOfType.Length > 1)
                            {
                                return _instance;
                            }
                        }

                        if (_instance == null)
                        {
                            _isCreatingDefaultComponent = true;
                            GameObject singletonGameObject = new GameObject { name = typeof(T).ToString() };
                            _instance = singletonGameObject.AddComponent<T>();

                            _isCreatingDefaultComponent = false;
                        }
                    }

                    return _instance;
                }
            }
        }

        public static bool IsInstanced
        {
            get
            {
                return _instance != null;
            }
        }

        #endregion

        #region Initialization
        public virtual void Awake()
        {
            if (_isCreatingDefaultComponent == false && Instance != this)
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