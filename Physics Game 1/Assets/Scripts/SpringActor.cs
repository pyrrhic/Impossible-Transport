using UnityEngine;
using System.Collections;
using System;

public class SpringActor : MonoBehaviour, IMovableNode {
    public Transform StartPos;
    public Transform EndPos;
    public SpringJoint2D SpringJoint { get; set; }
    [SerializeField]
    float damping = 0.5f;
    [SerializeField]
    float frequency = 10f;

	void Start () {

        //only useful for the build mode for moving.
        if (StartPos != null && EndPos != null) {
            return;
        }

        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name.Equals("bottom")) {
                StartPos = children[i];
            }
            else if (children[i].name.Equals("top")) {
                EndPos = children[i];
            }
        }
    }
	
	void Update () {
        Vector2 midpoint = Vector2.Lerp(StartPos.position, EndPos.position, 0.5f);
        transform.position = midpoint;

        //Rotate
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, EndPos.position - StartPos.position);
        transform.rotation = rotation;
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

        //Scale the spring.
        //scale needed = distance x units per pixel / sprite pixel size
        float distance = Vector2.Distance(StartPos.position, EndPos.position);
        BoxCollider2D wireCollider = GetComponentInChildren<BoxCollider2D>();
        float spritePixelSize = wireCollider.gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.y * 100;

        wireCollider.gameObject.transform.localScale = new Vector3(transform.localScale.x, distance * 100 / spritePixelSize, transform.localScale.z);

        //scale the spring joint
        //currentSpring.GetComponent<SpringJoint2D>().distance = distance / 2f;

        //Move the nodes.
        float fTop = wireCollider.offset.y + (wireCollider.size.y / 2f);
        float fBottom = wireCollider.offset.y - (wireCollider.size.y / 2f);
        //float fLeft = woodCollider.offset.x - (woodCollider.size.x / 2f);
        //float fRight = woodCollider.offset.x + (woodCollider.size.x / 2f);

        Transform bottomCircle = null;
        Transform topCircle = null;
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name.Equals("bottom")) {
                bottomCircle = children[i];
            }
            else if (children[i].name.Equals("top")) {
                topCircle = children[i];
            }
        }

        bottomCircle.position = wireCollider.gameObject.transform.TransformPoint(new Vector3(0f, fBottom, bottomCircle.position.z));
        topCircle.position = wireCollider.gameObject.transform.TransformPoint(new Vector3(0f, fTop, 0f));

        Transform springBottom = null;
        Transform springTop = null;
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name.Equals("spring bottom")) {
                springBottom = children[i];
            }
            else if (children[i].name.Equals("spring top")) {
                springTop = children[i];
            }
        }

        springBottom.position = bottomCircle.position;
        springTop.position = topCircle.position;

        if (SpringJoint) {
            SpringJoint.dampingRatio = damping;
            SpringJoint.frequency = frequency;
            SpringJoint.anchor = StartPos.parent.InverseTransformPoint(StartPos.position);
            SpringJoint.connectedAnchor = EndPos.parent.InverseTransformPoint(EndPos.position);
        }
    }
    
    void IMovableNode.UpdateMove() {
        Update();
    }
}
