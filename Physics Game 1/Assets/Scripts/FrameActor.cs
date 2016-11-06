using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof (HingeManager))]
public class FrameActor : MonoBehaviour, IMovableNode {
    GameObject topNode;
    GameObject bottomNode;
    GameObject wood;
    HingeManager hingeManager;

    void Start () {
        hingeManager = GetComponent<HingeManager>();

        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name.Equals("bottom")) {
                bottomNode = children[i].gameObject;
            }
            else if (children[i].name.Equals("top")) {
                topNode = children[i].gameObject;
            }
            else if (children[i].name.Equals("wood")) {
                wood = children[i].gameObject;                    
            }
        }
    }
	
	void Update () {
	
	}
    
    void IMovableNode.UpdateMove() {
        GameObject startNode = bottomNode;
        GameObject endNode = topNode;

        Vector2 midpoint = Vector2.Lerp(startNode.transform.position, endNode.transform.position, 0.5f);
        transform.position = midpoint;

        //Rotate
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, endNode.transform.position - startNode.transform.position);
        transform.rotation = rotation;
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

        //Scale the wood.
        //scale needed = distance x units per pixel / sprite pixel size
        BoxCollider2D woodCollider = wood.GetComponent<BoxCollider2D>();
        float distance = Vector2.Distance(startNode.transform.position, endNode.transform.position);
        float spritePixelSize = woodCollider.gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.y * 100;
        woodCollider.gameObject.transform.localScale = new Vector3(transform.localScale.x, distance * 100 / spritePixelSize, transform.localScale.z);

        //Move the nodes.
        float fTop = woodCollider.offset.y + (woodCollider.size.y / 2f);
        float fBottom = woodCollider.offset.y - (woodCollider.size.y / 2f);
        bottomNode.transform.position = woodCollider.gameObject.transform.TransformPoint(new Vector3(0f, fBottom, bottomNode.transform.position.z)); ;
        topNode.transform.position = woodCollider.gameObject.transform.TransformPoint(new Vector3(0f, fTop, topNode.transform.position.z));

        hingeManager.UpdateHingePositions();
    }
}
