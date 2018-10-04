// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Collections;


namespace TMPro.EditorUtilities
{

    public class TMPro_TexturePostProcessor : AssetPostprocessor
    {

        void OnPostprocessTexture(Texture2D texture)
        {
            //var importer = assetImporter as TextureImporter;

            Texture2D tex = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;

            // Send Event Sub Objects
            if (tex != null)
                TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, tex);
        }

    }
}