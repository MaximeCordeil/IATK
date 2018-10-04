using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

namespace IATK
{
    [ExecuteInEditMode]
    public class DataSourceManager : MonoBehaviour 
    {
        // DATA

        private static List<DataSource> dataSources = new List<DataSource>();

        #if UNITY_EDITOR
        [DidReloadScripts]
        public static void reloadEvent()
        {            
            foreach (DataSource source in dataSources)
            {
                if (!source.IsLoaded)
                source.load();
            }
        }
        #endif

        /// <summary>
        /// Register the specified source.
        /// </summary>
        /// <param name="source">Source.</param>
        public static void register(DataSource source)
        {
            dataSources.Add(source);
        }

        /// <summary>
        /// Unregister the specified source.
        /// </summary>
        /// <param name="source">Source.</param>
        public static void unregister(DataSource source)
        {
            dataSources.Remove(source);
        }

        /// <summary>
        /// Awake this instance.
        /// </summary>
        void Awake()
        {
        }

        /// <summary>
        /// Raises the application quit event.
        /// </summary>
        void OnApplicationQuit()
        {
        }
    }

}   // Namespace