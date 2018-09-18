/* Copyright (c) Logitech Corporation. All rights reserved. Licensed under the MIT License.*/

namespace LogiPen.Scripts
{
    using UnityEngine;

    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        public static bool IsInitialized
        {
            get { return Instance != null; }
            set { }
        }

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Debug.LogErrorFormat("Trying to instantiate a second instance of singleton class ");
                return;
            }

            Instance = (T) this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}