using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class GridManagerV2 : MonoBehaviour
    {
        private float x_Space =1, y_Space =1;
        public int columns, rows;
        public GameObject prefab;

        public Vector3 gridSize;
        
        [SerializeField] public List<GameObject> weaponList;
        [SerializeField] private bool showSelection = true;
        
        public List<FloorSquareData> levelTiles = new List<FloorSquareData>();
        public Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        private int selectedWeapon = 0;
        
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

            gridSize = prefab.transform.localScale;
            
        }

        void Update()
        {
            var pos = GetXZ();
            if (Input.GetMouseButtonDown(0) && pos.x >= 0 && pos.z >= 0)
            {
                if (selectedWeapon == 4)
                {
                    //checkpoint

                    Vector3 tmpVector = levelTiles[GetPositionFromXZ(GetXZ().x, 0)].transform.position;
                    
                    GameObject chkPoint =
                    Instantiate(weaponList[selectedWeapon],new Vector3(tmpVector.x,tmpVector.y + 1, tmpVector.z + (rows / 2) - 1), Quaternion.identity);

                    chkPoint.transform.localScale = new Vector3(gridSize.x, gridSize.y, gridSize.z * rows );

                }else
                {
                    SpawnFloorWeapon(GetPositionFromXZ(pos.x, pos.z));
                }
            }

            if (pos.x >= 0 && pos.x <= columns)
            {
                foreach (var tile in levelTiles)
                {
                    if (tile.weapon == null)
                    {
                        if (GetPositionFromXZ(pos.x, pos.z) == tile.id)
                        {
                            tile.renderer.material.color = Color.green;
                        }
                        else
                        {
                            tile.renderer.material.color = Color.gray;
                        }
                    }
                }
                
               }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                selectedWeapon = 0;
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                selectedWeapon = 1;
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                selectedWeapon = 2;
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                selectedWeapon = 3;
            }
            
            if (Input.GetKeyDown(KeyCode.F5))
            {
                selectedWeapon = 4;
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

                if (showSelection)
                {
                   Debug.DrawLine(Camera.main.transform.position, worldPosition); 
                  // Debug.Log("XPos: " + x + " / " + "ZPos:" + z);
                }
               
            }
            if( x >= 0 && z >= 0 && x < columns && z < rows )
                return (x,z);
            return (-1,-1);
        } 
        
        private int GetPositionFromXZ(int x, int z)
        {
            if(x >= 0 && z >= 0)
                return (z * columns) + (x);
            else
            {
                return 0;
            }
        }
        
        public void SpawnFloorWeapon(int element)
        {
           Vector3  selectedTransform = levelTiles[element].transform.position;

           if (levelTiles[element].weapon == null)
           {

               levelTiles[element].weapon = Instantiate(weaponList[selectedWeapon],
                   new Vector3(selectedTransform.x, selectedTransform.y + 1, selectedTransform.z), Quaternion.identity);
               
               levelTiles[element].renderer.material.color = Color.magenta;
           }
        }
        
        
    }
}