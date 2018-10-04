using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IATK;

namespace IATK
{
    public class Animation: MonoBehaviour
    {
        public enum ANIMATION_TYPE { POSITION1D, POSITION2D, POSITION3D, COLOR };
        private float duration;
        private bool start = false;
        float previousTime = 0f;
        
        public void Start()
        { }

        public void Update()
        {
            if (start)
            {
                float time = Time.time;
                float deltaTime = previousTime - time;

                previousTime = time;
                if (Time.time > duration)
                    start = false;
            }
        }

        public Vector3[] LerpVector3Array(float[] xSource, float[] ySource, float[] zSource, float[] xDest, float[] yDest, float[] zDest, float time)
        {
            Vector3[] lerpedPositions = new Vector3[xSource.Length];

            for (int i = 0; i < xSource.Length; i++)
                lerpedPositions[i] = Vector3.Lerp(new Vector3(xSource[i], ySource[i], zSource[i]),
                                                  new Vector3(xDest[i], yDest[i], zDest[i]), time);
            return lerpedPositions;
        }

    }
}
