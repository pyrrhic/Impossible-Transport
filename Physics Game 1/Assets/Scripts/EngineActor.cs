using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof (HingeManager))]
public class EngineActor : MonoBehaviour, IMovableComponent {
    [SerializeField]
    float torque = 100;

    HingeManager hingeManager;
    List<WheelActor> connectedWheels = new List<WheelActor>();

	void Start () {
        hingeManager = GetComponent<HingeManager>();
	}
	
    public void Clear() {
        connectedWheels.Clear();
    }

	void Update () {
        if (Input.GetKeyDown(KeyCode.Z)) {
            float torqueFrac = torque / connectedWheels.Count;
            for (int i = 0; i < connectedWheels.Count; i++) {
                connectedWheels[i].AddTorque(torqueFrac);
            }
        }
	    else if (Input.GetKeyDown(KeyCode.Space)) {
            for (int i = 0; i < connectedWheels.Count; i++) {
                connectedWheels[i].MotorOn();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space)) {
            for (int i = 0; i < connectedWheels.Count; i++) {
                connectedWheels[i].MotorOff();
            }
        }
	}

    void IMovableComponent.UpdateMove(float deltaX, float deltaY) {
        transform.position = new Vector3(transform.position.x + deltaX, transform.position.y + deltaY, transform.position.z);

        hingeManager.UpdateHingePositions();
    }

    public void AddWheel(WheelActor wa) {
        connectedWheels.Add(wa);
    }
}
