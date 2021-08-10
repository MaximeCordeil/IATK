using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using UnityEngine;
using UnityEditor;

namespace IATK
{
    /// <summary>
    /// VR Visualisation class to act as a view controller - reads the model to create the view
    /// </summary>
    [ExecuteInEditMode]
    public class VRVisualisation : Visualisation
    {
        private const string assetName = "Interactions.Interactable";
        private const string assetSuffix = ".prefab";

        new void OnEnable() {
            base.OnEnable();
            CreateInteractable();
        }

        private void CreateInteractable()
        {
            GameObject interactablePrefab = null;

            foreach (string assetGUID in AssetDatabase.FindAssets(assetName))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                if (assetPath.Contains(assetName + assetSuffix))
                {
                    interactablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }
            }

            int siblingIndex = gameObject.transform.GetSiblingIndex();
            GameObject newInteractable = (GameObject)PrefabUtility.InstantiatePrefab(interactablePrefab);
            newInteractable.name += "_" + gameObject.name;
            InteractableFacade facade = newInteractable.GetComponent<InteractableFacade>();

            newInteractable.transform.SetParent(gameObject.transform.parent);
            newInteractable.transform.localPosition = gameObject.transform.localPosition;
            newInteractable.transform.localRotation = gameObject.transform.localRotation;
            newInteractable.transform.localScale = gameObject.transform.localScale;

            foreach (MeshRenderer defaultMeshes in facade.Configuration.MeshContainer.GetComponentsInChildren<MeshRenderer>())
            {
                defaultMeshes.gameObject.SetActive(false);
            }

            gameObject.transform.SetParent(facade.Configuration.MeshContainer.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            newInteractable.transform.SetSiblingIndex(siblingIndex);
        }
    }
}