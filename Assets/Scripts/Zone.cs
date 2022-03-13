using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    [System.Serializable]
    public class Zone 
    {
     
        [SerializeField] public String zoneName;
        public int zoneIndexId;
        
        public Vector3 gridSize;

        public  Vector3 gridPositionOffset;
        [SerializeField] public GridInfo gridInfo;
        [SerializeField] public WallSettings wallSettings;
        public List<LevelData> _levelData = new List<LevelData>();
        
        [JsonIgnore]
        public List<FloorSquareData> levelTiles;

       
        //INNERCLASSES
        [System.Serializable]
        public class GridInfo
        {
            [SerializeField] public float totalGridWidth;
            [SerializeField] public float totalGridLength;
            [SerializeField] public float singleGridWidth;
            [SerializeField] public float singleGridLength;

            public float x_Space = 1, y_Space = 1;
            [SerializeField] public int columns, rows;
        }


        [System.Serializable]
        public class WallSettings
        {

            public Boolean enabled = true;
            public Boolean drawBackWall = true;
            public Boolean drawFrontWall = true;
            public Boolean drawSideWalls = true;
            public int wallHeight = 5;
            public int offset = 1;

        }

        public void Init()
        {
            levelTiles = new List<FloorSquareData>();
        }
        
        public void Start(GridManagerV2 mgr)
        {
            //gridPositionOffset = mgr.transform.position;

          
            gridPositionOffset += mgr.transform.position;
            gridInfo.totalGridWidth =mgr.prefab.transform.localScale.x * gridInfo.columns;
            gridInfo.totalGridLength = mgr.prefab.transform.localScale.z * gridInfo.rows;
            gridInfo.singleGridLength = mgr.prefab.transform.localScale.z;
            gridInfo.singleGridWidth = mgr.prefab.transform.localScale.x;
            
            
            for (int i = 0; i < gridInfo.columns * gridInfo.rows; i++)
            {
                GameObject go = mgr.CreateObject(mgr.prefab,
                    new Vector3(gridInfo.x_Space * (i % gridInfo.columns) + gridPositionOffset.x,
                        0 + gridPositionOffset.y, gridInfo.y_Space * (i / gridInfo.columns) + gridPositionOffset.z));
                go.AddComponent<FloorSquareData>();
                levelTiles.Add(go.GetComponent<FloorSquareData>());
                levelTiles.Last().id = i;
                levelTiles.Last().setXPos(i % gridInfo.columns);
                levelTiles.Last().setYPos(i / gridInfo.columns);
                levelTiles.Last().setZoneId(zoneIndexId);
                 
            }
            gridSize = mgr.prefab.transform.localScale;
        }

        public void DeleteFloorData(int objIndex)
        {
            Debug.Log($"Delete Data");
            levelTiles[objIndex].DeleteObjectData();
            for (int i = 0; i < _levelData.Count; i++)
            {
                if (_levelData[i].gridListPos == objIndex)
                {
                    Debug.Log("Delete Level Data");
                    _levelData.Remove(_levelData[i]);
                }
            }
        }

        public void AddToLevelData(int objIndex, Vector3 tmpVector, int selectedOption)
        {
            LevelData levelData = new LevelData();
            levelData.pos = tmpVector;
            levelData.objectIndex = selectedOption;
            levelData.gridListPos = objIndex;
            _levelData.Add(levelData);
        }

        public void SpawnCheckPoint(GridManagerV2 mgr,int objIndex, int selectedOption, List<GameObject> spawnableList, bool addToLevel)
        {
            if (levelTiles[objIndex].CheckForCheckPoint()) return;
            Vector3 tmpVector = levelTiles[objIndex].transform.position;
            GameObject chkPoint = mgr.CreateObject(spawnableList[selectedOption],
                new Vector3(tmpVector.x, tmpVector.y + 1, tmpVector.z + ((float) gridInfo.rows / 2) - 1));
            
            
                    
            levelTiles[objIndex].SetCheckPoint(chkPoint);
            chkPoint.transform.localScale = new Vector3(gridSize.x, gridSize.y, gridSize.z * gridInfo.rows);
            if(addToLevel)
                AddToLevelData(objIndex, tmpVector, selectedOption);
        }


        public void RenderWalls(GridManagerV2 mgr)
        {

            if (wallSettings.enabled)
            {
                if (wallSettings.drawBackWall)
                {
                    for (int c = 0; c < gridInfo.columns; c++)
                    {
                        for (int count = 0; count < wallSettings.wallHeight; count++)
                        {
                            mgr.CreateObject(mgr.edge,
                                new Vector3(
                                    (gridPositionOffset.x) + (c * mgr.prefab.transform.localScale.x),
                                    wallSettings.offset + (mgr.prefab.transform.localScale.y * count),
                                    gridPositionOffset.z + gridInfo.y_Space * (c / gridInfo.columns) + gridInfo.totalGridLength));


                        }
                    }
                }

                if (wallSettings.drawFrontWall)
                {
                    for (int c = 0; c < gridInfo.columns; c++)
                    {
                        for (int count = 0; count < wallSettings.wallHeight; count++)
                        {
                            mgr.CreateObject(mgr.edge,
                                new Vector3((gridPositionOffset.x) + (c * mgr.prefab.transform.localScale.x),
                                    wallSettings.offset + (mgr.prefab.transform.localScale.y * count),
                                    gridPositionOffset.z + gridInfo.y_Space * (c / gridInfo.columns) -
                                    mgr.prefab.transform.localScale.z));
                            
                            
                        }
                    }
                }

                if (wallSettings.drawSideWalls)
                {
                    for (int c = 0; c < gridInfo.rows; c++)
                    {
                        for (int count = 0; count < wallSettings.wallHeight; count++)
                        {

                            mgr.CreateObject(mgr.edge,
                                new Vector3(
                                    (gridPositionOffset.x) + gridInfo.totalGridWidth,
                                    wallSettings.offset + (mgr.prefab.transform.localScale.y * count),
                                    gridPositionOffset.z + (c / gridInfo.rows) + c));
  
                        }

                        for (int count = 0; count < wallSettings.wallHeight; count++)
                        {
                            mgr.CreateObject(mgr.edge,
                                new Vector3(
                                    (gridPositionOffset.x) - mgr.prefab.transform.localScale.x,
                                    wallSettings.offset + (mgr.prefab.transform.localScale.y * count),
                                    gridPositionOffset.z + (c / gridInfo.rows) + c));
           
                        }
                    }
                }
            }


        }
    }


}