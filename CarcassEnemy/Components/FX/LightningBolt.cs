using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Components.FX
{
    public class LightningBolt : MonoBehaviour
    {
        [SerializeField] private int resolution;
        [SerializeField] private float bend;
        [SerializeField] private float maxVariance;
        [SerializeField] private Light lightTemplate;
        [SerializeField] private LineRenderer lineRenderer;

        public Transform[] Points;

        private Light[] activeLights;

        private void Update()
        {
            if (Points == null)
                return;

            if (Points.Length < 2)
                return;

            SolveLine();
        }

        private void SolveLine()
        {

            Vector3[] basePoints = Points.Select(x => x.transform.position).ToArray();

            int pointCount = basePoints.Length*resolution;
            Vector3[] linePoints = new Vector3[pointCount];

            for(int i = 0; i < basePoints.Length; i++)
            {

            }

            if (lightTemplate != null)
                SolveLights(basePoints);
        }

        private Vector3[] SolveArc(Vector3 origin, Vector3 end)
        {
            int pointCount = resolution + 2;
            Vector3[] arc = new Vector3[pointCount];

            arc[0] = origin;
            arc[pointCount - 1] = end;

            //(r/0.5f) * i =

            for(int i = 1; i < arc.Length-1; i++)
            {

            }

            return arc;
        }

        private void SolveLights(Vector3[] points)
        {
            if(activeLights == null)
                activeLights = new Light[points.Length];

            if (activeLights.Length != points.Length)
            {
                DestroyLights();
                activeLights = new Light[points.Length];
            }

            for(int i = 0; i < points.Length; i++)
            {
                if (activeLights[i] == null)
                {
                    activeLights[i] = CreateLight();
                }

                activeLights[i].transform.position = points[i];
            }
        }

        private Light CreateLight()
        {
            return GameObject.Instantiate(lightTemplate, transform);
        }

        private void DestroyLights()
        {
            if (activeLights == null)
                return;

            for(int i = 0; i < activeLights.Length; i++)
            {
                if (activeLights[i] == null)
                    continue;

                Light light = activeLights[i];
                activeLights[i] = null;
                GameObject.Destroy(light.gameObject);
            }


            activeLights = null;
        }
    }
}
