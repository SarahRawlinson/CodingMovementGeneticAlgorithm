using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    //Todo: Split out things the manager shouldnt handle. i.e the WallSetting 
    //Todo: Add multiple area support
    
    public class GridManagerV2 : MonoBehaviour
    {

        [SerializeField] private List<Zone> zonesList;
        [SerializeField] public List<GameObject> spawnableList;

        [SerializeField] public GameObject prefab;
        [SerializeField] public GameObject edge;
        
        List<GameObject> _spawnablePrefabs = new List<GameObject>();
        private TextFileHandler _textFileHandler;
        [SerializeField] private string sceneFileName = "Scene1";
        [SerializeField] private bool showSelection = true;
        
         private int selectedOption = 0;


        private void Start()
        {

            //add edge around floor
            // _spawnablePrefabs.AddRange(spawnableList);
   
   
            _textFileHandler = new TextFileHandler(sceneFileName);
            
            (bool exists, string fileText) = _textFileHandler.GetFileText();
            if (exists)
            {
                
                zonesList = JsonConvert.DeserializeObject<List<Zone>>(fileText);

                foreach (var zone in zonesList)
                {
                    
                    zone.Init();
                    zone.Start(this);
                    
                    foreach (LevelData data in zone._levelData)
                    {
                        selectedOption = data.objectIndex;
                        SpawnObject(data.gridListPos, zone.zoneIndexId, false);
                  
                    } 
                }
       
                
                
                
                


            }
            else
            {
                
                for (var index = 0; index < zonesList.Count; index++)
                {
                    var z = zonesList[index];
                    z.zoneIndexId = index;
                    z.Start(this);
                }
                
                
            }

            foreach (var zone in zonesList)
            {
            
                zone.RenderWalls(this);
            }

  
            
        }


        public GameObject CreateObject(GameObject prefab, Vector3 position)
        {
            return Instantiate(prefab,position, Quaternion.identity);
        }
        
        
        private void OnApplicationQuit()
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
// This tells your serializer that multiple references are okay.
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            
            _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(zonesList));
        }

        void Update()
        {
            
            var pos = GetXZ();
            Zone selectedZone = zonesList[pos.zoneID];
            if (Input.GetMouseButtonDown(0) && pos.x >= 0 && pos.z >= 0)
            {

                selectedZone = zonesList[pos.zoneID];
                
                if (pos.x < 0 || pos.z < 0) return;
                
                if (selectedOption == -1)
                {
                    // Delete
                    int objIndex = GetIndexFromXZ(pos.x, pos.z, pos.zoneID);
                    selectedZone.DeleteFloorData(objIndex);
                }
                else if (selectedOption == 4)
                {
                    // Checkpoint
                    int objIndex = GetIndexFromXZ(pos.x, 0, pos.zoneID);
                    selectedZone.SpawnCheckPoint(this,objIndex, selectedOption, spawnableList, true);
                }
                else
                {
                    SpawnFloorWeapon(GetIndexFromXZ(pos.x, pos.z, pos.zoneID), pos.zoneID, true);
                }
            }

            if (pos.x >= 0 && pos.x <= selectedZone.gridInfo.columns)
            {
                foreach (var tile in selectedZone.levelTiles)
                {
                    if (!tile.CheckForWeapon())
                    {
                        if (GetIndexFromXZ(pos.x, pos.z, pos.zoneID) == tile.id)
                        {
                            
                            tile.renderer.material.color = Color.green;
                        }
                        else
                        {
                            tile.renderer.material.color = Color.magenta;
                        }
                    }
                    else
                    {
                        if (selectedOption == -1 && GetIndexFromXZ(pos.x, pos.z, pos.zoneID) == tile.id) tile.renderer.material.color = Color.red;
                        else
                        {
                            tile.renderer.material.color = Color.grey;
                        }
                    }
                }
                
            }
            
            HandleOptionSelection();
        }

        public void OnDrawGizmos()
        {

        }

        private void HandleOptionSelection()
        {

            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                selectedOption = -1;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                selectedOption = 0;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                selectedOption = 1;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                selectedOption = 2;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                selectedOption = 3;
            }

            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                selectedOption = 4;
            }

            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                selectedOption = 5;
            }
            //TODO add rotation in
            
            
        }

        private (int x, int z, int zoneID) GetXZ()
        {
            int x = 0;
            int z = 0;
            int zoneID = 0;
            // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // float distance;
            // if (_groundPlane.Raycast(ray, out distance))
            // {
            //     Vector3 worldPosition = ray.GetPoint(distance);
            //     x = Mathf.RoundToInt(worldPosition.x);
            //     z = Mathf.RoundToInt(worldPosition.z);
            //
            //     if (showSelection)
            //     {
            //        Debug.DrawLine(Camera.main.transform.position, worldPosition); 
            //       // Debug.Log("XPos: " + x + " / " + "ZPos:" + z);
            //     }
            //    
            // }

            RaycastHit hit;

            if (Camera.main is { })
            {
                Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
                if (Physics.Raycast (ray, out hit, 100)) {

                    if (hit.transform.TryGetComponent(out FloorSquareData data))
                    {
                        x = data.xPos;
                        z = data.yPos;
                        zoneID = data.zoneId;

                    }
                

                }
            }

            Zone tmpZone = zonesList[zoneID];

    
            
            if( x >= 0 && z >= 0 && x < tmpZone.gridInfo.columns && z < tmpZone.gridInfo.rows )
                return (x,z, zoneID);
            return (-1,-1, zoneID);
        } 
        
        private int GetIndexFromXZ(int x, int z,int zoneId)
        {
            if (x >= 0 && z >= 0)
            {
                Zone tmpZone = zonesList[zoneId];
                return (z * tmpZone.gridInfo.columns) + (x);
            }
            else
            {
                return 0;
            }
        }

        void SpawnObject(int index, int zoneID, bool addToLevel)
        {
            if (selectedOption == 4)
            {
                zonesList[zoneID].SpawnCheckPoint(this,index, selectedOption,spawnableList, addToLevel) ;
            }
            else
            {
                SpawnFloorWeapon(index, zoneID, addToLevel);
            }
        }
        
        public void SpawnFloorWeapon(int element, int zoneId, bool addToLevel)
        {
            Zone tmpZone = zonesList[zoneId];
            
            Vector3  selectedTransform = tmpZone.levelTiles[element].transform.position;
            float f = spawnableList[selectedOption].transform.localPosition.y;
            selectedTransform = new Vector3(selectedTransform.x, selectedTransform.y + f, selectedTransform.z);
            if (!tmpZone.levelTiles[element].CheckForWeapon())
            {

                tmpZone.levelTiles[element].SetWeapon(Instantiate(spawnableList[selectedOption],selectedTransform, Quaternion.identity));

            }
            if(addToLevel)
                tmpZone.AddToLevelData(element, selectedTransform, selectedOption);
        }
        
        
    }
}