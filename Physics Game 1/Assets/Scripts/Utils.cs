using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utils {
    public static Utils instance = new Utils();

    public static Utils Instance() { return instance; }

    private Utils() { }

    public GameObject GetNodeOverMouse() {
        return GetNodeOverPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                   Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    }

    public GameObject GetNodeOverNode(GameObject node) {
        GameObject hitNode = GetNodeOverPosition(node.transform.position.x, node.transform.position.y);

        if (hitNode != null && hitNode.GetInstanceID() == node.GetInstanceID()) {
            return null;
        }

        return hitNode;
    }

    public List<GameObject> GetAllNodesOverNode(GameObject node) {
        List<GameObject> hitNodes = GetNodesOverPosition(node.transform.position.x, node.transform.position.y);
        
        for (int i = 0; i < hitNodes.Count; i++) {
            if (hitNodes[i].GetInstanceID() == node.GetInstanceID()) {
                hitNodes.RemoveAt(i);
                break;
            }
        } 
        
        return hitNodes;
    }

    public GameObject GetNodeOverPosition(float x, float y) {
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

    public List<GameObject> GetNodesOverPosition(float x, float y) {
        RaycastHit2D[] hitInformations = Physics2D.RaycastAll(new Vector2(x, y),
                                                         Vector2.zero,
                                                         0);

        List<GameObject> nodes = new List<GameObject>();
        for (int i = 0; i < hitInformations.Length; i++) {
            GameObject objectHit = hitInformations[i].collider.gameObject;
            if (objectHit.CompareTag("Node")) {
                nodes.Add(objectHit);
            }
        }

        return nodes;
    }

    public void setZlayerForNode(GameObject buildingMaterial, string nodeName, int z) {
        SpriteRenderer[] childRenderers = buildingMaterial.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < childRenderers.Length; i++) {
            SpriteRenderer sr = childRenderers[i];
            if (sr.name.Equals(nodeName)) {
                Vector3 nodePos = sr.gameObject.transform.position;
                sr.gameObject.transform.position = new Vector3(nodePos.x, nodePos.y, z);
            }
        }
    }
}
