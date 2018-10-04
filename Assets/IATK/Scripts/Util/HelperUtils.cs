using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace IATK
{

    /// <summary>
    /// Helper utils.
    /// </summary>
    public static class HelperUtils
    {

        /// <summary>
        /// Gets the component in children with tag.
        /// </summary>
        /// <returns>The component in children with tag.</returns>
        /// <param name="gameObject">Game object.</param>
        /// <param name="tag">Tag.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetComponentInChildrenWithTag<T>(this GameObject gameObject, string tag)
        {
            if (gameObject.CompareTag(tag))
            {
                return gameObject.GetComponent<T>();
            }
            else
            {
                foreach (Transform child in gameObject.transform)
                {
                    T result = child.gameObject.GetComponentInChildrenWithTag<T>(tag);
                    if (!EqualityComparer<T>.Default.Equals(result, default(T)))
                    {
                        return result;
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Gets the components in children with tag.
        /// </summary>
        /// <returns>The components in children with tag.</returns>
        /// <param name="gameObject">Game object.</param>
        /// <param name="tag">Tag.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[] GetComponentsInChildrenWithTag<T>(this GameObject gameObject, string tag)
        {
            List<T> componentList = new List<T>();

            GetComponentsInChildrenWithTagImpl(gameObject, tag, componentList);

            return componentList.ToArray();
        }

        /// <summary>
        /// Gets the components in children with tag impl.
        /// </summary>
        /// <param name="gameObject">Game object.</param>
        /// <param name="tag">Tag.</param>
        /// <param name="componentList">Component list.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        private static void GetComponentsInChildrenWithTagImpl<T>(GameObject gameObject, string tag, List<T> componentList)
        {
            if (gameObject.CompareTag(tag))
            {
                componentList.Add(gameObject.GetComponent<T>());
            }
            else
            {
                foreach (Transform child in gameObject.transform)
                {
                    GetComponentsInChildrenWithTagImpl<T>(child.gameObject, tag, componentList);
                }
            }
        }
        /// <summary>
        /// selects a sub array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            int index2 = index;
            T[] result = new T[length];
            if (index > 0) index2 -= 1;
            Array.Copy(data, index2, result, 0, length);
            return result;
        }
    }

}   // Namespace