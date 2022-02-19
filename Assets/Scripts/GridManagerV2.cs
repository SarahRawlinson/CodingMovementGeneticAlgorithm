using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class GridManagerV2 : MonoBehaviour
    {
        public float x_Space =1, y_Space =1;
        public int columns, rows;
        public GameObject prefab;

        public List<FloorSquareData> levelTiles = new List<FloorSquareData>();
        public Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        private void Start()
        {
            for (int i = 0; i < columns * rows; i++)
            {
                GameObject go = Instantiate(prefab, new Vector3(x_Space * (i % columns),0,y_Space * (i / columns)), Quaternion.identity);
                go.AddComponent<FloorSquareData>();
                levelTiles.Add(go.GetComponent<FloorSquareData>());
                levelTiles.Last().id = i;
                levelTiles.Last().setXPos(i % columns);
                levelTiles.Last().setYPos(i / columns);
                
            }
            
        }

        void Update()
        {
            var pos = GetXZ();
            if (Input.GetMouseButtonDown(0))
            {
                ChangeSelectionColour(GetPositionFromXZ(pos.x,pos.z));
            }
        }

        private (int x, int z) GetXZ()
        {
            int x = 0;
            int z = 0;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance;
            if (_groundPlane.Raycast(ray, out distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);
                x = Mathf.RoundToInt(worldPosition.x);
                z = Mathf.RoundToInt(worldPosition.z);

                Debug.DrawLine(Camera.main.transform.position, worldPosition);
                Debug.LogFormat("Clicked positions: {0} | {1}", x, z);
                              
               
            }
            return (x,z);
        } 
        
        private int GetPositionFromXZ(int x, int z)
        {
            return (z * columns) + (x);
        }
        
        public void ChangeSelectionColour(int element)
        {
            levelTiles[element].renderer.material.color = Color.blue;
        }
        
        
    }
}