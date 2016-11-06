using UnityEngine;
using System.Collections.Generic;

public class HingeManager : MonoBehaviour {
    [SerializeField]
    IDictionary<GameObject, List<HingeJoint2D>> hinges = new Dictionary<GameObject, List<HingeJoint2D>>();

    public float breakForce = 1000;
    public float breakTorque = 1000;

	void Start () {
	
	}
	
	void Update () {
        ICollection<List<HingeJoint2D>> values = hinges.Values;
        foreach (List<HingeJoint2D> hingeList in values) {
            foreach (HingeJoint2D hinge in hingeList) {
                hinge.breakForce = breakForce;
                hinge.breakTorque = breakTorque;
            }
        }
    }

    public void Clear() {
        hinges = new Dictionary<GameObject, List<HingeJoint2D>>();
    }

    public void UpdateHingePositions() {
        ICollection<GameObject> keys = hinges.Keys;
        foreach(GameObject node in keys) {
            Vector3 nodePos = node.transform.parent.InverseTransformPoint(node.transform.position);
            List<HingeJoint2D> hingeList = null;
            hinges.TryGetValue(node, out hingeList);

            for (int i = 0; i < hingeList.Count; i++) {
                HingeJoint2D th = hingeList[i];
                th.anchor = new Vector2(nodePos.x, nodePos.y);
            }
        }
    }

    public void AddHinge(HingeJoint2D hingeJoint, GameObject nodeAddedTo) {
        List<HingeJoint2D> hingeList;
        if (hinges.TryGetValue(nodeAddedTo, out hingeList)) {
            hingeList.Add(hingeJoint);
        }
        else {
            hingeList = new List<HingeJoint2D>();
            hingeList.Add(hingeJoint);
            hinges.Add(nodeAddedTo, hingeList);
        }
    }
}
