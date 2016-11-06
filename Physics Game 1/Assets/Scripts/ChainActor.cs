using UnityEngine;
using System.Collections;

public class ChainActor : MonoBehaviour {
    public Transform StartPos;
    public Transform EndPos;
    
    void Start () {
	
	}
	
    public void SetStartAndEnd() {
        Transform chainStart = null;
        Transform chainEnd = null;
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name.Equals("chain start")) {
                chainStart = children[i];
            }
            else if (children[i].name.Equals("chain end")) {
                chainEnd = children[i];
            }
        }

        StartPos = (GetNodeOverPosition(chainStart.position.x, chainStart.position.y)).transform;
        EndPos = (GetNodeOverPosition(chainEnd.position.x, chainEnd.position.y)).transform;
    }

    GameObject GetNodeOverPosition(float x, float y) {
        RaycastHit2D hitInfo = Physics2D.Raycast(new Vector2(x, y),
                                                 Vector2.zero,
                                                 0);
        if (hitInfo) {
            if (hitInfo.collider.gameObject.CompareTag("Node")) {
                return hitInfo.collider.gameObject;
            }
        }

        return null;
    }

    void Update () {
        if (StartPos == null || EndPos == null) {
            return;
        }

        Vector3 startPosition = StartPos.position;
        Vector3 mousePosition = EndPos.position;
        
        Vector2 midpoint = Vector2.Lerp(startPosition, mousePosition, 0.5f);
        transform.position = midpoint;

        //Rotate
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - transform.position);
        transform.rotation = rotation;
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

        //Scale the spring.
        //scale needed = distance x units per pixel / sprite pixel size
        float distance = Vector2.Distance(startPosition, mousePosition);
        GameObject chainMiddle = null;
        Transform[] chainChildren = GetComponentsInChildren<Transform>();
        for (int i = 0; i < chainChildren.Length; i++) {
            if (chainChildren[i].name.Equals("chain middle")) {
                chainMiddle = chainChildren[i].gameObject;
                break;
            }
        }
        float spritePixelSize = chainMiddle.GetComponent<SpriteRenderer>().sprite.bounds.size.y * 100;

        chainMiddle.transform.localScale = new Vector3(transform.localScale.x, distance * 100 / spritePixelSize, transform.localScale.z);

        //scale the spring joint
        //currentSpring.GetComponent<SpringJoint2D>().distance = distance / 2f;

        //Move the ends.
        BoxCollider2D chainCollider = chainMiddle.GetComponent<BoxCollider2D>();
        float fTop = chainCollider.offset.y + (chainCollider.size.y / 2f);
        float fBottom = chainCollider.offset.y - (chainCollider.size.y / 2f);
        //float fLeft = woodCollider.offset.x - (woodCollider.size.x / 2f);
        //float fRight = woodCollider.offset.x + (woodCollider.size.x / 2f);

        Transform chainStart = null;
        Transform chainEnd = null;
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name.Equals("chain start")) {
                chainStart = children[i];
            }
            else if (children[i].name.Equals("chain end")) {
                chainEnd = children[i];
            }
        }

        chainStart.position = chainCollider.gameObject.transform.TransformPoint(new Vector3(0f, fBottom, chainStart.position.z)); ;
        chainEnd.position = chainCollider.gameObject.transform.TransformPoint(new Vector3(0f, fTop, 0f));
    }
}
