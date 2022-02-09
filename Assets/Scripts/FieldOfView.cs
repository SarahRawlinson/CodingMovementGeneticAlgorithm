using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class FieldOfView : MonoBehaviour
{
    public enum Visibility {Visible, CloseRange, Aware, NotVisable, Hear, ActiveRange}
    [SerializeField] Visibility visibility;
    [SerializeField] Color colour;
    [SerializeField] float viewRadius;
    // [SerializeField] List<string> tagToLookFor;
    [SerializeField] private LayerMask _layerMask;
    [Range(0,360)]
    [SerializeField] float viewAngle;
    [SerializeField] Transform eye = null;
    private readonly List<GameObject> _visibleTargets = new List<GameObject>();
    private readonly List<Vector3> _visibleVector3s = new List<Vector3>();
    public float ViewRadius { get => viewRadius; set => viewRadius = value; }
    public float ViewAngle { get => viewAngle; set => viewAngle = value; }
    public List<GameObject> VisibleTargets { get => _visibleTargets;}
    public Transform Eye { get => eye;}
    public Visibility VisibilityType { get => visibility; set => visibility = value; }
    public Color Colour { get => colour; }

    public IEnumerator FindTargetsWithDelay(float delay, ITestForTarget testForTarget)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets(testForTarget);
        }
    }

    public static float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);
 
        if (dir > 0.0f) {
            return 1.0f;
        } else if (dir < 0.0f) {
            return -1.0f;
        } else {
            return 0.0f;
        }
    }  
    

    public (List<GameObject>, List<Vector3>) FindVisibleTargets(ITestForTarget testForTarget)
    {
        Transform hasHit = null;
        _visibleTargets.Clear();
        _visibleVector3s.Clear();
        try
        {
            Collider[] targetsInViewRadius = Physics.OverlapSphere(eye.position, viewRadius);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                // if (!tagToLookFor.Contains(targetsInViewRadius[i].gameObject.tag)) continue;
                Transform target = targetsInViewRadius[i].transform;
                Vector3 directionToTarget = target.position;
                if (targetsInViewRadius[i].gameObject.CompareTag($"Floor"))
                {
                    RaycastHit hit;
                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(eye.transform.position, eye.transform.TransformDirection(Vector3.forward),
                        out hit, viewRadius, _layerMask))
                    {
                         // Debug.DrawRay(eye.transform.position, eye.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                        // Debug.Log("Did Hit");
                        if (hit.collider.CompareTag($"Floor")) hasHit = TestRayTransformForTarget(testForTarget, target, directionToTarget, targetsInViewRadius[i], hit.point);
                    }
                    // else
                    // {
                    //     Debug.DrawRay(eye.transform.position, eye.transform.TransformDirection(Vector3.forward) * 1000, Color.red);
                    // }
                }
                else if (Vector3.Angle(eye.forward, (directionToTarget - eye.position).normalized) < viewAngle / 2)
                {
                    hasHit = TestRayTransformForTarget(testForTarget, target, directionToTarget, targetsInViewRadius[i], target.transform.position);
                }
            }
        }
        catch (System.Exception e)
        {
            // Debug.Log($"No Targets - {e.Message} - {e.TargetSite}");
        }
        return (_visibleTargets, _visibleVector3s);
    }

    private Transform TestRayTransformForTarget(ITestForTarget testForTarget, Transform target, Vector3 directionToTarget, Collider collider, Vector3 hitpos)
    {
        Ray ray = new Ray(eye.position, (hitpos - eye.position).normalized * viewRadius);
        RaycastHit hit;
        Transform hitTransform = null;
        bool hasHit = Physics.Raycast(ray, out hit);
        if (hasHit)
        {
            if (hit.collider != collider)
            {
                return null;
            }
            else
            {

            }
            (bool on, GameObject obj) tple;
            tple = (testForTarget.TestForTarget(collider, _visibleTargets));
            hasHit = tple.on;
            if (hasHit)
            {
                // Debug.DrawLine(eye.transform.position, hit.transform.position);
                _visibleTargets.Add(tple.obj);
                _visibleVector3s.Add(hitpos);
            }
        }
        return hitTransform;
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += eye.eulerAngles.y;
        }
        return eye.rotation * new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }


}

