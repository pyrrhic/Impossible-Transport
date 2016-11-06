using UnityEngine;
using System.Collections;

public class CameraActor : MonoBehaviour {
    bool start = false;

    GameObject[] playerParts;

    Vector2 lastPos = new Vector2();

	void Start () {
        
	}
	
    public void Reset() {
        start = true;
        playerParts = GameObject.FindGameObjectsWithTag("Player");
    }

	void Update () {
        if (Input.GetKeyDown(KeyCode.Z)) {
            start = true;
            playerParts = GameObject.FindGameObjectsWithTag("Player");
        }
	}

    void FixedUpdate() {
        if (start) {
            float deltaX = Mathf.Abs(lastPos.x - transform.position.x);
            float deltaY = Mathf.Abs(lastPos.y - transform.position.y);
            lastPos = transform.position;
            Debug.Log((deltaX + deltaY) * 100f);

            //position camera
            float x = 0;
            float y = 0;
            for (int i = 0; i < playerParts.Length; i++) {
                x += playerParts[i].transform.position.x;
                y += playerParts[i].transform.position.y;
            }

            x /= playerParts.Length;
            x += 5;
            y /= playerParts.Length;

            transform.position = Vector3.Lerp(transform.position, new Vector3(x, y, transform.position.z), 5.0f * Time.deltaTime);
        }
    }
}
