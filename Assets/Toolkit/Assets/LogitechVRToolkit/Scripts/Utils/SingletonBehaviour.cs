namespace Logitech.XRToolkit.Utils
{
    using UnityEngine;

    /// <summary>
    /// Behaviours inheriting from this will automatically be available as singleton through their Instance static field
    /// </summary>
    /// <typeparam name="T">Your class inheriting from this one.</typeparam>
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        /// <summary>
        /// The only instance of this object. Will complain if there is more than one in your scene.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var instances = GameObject.FindObjectsOfType<T>();
                    if (instances == null || instances.Length != 1)
                    {
                        Debug.LogErrorFormat("There should be exactly one instance of singleton type {0} in your scene", typeof(T));
                        return null;
                    }
                    else
                    {
                        _instance = instances[0];
                    }
                }
                return _instance;
            }
        }

        private static T _instance = null;

        protected virtual void Awake()
        {
            Debug.Assert(Instance == this, "There seems to be an issue with singleton initialization (multiple objects?)");
            var parent = this.transform;
            while (parent.parent != null)
            {
                parent = parent.parent;
            }
            DontDestroyOnLoad(parent.gameObject);
        }

        protected virtual void OnDestroy()
        {
            Debug.Assert(Instance != null);
            if (Instance == this)
            {
                _instance = null;
            }
        }
    }
}
