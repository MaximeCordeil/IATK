using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IATK;
using UnityEditor;

public class AttributeAnimation : MonoBehaviour {

    [SerializeField]
    public Visualisation vis;    
    [SerializeField]
    public float timeScale;
    [SerializeField]
    public float rangeSize;
    [SerializeField]
    public bool animateRange;

    public KeyCode startAnimationBinding;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(startAnimationBinding)) Tween();
    }

    void animateMaxFilter(int attribute, float t)
    {
        vis.attributeFilters[0].maxFilter = t;
    }

    void animateMinFilter(int attribute, float t)
    {
        vis.attributeFilters[0].minFilter = t;
    }

    // handle animations
    float _tween = 0.0f;

    public void Tween()
    {
        _tween = 0.0f;
        vis.attributeFilters[0].maxFilter = _tween;
#if UNITY_EDITOR
        EditorApplication.update = DoTheTween;
#endif
    }

    private void DoTheTween()
    {
        _tween += Time.deltaTime * timeScale;
        if (_tween < 1.0f)
        {
            float v = Mathf.Pow(_tween, 3) * (_tween * (6f * _tween - 15f) + 10f);
            vis.attributeFilters[0].maxFilter = v;
            if(animateRange)
            vis.attributeFilters[0].minFilter = v-rangeSize;

            
        }
        else
        {
            _tween = 1.0f;
            vis.attributeFilters[0].maxFilter = 1f;
            if (animateRange)
            vis.attributeFilters[0].minFilter = 1f-rangeSize;
#if UNITY_EDITOR
            EditorApplication.update = null;
#endif
        }

        vis.updateViewProperties(AbstractVisualisation.PropertyType.AttributeFiltering);
        vis.updateViewProperties(AbstractVisualisation.PropertyType.SizeValues);


    }
}
