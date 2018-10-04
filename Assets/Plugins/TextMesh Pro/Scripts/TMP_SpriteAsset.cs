// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System.Collections.Generic;


namespace TMPro
{

    public class TMP_SpriteAsset : TMP_Asset
    {
        /// <summary>
        /// Static reference to the default font asset included with TextMesh Pro.
        /// </summary>
        public static TMP_SpriteAsset defaultSpriteAsset
        {
            get
            {
                if (m_defaultSpriteAsset == null)
                {
                    m_defaultSpriteAsset = Resources.Load<TMP_SpriteAsset>("Sprite Assets/Default Sprite Asset");
                }

                return m_defaultSpriteAsset;
            }
        }
        public static TMP_SpriteAsset m_defaultSpriteAsset;
        
        
        // The texture which contains the sprites.
        public Texture spriteSheet;

        // List which contains the SpriteInfo for the sprites contained in the sprite sheet.
        public List<TMP_Sprite> spriteInfoList;

        // List which contains the individual sprites.
        private List<Sprite> m_sprites;


        //private bool isEditingAsset;

        void OnEnable()
        {
            // Make sure we have a valid material.
            //if (this.material == null && !isEditingAsset)
            //   this.material = GetDefaultSpriteMaterial();
        }


#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        void OnValidate()
        {

            TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, this);
        }
#endif


        /// <summary>
        /// Create a material for the sprite asset.
        /// </summary>
        /// <returns></returns>
        Material GetDefaultSpriteMaterial()
        {
            //isEditingAsset = true;
            ShaderUtilities.GetShaderPropertyIDs();

            // Add a new material
            Shader shader = Shader.Find("TextMeshPro/Sprite");
            Material tempMaterial = new Material(shader);
            tempMaterial.SetTexture(ShaderUtilities.ID_MainTex, spriteSheet);
            tempMaterial.hideFlags = HideFlags.HideInHierarchy;

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(tempMaterial, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
#endif
            //isEditingAsset = false;

            return tempMaterial;
        }


        /// <summary>
        /// Function which returns the sprite index using the hashcode of the name
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public int GetSpriteIndex(int hashCode)
        {
            for (int index = 0; index < spriteInfoList.Count; index++)
            {
                if (spriteInfoList[index].hashCode == hashCode)
                    return index;
            }

            return -1;
        }



#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        public void LoadSprites()
        {
            if (m_sprites != null && m_sprites.Count > 0)
                return;

            Debug.Log("Loading Sprite List");
            
            string filePath = UnityEditor.AssetDatabase.GetAssetPath(spriteSheet);

            Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

            m_sprites = new List<Sprite>();

            foreach (Object obj in objects)
            {
                if (obj.GetType() == typeof(Sprite))
                {
                    Sprite sprite = obj as Sprite;
                    Debug.Log("Sprite # " + m_sprites.Count + " Rect: " + sprite.rect);
                    m_sprites.Add(sprite);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Sprite> GetSprites()
        {
            if (m_sprites != null && m_sprites.Count > 0)
                return m_sprites;

            //Debug.Log("Loading Sprite List");

            string filePath = UnityEditor.AssetDatabase.GetAssetPath(spriteSheet);

            Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

            m_sprites = new List<Sprite>();

            foreach (Object obj in objects)
            {
                if (obj.GetType() == typeof(Sprite))
                {
                    Sprite sprite = obj as Sprite;
                    //Debug.Log("Sprite # " + m_sprites.Count + " Rect: " + sprite.rect);
                    m_sprites.Add(sprite);
                }
            }

            return m_sprites;
        }
#endif
      
    }
}
