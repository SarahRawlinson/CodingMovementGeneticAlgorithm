using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ITestForTarget
{
    (bool, GameObject) TestForTarget(Collider collider, List<GameObject> gameObjects);
}

