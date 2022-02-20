using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class GridManagerV2 : MonoBehaviour
    {
        private float x_Space =1, y_Space =1;
        [SerializeField] public int columns, rows;
        [SerializeField] public GameObject prefab;
        [SerializeField] public GameObject edge;

        public Vector3 gridSize;
        // private List<GameObject> _spawnablePrefabs = new List<GameObject>();
        private TextFileHandler _textFileHandler;
        [SerializeField] private string sceneFileName = "Scene1";
        
        [FormerlySerializedAs("weaponList")] [SerializeField] public List<GameObject> spawnableList;
        [SerializeField] private bool showSelection = true;
        
        public List<FloorSquareData> levelTiles = new List<FloorSquareData>();
        public Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);
        private List<LevelData> _levelData = new List<LevelData>();

        private int selectedOption = 0;
        
        private void Start()
        {
            // TODO add edge around floor
            // _spawnablePrefabs.AddRange(spawnableList);
            for (int c = 0; c < columns; c++)
            {
                Instantiate(edge, new Vector3(x_Space * (c % columns),0,y_Space * (c / columns)), Quaternion.identity);
                
            }
            for (int r = 0; r < rows; r++)
            {
                Instantiate(edge, new Vector3(x_Space * (r / rows),0,y_Space * r), Quaternion.identity); 
            }
            
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
            
            _textFileHandler = new TextFileHandler(sceneFileName);
            
            (bool exists, string fileText) = _textFileHandler.GetFileText();
            if (exists)
            {
                List<LevelData> levelData = JsonConvert.DeserializeObject<List<LevelData>>(fileText);
                foreach (LevelData data in levelData)
                {
                    selectedOption = data.objectIndex;
                    SpawnObject(data.gridListPos);
                    // Instantiate(_spawnablePrefabs[], data.pos, Quaternion.identity);
                }

            }
            
        }

        private void OnApplicationQuit()
        {
            _textFileHandler.AddTextToFile(JsonConvert.SerializeObject(_levelData));
        }

        void Update()
        {
            var pos = GetXZ();
            if (Input.GetMouseButtonDown(0) && pos.x >= 0 && pos.z >= 0)
            {
                
                if (selectedOption == -1)
                {
                    // Delete
                    int objIndex = GetPositionFromXZ(pos.x, pos.z);
                    DeleteFloorData(objIndex);
                }
                else if (selectedOption == 4)
                {
                    // Checkpoint
                    int objIndex = GetPositionFromXZ(GetXZ().x, 0);
                    SpawnCheckPoint(objIndex);
                }
                else
                {
                    SpawnFloorWeapon(GetPositionFromXZ(pos.x, pos.z));
                }
            }

            if (pos.x >= 0 && pos.x <= columns)
            {
                foreach (var tile in levelTiles)
                {
                    if (!tile.CheckForWeapon())
                    {
                        if (GetPositionFromXZ(pos.x, pos.z) == tile.id)
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
                        if (selectedOption == -1 && GetPositionFromXZ(pos.x, pos.z) == tile.id) tile.renderer.material.color = Color.red;
                        else
                        {
                            tile.renderer.material.color = Color.grey;
                        }
                    }
                }
                
            }
            
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

        }

        private void DeleteFloorData(int objIndex)
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

        private void SpawnCheckPoint(int objIndex)
        {
            if (levelTiles[objIndex].CheckForCheckPoint()) return;
            Vector3 tmpVector = levelTiles[objIndex].transform.position;
            GameObject chkPoint =
                Instantiate(spawnableList[selectedOption], new Vector3(tmpVector.x, tmpVector.y + 1, tmpVector.z + (rows / 2) - 1),
                    Quaternion.identity);
            levelTiles[objIndex].SetCheckPoint(chkPoint);
            Debug.Log($"Spawn Check Point {gridSize.ToString()}");
            chkPoint.transform.localScale = new Vector3(gridSize.x, gridSize.y, gridSize.z * rows);
            AddToLevelData(objIndex, tmpVector);
        }

        private void AddToLevelData(int objIndex, Vector3 tmpVector)
        {
            LevelData levelData = new LevelData();
            levelData.pos = tmpVector;
            levelData.objectIndex = selectedOption;
            levelData.gridListPos = objIndex;
            _levelData.Add(levelData);
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

        void SpawnObject(int index)
        {
            if (selectedOption == 4)
            {
                SpawnCheckPoint(index);
            }
            else
            {
                SpawnFloorWeapon(index);
            }
        }
        
        public void SpawnFloorWeapon(int element)
        {
            Vector3  selectedTransform = levelTiles[element].transform.position;
            float f = spawnableList[selectedOption].transform.localPosition.y;
            selectedTransform = new Vector3(selectedTransform.x, selectedTransform.y + f, selectedTransform.z);
            if (!levelTiles[element].CheckForWeapon())
            {

               levelTiles[element].SetWeapon(Instantiate(spawnableList[selectedOption],selectedTransform, Quaternion.identity));

            }
            AddToLevelData(element, selectedTransform);
        }
        
        
    }
}