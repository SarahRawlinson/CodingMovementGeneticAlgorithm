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
    [SerializeField] List<string> tagToLookFor;
    [Range(0,360)]
    [SerializeField] float viewAngle;
    [SerializeField] Transform eye = null;
    private readonly List<GameObject> _visibleTargets = new List<GameObject>();
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

    public void LookAtTarget(Transform target)
    {
        if (eye == null) return;
        eye.transform.LookAt(target);
    }

    public List<GameObject> FindVisibleTargets(ITestForTarget testForTarget)
    {
        Transform hasHit = null;
        _visibleTargets.Clear();
        try
        {
            Collider[] targetsInViewRadius = Physics.OverlapSphere(eye.position, viewRadius);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                if (!tagToLookFor.Contains(targetsInViewRadius[i].gameObject.tag)) continue;
                Transform target = targetsInViewRadius[i].transform;
                Vector3 directionToTarget = target.position;
                if (Vector3.Angle(eye.forward, (directionToTarget - eye.position).normalized) < viewAngle / 2)
                {
                    hasHit = TestRayTransformForTarget(testForTarget, target, directionToTarget, targetsInViewRadius[i]);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"No Targets - {e.Message} - {e.TargetSite}");
        }
        return _visibleTargets;
    }

    private Transform TestRayTransformForTarget(ITestForTarget testForTarget, Transform target, Vector3 directionToTarget, Collider collider)
    {
        Ray ray = new Ray(eye.position, (directionToTarget - eye.position).normalized * viewRadius);
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
                _visibleTargets.Add(tple.obj);
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

