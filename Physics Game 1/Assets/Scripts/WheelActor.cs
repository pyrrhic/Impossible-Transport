using UnityEngine;
using System.Collections;
using System;

[RequireComponent (typeof (HingeManager))]
public class WheelActor : MonoBehaviour, IMovableComponent {
    [SerializeField]
    float speed = 10000f;
    [SerializeField]
    float torque = 0;
    [SerializeField]
    bool useMotor;
    Rigidbody2D rbody;
    GameObject tire;

    public float currentSpeed;

    void Start () {
        rbody = GetComponent<Rigidbody2D>();
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name.Equals("tire")) {
                tire = children[i].gameObject;
            }
        }
	}
	
	void Update () {
        currentSpeed = -rbody.angularVelocity;
    }

    void FixedUpdate() {
        if(useMotor && -rbody.angularVelocity < speed) {
            rbody.AddTorque(-torque);
        }
    }

    public void MotorOn() {
        useMotor = true;
    }

    public void MotorOff() {
        useMotor = false;
    }

    public void AddSpeed(float speed) {
        this.speed += speed;
    }

    public void AddTorque(float torque) {
        this.torque += torque;
    }

    void IMovableComponent.UpdateMove(float deltaX, float deltaY) {
        transform.position = new Vector3(transform.position.x + deltaX, transform.position.y + deltaY, transform.position.z);
    }
}
