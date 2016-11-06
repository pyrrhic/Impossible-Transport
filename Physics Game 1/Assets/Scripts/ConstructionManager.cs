using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ConstructionManager : MonoBehaviour {
    IState[] states;
    IState currentState;

    GameObject framePrefab;
    GameObject wheelPrefab;
    GameObject enginePrefab;
    GameObject springPrefab;
    GameObject chainPrefab;
    
    void Start () {
        framePrefab = Resources.Load("Prefabs/frame") as GameObject;
        wheelPrefab = Resources.Load("Prefabs/wheel") as GameObject;
        enginePrefab = Resources.Load("Prefabs/engine") as GameObject;
        springPrefab = Resources.Load("Prefabs/spring") as GameObject;
        chainPrefab = Resources.Load("Prefabs/chain") as GameObject;

        states = new IState[] {new FrameState(framePrefab),
                               new WheelState(wheelPrefab),
                               new EngineState(enginePrefab),
                               new SpringState(springPrefab),
                               new MoveState(),
                               new ChainState(chainPrefab)};
        
        currentState = states[0];
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Q)) {
            currentState.ExitState();
            currentState = states[0];
            currentState.EnterState();
        }
        else if (Input.GetKeyDown(KeyCode.W)) {
            currentState.ExitState();
            currentState = states[1];
            currentState.EnterState();
        }
        else if (Input.GetKeyDown(KeyCode.E)) {
            currentState.ExitState();
            currentState = states[2];
            currentState.EnterState();
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            currentState.ExitState();
            currentState = states[3];
            currentState.EnterState();
        }
        else if (Input.GetKeyDown(KeyCode.Z)) {
            GameObject[] playerParts = GameObject.FindGameObjectsWithTag("Player");
            List<GameObject> playerSprings = new List<GameObject>();
            for (int i = 0; i < playerParts.Length; i++) {
                //clean up name.
                playerParts[i].name = playerParts[i].name.Replace("(Clone)", "");
                playerParts[i].name = playerParts[i].name.Replace("0", "");
                playerParts[i].name = playerParts[i].name.Replace("1", "");
                playerParts[i].name = playerParts[i].name.Replace("2", "");
                playerParts[i].name = playerParts[i].name.Replace("3", "");
                playerParts[i].name = playerParts[i].name.Replace("4", "");
                playerParts[i].name = playerParts[i].name.Replace("5", "");
                playerParts[i].name = playerParts[i].name.Replace("6", "");
                playerParts[i].name = playerParts[i].name.Replace("7", "");
                playerParts[i].name = playerParts[i].name.Replace("8", "");
                playerParts[i].name = playerParts[i].name.Replace("9", "");
                playerParts[i].name = playerParts[i].name.Replace(" ", "");
                playerParts[i].name = playerParts[i].name + " " + i;

                //doing this because the first call to SpringState won't do anything if this is not the first call to it. Because it clears the list of springs. Because its list of springs is from the
                //original car.
                SpringActor sa = playerParts[i].GetComponent<SpringActor>();
                if (sa) {
                    playerSprings.Add(sa.gameObject);
                }
            }

            if (playerSprings.Count > 0) {
                SpringState.WrapUp(playerSprings);
            }
            else {
                ((SpringState)states[3]).WrapUp();
            }

            //duplicate the player
            GameObject[] newParts = new GameObject[playerParts.Length];
            for (int i = 0; i < playerParts.Length; i++) {
                newParts[i] = Instantiate(playerParts[i]);
                newParts[i].tag = "Respawn";
            }

            //move the duplicate
            for (int i = 0; i < newParts.Length; i++) {
                newParts[i].transform.position = new Vector3(newParts[i].transform.position.x, newParts[i].transform.position.y + 7, newParts[i].transform.position.z);
            }

            //the joints in the cloned parts are still pointing to the original parts. easier to destroy them and then link up.
            for (int i = 0; i < newParts.Length; i++) {
                GameObject newPart = newParts[i];

                //Will re-create the joints so they are setup properly by the spring wrap up.
                SpringJoint2D[] springJoints = newParts[i].GetComponents<SpringJoint2D>();
                if (springJoints.Length > 0) {
                    foreach (SpringJoint2D sj in springJoints) {
                        DestroyImmediate(sj);
                    }
                }

                //What I want to do is update the hinge manager. But right now it's pointing at all the old hinges.
                HingeJoint2D[] hingeJoints = newParts[i].GetComponents<HingeJoint2D>();
                if (hingeJoints.Length > 0) {
                    foreach (HingeJoint2D hj in hingeJoints) {
                        Destroy(hj);
                    }

                    HingeManager hingeManager = newParts[i].GetComponent<HingeManager>();
                    if (hingeManager) {
                        hingeManager.Clear();
                    }
                }

                //get nodes for this part.
                Transform[] children = newParts[i].GetComponentsInChildren<Transform>();
                List<GameObject> nodes = new List<GameObject>();
                foreach (Transform child in children) {
                    if (child.CompareTag("Node") && !child.transform.parent.GetComponent<SpringActor>()) {
                        nodes.Add(child.gameObject);
                    }
                }

                //create hinge
                foreach (GameObject node in nodes) {
                    GameObject topLevelNode = Utils.Instance().GetNodeOverNode(node);
                    //spring parts don't have rigidbodies.
                    if (topLevelNode && !topLevelNode.transform.parent.GetComponent<SpringActor>()) {
                        HingeJoint2D hinge = topLevelNode.transform.parent.gameObject.AddComponent<HingeJoint2D>();
                        Vector3 hingePosition = topLevelNode.transform.localPosition;
                        hinge.anchor = new Vector2(hingePosition.x, hingePosition.y);
                        hinge.connectedBody = node.transform.parent.GetComponent<Rigidbody2D>();

                        topLevelNode.transform.parent.GetComponent<HingeManager>().AddHinge(hinge, topLevelNode);
                    }
                }
            }

            //do this after moving stuff because wrapup does raycasts.
            //this will set the spring top and bottom correctly.
            List<GameObject> newSpringGOs = new List<GameObject>();
            for (int i = 0; i < newParts.Length; i++) {
                SpringActor sa = newParts[i].GetComponent<SpringActor>();
                if (sa) {
                    newSpringGOs.Add(sa.gameObject);
                }
            }
            SpringState.WrapUp(newSpringGOs);

            //set chain start and end, they are pointing to original. set engine actor as well.
            for (int i = 0; i < newParts.Length; i++) {
                EngineActor ea = newParts[i].GetComponent<EngineActor>();
                if (ea) {
                    ea.Clear();
                }

                ChainActor chainActor = newParts[i].GetComponent<ChainActor>();
                if (chainActor) {
                    chainActor.SetStartAndEnd();

                    //one end should be attached to an engine and the other end attached to a wheel.
                    EngineActor engineActor = null;
                    WheelActor wheelActor = null;
                    List<GameObject> startNodes = Utils.Instance().GetNodesOverPosition(chainActor.StartPos.position.x, chainActor.StartPos.position.y);
                    foreach(GameObject node in startNodes) {
                        EngineActor tmpEngineActor = node.transform.parent.GetComponent<EngineActor>();
                        WheelActor tmpWheelActor = node.transform.parent.GetComponent<WheelActor>();
                        if (tmpEngineActor) {
                            engineActor = tmpEngineActor;
                        }
                        else if (tmpWheelActor) {
                            wheelActor = tmpWheelActor;
                        }
                    }

                    List<GameObject> endNodes = Utils.Instance().GetNodesOverPosition(chainActor.EndPos.position.x, chainActor.EndPos.position.y);
                    foreach (GameObject node in endNodes) {
                        EngineActor tmpEngineActor = node.transform.parent.GetComponent<EngineActor>();
                        WheelActor tmpWheelActor = node.transform.parent.GetComponent<WheelActor>();
                        if (tmpEngineActor) {
                            engineActor = tmpEngineActor;
                        }
                        else if (tmpWheelActor) {
                            wheelActor = tmpWheelActor;
                        }
                    }

                    engineActor.AddWheel(wheelActor);
                }
            }

            //set gravity for original player car
            for (int i = 0; i < playerParts.Length; i++) {
                //turn gravity on
                Rigidbody2D rb = playerParts[i].GetComponent<Rigidbody2D>();
                if (rb) {
                    rb.gravityScale = 1;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.X)) {
            ////duplicate the player
            //GameObject[] playerParts = GameObject.FindGameObjectsWithTag("Player");
            //GameObject[] newParts = new GameObject[playerParts.Length];
            //for (int i = 0; i < playerParts.Length; i++) {
            //    newParts[i] = Instantiate(playerParts[i]);
            //}

            ////the joints in the cloned parts are still pointing to the original parts. need to link them to the cloned ones.
            //List<GameObject> springGOs = new List<GameObject>();
            //for (int i = 0; i < newParts.Length; i++) {
            //    GameObject newPart = newParts[i];
            //    HingeJoint2D[] hinges = newPart.GetComponents<HingeJoint2D>();
            //    if (hinges.Length > 0) {
            //        foreach (HingeJoint2D hj in hinges) {
            //            string newConnectedBodyName = hj.connectedBody.name + "(Clone)";
            //            foreach (GameObject np in newParts) {
            //                if (np.name.Equals(newConnectedBodyName)) {
            //                    hj.connectedBody = np.GetComponent<Rigidbody2D>();
            //                }
            //            }
            //        }
            //    }
                
            //    SpringJoint2D[] springs = newPart.GetComponents<SpringJoint2D>();
            //    if (springs.Length > 0) {
            //        foreach (SpringJoint2D sj in springs) {
            //            string newConnectedBodyName = sj.connectedBody.name + "(Clone)";
            //            foreach (GameObject np in newParts) {
            //                if (np.name.Equals(newConnectedBodyName)) {
            //                    sj.connectedBody = np.GetComponent<Rigidbody2D>();
            //                }
            //            }
            //        }
            //    }
            //}
            
            //for (int i = 0; i < newParts.Length; i++) {
            //    SpringActor sa = newParts[i].GetComponent<SpringActor>();
            //    if (sa) {
            //        springGOs.Add(sa.gameObject);
            //    }
            //}

            //for (int i = 0; i < newParts.Length; i++) {
            //    //move
            //    newParts[i].transform.position = new Vector3(newParts[i].transform.position.x, newParts[i].transform.position.y + 7, newParts[i].transform.position.z);

            //    newParts[i].tag = "Respawn";

            //    Rigidbody2D rb = newParts[i].GetComponent<Rigidbody2D>();
            //    if (rb) {
            //        rb.gravityScale = 0;
            //    }
            //}

            ////do this after moving stuff because wrapup does raycasts.
            ////this will set the spring top and bottom correctly.
            //SpringState.WrapUp(springGOs, false);

            ////need to do something similar for chain actors
            //for (int i = 0; i < newParts.Length; i++) {
            //    ChainActor chainActor = newParts[i].GetComponent<ChainActor>();
            //    if (chainActor) {
            //        chainActor.SetStartAndEnd();
            //    }
            //}
                
            ////need to update the joint managers, can do that later..
        }
        else if (Input.GetKeyDown(KeyCode.C)) {
            GameObject[] playerParts = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < playerParts.Length; i++) {
                playerParts[i].tag = "Untagged";
                Destroy(playerParts[i]);
            }

            GameObject[] newParts = GameObject.FindGameObjectsWithTag("Respawn");
            for (int i = 0; i < newParts.Length; i++) {
                newParts[i].transform.position = new Vector3(newParts[i].transform.position.x, newParts[i].transform.position.y - 7, newParts[i].transform.position.z);

                newParts[i].tag = "Player";

                //turn off springs because the next mode will be build mode, and they will make the car all bouncy and shit when i don't want it to move.
                SpringJoint2D[] springJoints = newParts[i].GetComponents<SpringJoint2D>();
                if (springJoints.Length > 0) {
                    foreach (SpringJoint2D sj in springJoints) {
                        sj.enabled = false;
                    }
                }
            }

            Camera.main.gameObject.GetComponent<CameraActor>().Reset();
        }
        else if (Input.GetKeyDown(KeyCode.M)) {
            currentState.ExitState();
            currentState = states[4];
            currentState.EnterState();
        }
        else if (Input.GetKeyDown(KeyCode.T)) {
            currentState.ExitState();
            currentState = states[5];
            currentState.EnterState();
        }

        currentState.Update();


        //transform.position = Vector2.Lerp(start.position, end.position, 0.5f);
        //Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Quaternion rot = Quaternion.LookRotation(Vector3.forward, mousePosition - transform.position);

        //transform.rotation = rot;
        //transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

        //float distance = Vector2.Distance(new Vector2(start.position.x, start.position.y), 
        //new Vector2(end.position.x, end.position.y));
        //print(distance);
        //scale needed = distance x units per pixel / sprite pixel size
        //transform.localScale = new Vector3(distance * 100 / 16, transform.localScale.y, transform.localScale.z);
    }

    private abstract class IState {
        abstract public void EnterState();
        abstract public void Update();
        abstract public void ExitState();

        protected GameObject GetNodeMouseOver() {
            return GetNodeOverPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, 
                                       Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }

        protected static GameObject GetNodeOverNode (GameObject node) {
            GameObject hitNode = GetNodeOverPosition(node.transform.position.x, node.transform.position.y);

            if (hitNode!= null && hitNode.GetInstanceID() == node.GetInstanceID()) {
                return null;
            }

            return hitNode;
        }

        protected static GameObject GetNodeOverPosition (float x, float y) {
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

        protected void setZlayerForNode(GameObject buildingMaterial, string nodeName, int z) {
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

    private class MoveState : IState {
        bool isMouseDown = false;
        List<MoveNode> movableNodes = new List<MoveNode>();
        MoveComponent moveComponent = new MoveComponent();
        Vector2 lastFramePos = new Vector2();

        struct MoveComponent {
            public IMovableComponent script;
        }

        struct MoveNode {
            public GameObject node;
            public IMovableNode script;
        }

        public override void EnterState() {
            
        }

        public override void ExitState() {
            
        }
        
        public override void Update() {
            if (isMouseDown == false && Input.GetMouseButtonDown(0)) {
                lastFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                isMouseDown = true;
            } else if (isMouseDown && Input.GetMouseButtonUp(0)) {
                isMouseDown = false;
            }
            
            ComponentMove();

            //umm, this script check isn't working out. move when selecting component node that has a frame on it.
            if (moveComponent.script == null) {
                NodeMove();
            }


            lastFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        void ComponentMove() {
            if (isMouseDown && moveComponent.script == null && movableNodes.Count == 0) {
                Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D[] hitInformations = Physics2D.RaycastAll(new Vector2(position.x, position.y),
                                                                 Vector2.zero,
                                                                 0);

                int numNodes = 0;
                for (int i = 0; i < hitInformations.Length; i++) {
                    if (hitInformations[i].collider.gameObject.CompareTag("Node")) {
                        numNodes++;
                    }
                }

                bool nodeWithOtherNodesClicked = (numNodes > 1) ? true : false;
                if (nodeWithOtherNodesClicked) {
                    for (int i = 0; i < hitInformations.Length; i++) {
                        GameObject objectHit = hitInformations[i].collider.gameObject;
                        if (objectHit.CompareTag("Node")) {
                            if (moveComponent.script == null) {
                                IMovableComponent mc = objectHit.transform.parent.GetComponent<IMovableComponent>();
                                if (mc != null) {
                                    moveComponent.script = mc;
                                    continue;
                                }
                            }

                            AddMovableNodesAtLocation(objectHit.gameObject.transform.position);
                        }
                    }
                }
                else {
                    for (int i = 0; i < hitInformations.Length; i++) {
                        GameObject objectHit = hitInformations[i].collider.gameObject;
                        IMovableComponent mc = objectHit.transform.parent.GetComponent<IMovableComponent>();
                        if (mc != null) {
                            moveComponent.script = mc;

                            Transform[] children = objectHit.transform.parent.GetComponentsInChildren<Transform>();
                            foreach (Transform child in children) {
                                AddMovableNodesAtLocation(child.position);
                            }

                            break;
                        }
                    }
                }
            }
            else if (isMouseDown && moveComponent.script != null) {
                Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float deltaX = position.x - lastFramePos.x;
                float deltaY = position.y - lastFramePos.y;
                moveComponent.script.UpdateMove(deltaX, deltaY);

                if (movableNodes.Count > 0) {
                    for (int i = 0; i < movableNodes.Count; i++) {
                        MoveNode mn = movableNodes[i];
                        Vector2 newPosition = new Vector2(mn.node.transform.position.x + deltaX, mn.node.transform.position.y + deltaY);
                        mn.node.transform.position = new Vector3(newPosition.x, newPosition.y, mn.node.transform.position.z);
                        mn.script.UpdateMove();
                    }
                }
            }
            else if (isMouseDown == false) {
                moveComponent.script = null;
                movableNodes.Clear();
            }
        }
        
        void NodeMove() {
            if (isMouseDown && movableNodes.Count == 0) {
                AddMovableNodesAtLocation(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            else if (isMouseDown && movableNodes.Count > 0) {
                for (int i = 0; i < movableNodes.Count; i++) {
                    MoveNode mn = movableNodes[i];
                    Vector2 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mn.node.transform.position = new Vector3(newPosition.x, newPosition.y, mn.node.transform.position.z);
                    mn.script.UpdateMove();
                }
            }
            else if (isMouseDown == false) {
                movableNodes.Clear();
            }
        }

        //get nodes at location
        //add them to list of nodes to be moved.
        void AddMovableNodesAtLocation(Vector3 worldCoord) {
            RaycastHit2D[] hitInfos = Physics2D.RaycastAll(new Vector2(worldCoord.x, worldCoord.y),
                                                             Vector2.zero,
                                                             0);

            for (int i = 0; i < hitInfos.Length; i++) {
                GameObject objectHit = hitInfos[i].collider.gameObject;

                if (objectHit.CompareTag("Node")) {
                    IMovableNode script = objectHit.transform.parent.GetComponent<IMovableNode>();
                    if (script != null) {
                        MoveNode mn = new MoveNode();
                        mn.node = objectHit;
                        mn.script = script;
                        movableNodes.Add(mn);
                    }
                }
            }
        }
    }

    private class EngineState : IState {
        GameObject prefab;
        GameObject engine;

        public EngineState(GameObject prefab) {
            this.prefab = prefab;
        }

        public override void EnterState() {
            engine = Instantiate(prefab);
            engine.GetComponent<Rigidbody2D>().gravityScale = 0;
        }

        public override void ExitState() {
            if (engine) {
                Destroy(engine);
                engine = null;
            }
        }

        public override void Update() {
            //if user clicked and the engine was created, create another one for them to potentially place.
            if (engine == null) {
                engine = Instantiate(prefab);
                engine.GetComponent<Rigidbody2D>().gravityScale = 0;
            }

            //Move them so when we iterate over the children and look for other nodes they are colliding with, the -3 nodes will
            //get hit by the ray cast before the unplaced engine's nodes. we want to check for other nodes so we can do things like snap to them,
            //setZlayerForNode(engine, "bottom right", -2);
            //setZlayerForNode(engine, "bottom left", -2);
            setZlayerForNode(engine, "top right", -2);
            setZlayerForNode(engine, "top left", -2);
            
            Transform tlNode = null;
            Transform trNode = null;
            Transform blNode = null;
            Transform brNode = null;
            Transform[] children = engine.GetComponentsInChildren<Transform>();
            for (int x = 0; x < children.Length; x++) {
                Transform child = children[x];
                if (child.name.Equals("top left")) {
                    tlNode = child;
                }
                else if (child.name.Equals("top right")) {
                    trNode = child;
                }
                else if (child.name.Equals("bottom left")) {
                    blNode = child;
                }
                else if (child.name.Equals("bottom right")) {
                    brNode = child;
                }
            }

            //position over the mouse first, otherwise the engine will never break away from the locked on node when the player moves the mouse.
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            engine.transform.position = new Vector3(mousePosition.x, mousePosition.y, -1);
            //setZlayerForNode(engine, "bottom right", -2);
            //setZlayerForNode(engine, "bottom left", -2);
            setZlayerForNode(engine, "top right", -2);
            setZlayerForNode(engine, "top left", -2);

            GameObject otherNode = null;
            Vector2 delta = Vector2.zero;
            GameObject tlOtherNode = GetNodeOverNode(tlNode.gameObject);
            GameObject trOtherNode = GetNodeOverNode(trNode.gameObject);
            //GameObject blOtherNode = GetNodeOverNode(blNode.gameObject);
            //GameObject brOtherNode = GetNodeOverNode(brNode.gameObject);
            if (tlOtherNode) {
                delta.x = tlNode.position.x - tlOtherNode.transform.position.x;
                delta.y = tlNode.position.y - tlOtherNode.transform.position.y;

                otherNode = tlOtherNode;
            }
            else if (trOtherNode) {
                delta.x = trNode.position.x - trOtherNode.transform.position.x;
                delta.y = trNode.position.y - trOtherNode.transform.position.y;

                otherNode = trOtherNode;
            }
            //else if (blOtherNode) {
            //    delta.x = blNode.position.x - blOtherNode.transform.position.x;
            //    delta.y = blNode.position.y - blOtherNode.transform.position.y;

            //    otherNode = blOtherNode;
            //}
            //else if (brOtherNode) {
            //    delta.x = brNode.position.x - brOtherNode.transform.position.x;
            //    delta.y = brNode.position.y - brOtherNode.transform.position.y;

            //    otherNode = brOtherNode;
            //}

            //The delta x and delta y between the child node and the other node.
            if (otherNode) {
                engine.transform.position = new Vector3(engine.transform.position.x - delta.x, engine.transform.position.y - delta.y, -1);
                //setZlayerForNode(engine, "bottom right", -2);
                //setZlayerForNode(engine, "bottom left", -2);
                setZlayerForNode(engine, "top right", -2);
                setZlayerForNode(engine, "top left", -2);

                if (tlOtherNode == null) {
                    setZlayerForNode(engine, "top left", -3);
                }
                if (trOtherNode == null) {
                    setZlayerForNode(engine, "top right", -3);
                }
                //if (brOtherNode == null) {
                //    setZlayerForNode(engine, "bottom right", -3);
                //}
                //if (blOtherNode == null) {
                //    setZlayerForNode(engine, "bottom left", -3);
                //}
            }

            if (Input.GetMouseButtonDown(0)) {
                if (otherNode != null) {
                    HingeJoint2D hingeJoint = otherNode.transform.parent.gameObject.AddComponent<HingeJoint2D>();
                    Vector3 worldCoordOfNode = otherNode.transform.parent.InverseTransformPoint(otherNode.transform.position);
                    hingeJoint.anchor = new Vector2(worldCoordOfNode.x, worldCoordOfNode.y);
                    hingeJoint.connectedBody = engine.GetComponent<Rigidbody2D>();
                }

                if (tlOtherNode == null) {
                    setZlayerForNode(engine, "top left", -3);
                }
                if (trOtherNode == null) {
                    setZlayerForNode(engine, "top right", -3);
                }
                //if (brOtherNode == null) {
                //    setZlayerForNode(engine, "bottom right", -3);
                //}
                //if (blOtherNode == null) {
                //    setZlayerForNode(engine, "bottom left", -3);
                //}

                engine = null;
            }
        }
    }

    private class WheelState : IState {
        GameObject prefab;
        GameObject wheel;

        public WheelState(GameObject prefab) {
            this.prefab = prefab;
        }
        
        public override void EnterState() {
           wheel = Instantiate(prefab);
           wheel.GetComponent<Rigidbody2D>().gravityScale = 0;
        }

        public override void ExitState() {
            if (wheel) {
                Destroy(wheel);
                wheel = null;
            }
        }

        public override void Update() {
            //if user clicked and the wheel was created, create another one for them to potentially place.
            if (wheel == null) {
                wheel = Instantiate(prefab);
                //setZlayerForNode(wheel, "middle", -2);
                wheel.GetComponent<Rigidbody2D>().gravityScale = 0;
            }

            GameObject node = GetNodeMouseOver();
            if (node != null && node.transform.parent.gameObject.GetInstanceID() != wheel.GetInstanceID()) {
                wheel.transform.position = new Vector3(node.transform.position.x, node.transform.position.y, -1);
                setZlayerForNode(wheel, "middle", -2);
            }
            else {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                wheel.transform.position = new Vector3(mousePosition.x, mousePosition.y, -1);
                setZlayerForNode(wheel, "middle", -2);
            }

            if (Input.GetMouseButtonDown(0)) {
                if (node != null && node.transform.parent.gameObject.GetInstanceID() != wheel.GetInstanceID()) {
                    HingeJoint2D hingeJoint = node.transform.parent.gameObject.AddComponent<HingeJoint2D>();
                    Vector3 worldCoordOfNode = node.transform.parent.InverseTransformPoint(node.transform.position);
                    hingeJoint.anchor = new Vector2(worldCoordOfNode.x, worldCoordOfNode.y);
                    hingeJoint.connectedBody = wheel.GetComponent<Rigidbody2D>();
                }

                if (node != null && node.transform.parent.gameObject.GetInstanceID() != wheel.GetInstanceID()) {
                    setZlayerForNode(wheel, "middle", -2);
                }
                else {
                    setZlayerForNode(wheel, "middle", -3);
                }

                wheel = null;
            }
        }
    }

    private class FrameState : IState {
        enum FRAME_STATES {
            START_FRAME, STRETCH_FRAME
        }

        FRAME_STATES frameState = FRAME_STATES.START_FRAME;

        GameObject currentFrame;
        Vector2 startPosition;
        GameObject prefab;

        public FrameState (GameObject prefab) {
            this.prefab = prefab;
        }

        public override void EnterState() {
            
        }

        public override void ExitState() {
            
        }

        public override void Update() {
            switch (frameState) {
                case FRAME_STATES.START_FRAME:
                    if (Input.GetMouseButtonDown(0)) {
                        GameObject node = GetNodeMouseOver();
                        HingeJoint2D hj = null;
                        if (node && node.transform.position.z == -3) {
                            hj = node.transform.parent.gameObject.AddComponent<HingeJoint2D>();
                            node.transform.parent.GetComponent<HingeManager>().AddHinge(hj, node);
                            Vector3 bottomNodeHingePos = node.transform.parent.InverseTransformPoint(node.transform.position);
                            hj.anchor = new Vector2(bottomNodeHingePos.x, bottomNodeHingePos.y);
                        }

                        currentFrame = Instantiate(prefab);
                        if (hj) {
                            hj.connectedBody = currentFrame.GetComponent<Rigidbody2D>();
                        }
                        currentFrame.GetComponent<Rigidbody2D>().gravityScale = 0;

                        if (node && node.transform.position.z == -3) {
                            setZlayerForNode(currentFrame, "bottom", -2);
                        }
                        else {
                            setZlayerForNode(currentFrame, "bottom", -3);
                        }

                        if (node) {
                            currentFrame.transform.position = Camera.main.ScreenToWorldPoint(node.transform.position);
                            currentFrame.transform.position = new Vector3(node.transform.position.x, node.transform.position.y, 0);
                        }
                        else {
                            currentFrame.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            currentFrame.transform.position = new Vector3(currentFrame.transform.position.x, currentFrame.transform.position.y, 0);
                        }

                        startPosition = new Vector3(currentFrame.transform.position.x, currentFrame.transform.position.y, 0);

                        frameState = FRAME_STATES.STRETCH_FRAME;
                    }

                    break;
                case FRAME_STATES.STRETCH_FRAME:
                    //Midpoint
                    GameObject nodee = GetNodeMouseOver();

                    Vector3 mousePosition = (nodee) ? nodee.transform.position : Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 midpoint = Vector2.Lerp(startPosition, mousePosition, 0.5f);
                    currentFrame.transform.position = midpoint;

                    //Rotate
                    Quaternion rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - currentFrame.transform.position);
                    currentFrame.transform.rotation = rotation;
                    currentFrame.transform.eulerAngles = new Vector3(0, 0, currentFrame.transform.eulerAngles.z);

                    //Scale the wood.
                    //scale needed = distance x units per pixel / sprite pixel size
                    float distance = Vector2.Distance(startPosition, mousePosition);
                    BoxCollider2D woodCollider = currentFrame.GetComponentInChildren<BoxCollider2D>();
                    float spritePixelSize = woodCollider.gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.y * 100;
                    woodCollider.gameObject.transform.localScale = new Vector3(currentFrame.transform.localScale.x, distance * 100 / spritePixelSize, currentFrame.transform.localScale.z);

                    //Move the nodes.
                    float fTop = woodCollider.offset.y + (woodCollider.size.y / 2f);
                    float fBottom = woodCollider.offset.y - (woodCollider.size.y / 2f);
                    //float fLeft = woodCollider.offset.x - (woodCollider.size.x / 2f);
                    //float fRight = woodCollider.offset.x + (woodCollider.size.x / 2f);

                    Transform bottomCircle = null;
                    Transform topCircle = null;
                    Transform[] children = currentFrame.GetComponentsInChildren<Transform>();
                    for (int i = 0; i < children.Length; i++) {
                        if (children[i].name.Equals("bottom")) {
                            bottomCircle = children[i];
                        }
                        else if (children[i].name.Equals("top")) {
                            topCircle = children[i];
                        }
                    }

                    bottomCircle.position = woodCollider.gameObject.transform.TransformPoint(new Vector3(0f, fBottom, bottomCircle.position.z)); ;
                    topCircle.position = woodCollider.gameObject.transform.TransformPoint(new Vector3(0f, fTop, 0f));

                    if (Input.GetMouseButtonDown(0)) {
                        if (nodee != null && nodee.transform.parent.gameObject.GetInstanceID() != currentFrame.GetInstanceID() &&
                            nodee.transform.position.z == -3) {
                            //Hinge
                            HingeJoint2D hingeJoint = nodee.transform.parent.gameObject.AddComponent<HingeJoint2D>();
                            nodee.transform.parent.GetComponent<HingeManager>().AddHinge(hingeJoint, nodee);
                            Vector3 topNodeHingePos = nodee.transform.parent.InverseTransformPoint(nodee.transform.position);
                            hingeJoint.anchor = new Vector2(topNodeHingePos.x, topNodeHingePos.y);
                            hingeJoint.connectedBody = currentFrame.GetComponent<Rigidbody2D>();

                            setZlayerForNode(currentFrame, "top", -2);
                        } else {
                            setZlayerForNode(currentFrame, "top", -3);
                        }
                        
                        frameState = FRAME_STATES.START_FRAME;
                    }

                    break;
            }
        }
    }

    private class ChainState : IState {
        enum CHAIN_STATES {
            START, STRETCH
        }

        CHAIN_STATES currentState = CHAIN_STATES.START;
        GameObject currentChain;
        GameObject prefab;
        Vector2 startPosition;

        public ChainState(GameObject prefab) {
            this.prefab = prefab;
        }

        public override void EnterState() {
        }

        public override void ExitState() {
        }

        public override void Update() {
            switch (currentState) {
                case CHAIN_STATES.START:
                    if (Input.GetMouseButtonDown(0)) {
                        GameObject node = GetNodeMouseOver();
                        
                        if (node) {
                            currentChain = Instantiate(prefab);

                            currentChain.transform.position = Camera.main.ScreenToWorldPoint(node.transform.position);
                            currentChain.transform.position = new Vector3(node.transform.position.x, node.transform.position.y, 0);

                            startPosition = new Vector3(currentChain.transform.position.x, currentChain.transform.position.y, 0);
                            currentState = CHAIN_STATES.STRETCH;
                        }
                    }

                    break;
                case CHAIN_STATES.STRETCH:
                    //Midpoint
                    GameObject nodee = GetNodeMouseOver();

                    Vector3 mousePosition = (nodee) ? nodee.transform.position : Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 midpoint = Vector2.Lerp(startPosition, mousePosition, 0.5f);
                    currentChain.transform.position = midpoint;

                    //Rotate
                    Quaternion rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - currentChain.transform.position);
                    currentChain.transform.rotation = rotation;
                    currentChain.transform.eulerAngles = new Vector3(0, 0, currentChain.transform.eulerAngles.z);

                    //Scale the spring.
                    //scale needed = distance x units per pixel / sprite pixel size
                    float distance = Vector2.Distance(startPosition, mousePosition);
                    GameObject chainMiddle = null;
                    Transform[] chainChildren = currentChain.GetComponentsInChildren<Transform>();
                    for (int i = 0; i < chainChildren.Length; i++) {
                        if (chainChildren[i].name.Equals("chain middle")) {
                            chainMiddle = chainChildren[i].gameObject;
                            break;
                        }
                    }
                    float spritePixelSize = chainMiddle.GetComponent<SpriteRenderer>().sprite.bounds.size.y * 100;

                    chainMiddle.transform.localScale = new Vector3(currentChain.transform.localScale.x, distance * 100 / spritePixelSize, currentChain.transform.localScale.z);

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
                    Transform[] children = currentChain.GetComponentsInChildren<Transform>();
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

                    if (Input.GetMouseButtonDown(0) && nodee) {
                        currentChain.GetComponent<ChainActor>().StartPos = (GetNodeOverPosition(chainStart.position.x, chainStart.position.y)).transform;
                        currentChain.GetComponent<ChainActor>().EndPos = (GetNodeOverPosition(chainEnd.position.x, chainEnd.position.y)).transform;

                        EngineActor engineActor = null;
                        WheelActor wheelActor = null;
                        RaycastHit2D[] hitInformations = Physics2D.RaycastAll(new Vector2(chainStart.position.x, chainStart.position.y),
                                                                             Vector2.zero,
                                                                             0);
                        for (int i = 0; i < hitInformations.Length; i++) {
                            EngineActor engine = hitInformations[i].collider.gameObject.transform.parent.GetComponent<EngineActor>();
                            WheelActor wheel = hitInformations[i].collider.gameObject.transform.parent.GetComponent<WheelActor>();
                            if (engine) {
                                engineActor = engine;
                            }
                            else if (wheel) {
                                wheelActor = wheel;
                            }
                        }

                        hitInformations = Physics2D.RaycastAll(new Vector2(chainEnd.position.x, chainEnd.position.y),
                                                                             Vector2.zero,
                                                                             0);
                        for (int i = 0; i < hitInformations.Length; i++) {
                            EngineActor engine = hitInformations[i].collider.gameObject.transform.parent.GetComponent<EngineActor>();
                            WheelActor wheel = hitInformations[i].collider.gameObject.transform.parent.GetComponent<WheelActor>();
                            if (engine) {
                                engineActor = engine;
                            }
                            else if (wheel) {
                                wheelActor = wheel;
                            }
                        }
                        
                        if (engineActor && wheelActor) {
                            engineActor.AddWheel(wheelActor);
                        }

                        currentState = CHAIN_STATES.START;
                    }

                    break;
            }
        }
    }

    private class SpringState : IState {
        enum SPRING_STATES {
            START_SPRING, STRETCH_SPRING
        }

        SPRING_STATES springState = SPRING_STATES.START_SPRING;

        GameObject currentSpring;
        Vector2 startPosition;
        GameObject prefab;
        List<GameObject> springs = new List<GameObject>();

        public SpringState(GameObject prefab) {
            this.prefab = prefab;
        }

        public static void WrapUp(List<GameObject> springs) {
            for (int x = 0; x < springs.Count; x++) {
                GameObject spring = springs[x];

                Transform bottomCircle = null;
                Transform topCircle = null;
                Transform[] children = spring.GetComponentsInChildren<Transform>();
                for (int i = 0; i < children.Length; i++) {
                    if (children[i].name.Equals("bottom")) {
                        bottomCircle = children[i];
                    }
                    else if (children[i].name.Equals("top")) {
                        topCircle = children[i];
                    }
                }

                GameObject topNode = GetNodeOverNode(topCircle.gameObject);
                GameObject bottomNode = GetNodeOverNode(bottomCircle.gameObject);
                if (topNode && bottomNode) {
                    SpringJoint2D springJoint = bottomNode.transform.parent.gameObject.AddComponent<SpringJoint2D>();

                    springJoint.anchor = bottomNode.transform.parent.InverseTransformPoint(bottomNode.transform.position);

                    Rigidbody2D rb = topNode.transform.parent.gameObject.GetComponent<Rigidbody2D>();
                    springJoint.connectedBody = rb;
                    springJoint.connectedAnchor = topNode.transform.parent.InverseTransformPoint(topNode.transform.position);

                    springJoint.autoConfigureDistance = false;

                    spring.GetComponent<SpringActor>().SpringJoint = springJoint;
                    
                    //setting start and end so the visual spring moves with the 'top' node whenever they move.
                    spring.GetComponent<SpringActor>().StartPos = bottomNode.transform;
                    spring.GetComponent<SpringActor>().EndPos = topNode.transform;
                }
            }
        }

        public void WrapUp() {
            WrapUp(springs);
            springs.Clear();
        }

        public override void EnterState() {

        }

        public override void ExitState() {

        }

        public override void Update() {
            switch (springState) {
                case SPRING_STATES.START_SPRING:
                    if (Input.GetMouseButtonDown(0)) {
                        GameObject node = GetNodeMouseOver();

                        currentSpring = Instantiate(prefab);

                        //if (node) {
                        //    setZlayerForNode(currentSpring, "bottom", -2);
                        //}
                        //else {
                        //    setZlayerForNode(currentSpring, "bottom", -3);
                        //}

                        setZlayerForNode(currentSpring, "bottom", -2);

                        if (node) {
                            currentSpring.transform.position = Camera.main.ScreenToWorldPoint(node.transform.position);
                            currentSpring.transform.position = new Vector3(node.transform.position.x, node.transform.position.y, 0);
                            setZlayerForNode(currentSpring, "bottom", -2);
                        }
                        else {
                            currentSpring.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            currentSpring.transform.position = new Vector3(currentSpring.transform.position.x, currentSpring.transform.position.y, 0);
                            setZlayerForNode(currentSpring, "bottom", -2);
                        }

                        startPosition = new Vector3(currentSpring.transform.position.x, currentSpring.transform.position.y, 0);

                        springState = SPRING_STATES.STRETCH_SPRING;
                    }

                    break;
                case SPRING_STATES.STRETCH_SPRING:
                    //Midpoint
                    GameObject nodee = GetNodeMouseOver();

                    Vector3 mousePosition = (nodee) ? nodee.transform.position : Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 midpoint = Vector2.Lerp(startPosition, mousePosition, 0.5f);
                    currentSpring.transform.position = midpoint;

                    //Rotate
                    Quaternion rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - currentSpring.transform.position);
                    currentSpring.transform.rotation = rotation;
                    currentSpring.transform.eulerAngles = new Vector3(0, 0, currentSpring.transform.eulerAngles.z);

                    //Scale the spring.
                    //scale needed = distance x units per pixel / sprite pixel size
                    float distance = Vector2.Distance(startPosition, mousePosition);
                    BoxCollider2D wireCollider = currentSpring.GetComponentInChildren<BoxCollider2D>();
                    float spritePixelSize = wireCollider.gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.y * 100;
                    
                    wireCollider.gameObject.transform.localScale = new Vector3(currentSpring.transform.localScale.x, distance * 100 / spritePixelSize, currentSpring.transform.localScale.z);

                    //scale the spring joint
                    //currentSpring.GetComponent<SpringJoint2D>().distance = distance / 2f;

                    //Move the nodes.
                    float fTop = wireCollider.offset.y + (wireCollider.size.y / 2f);
                    float fBottom = wireCollider.offset.y - (wireCollider.size.y / 2f);
                    //float fLeft = woodCollider.offset.x - (woodCollider.size.x / 2f);
                    //float fRight = woodCollider.offset.x + (woodCollider.size.x / 2f);

                    Transform bottomCircle = null;
                    Transform topCircle = null;
                    Transform[] children = currentSpring.GetComponentsInChildren<Transform>();
                    for (int i = 0; i < children.Length; i++) {
                        if (children[i].name.Equals("bottom")) {
                            bottomCircle = children[i];
                        }
                        else if (children[i].name.Equals("top")) {
                            topCircle = children[i];
                        }
                    }

                    bottomCircle.position = wireCollider.gameObject.transform.TransformPoint(new Vector3(0f, fBottom, bottomCircle.position.z)); ;
                    topCircle.position = wireCollider.gameObject.transform.TransformPoint(new Vector3(0f, fTop, 0f));

                    Transform springBottom = null;
                    Transform springTop = null;
                    Transform[] springChildren = currentSpring.GetComponentsInChildren<Transform>();
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

                    if (Input.GetMouseButtonDown(0)) {
                        //if (nodee != null && nodee.transform.parent.gameObject.GetInstanceID() != currentSpring.GetInstanceID()) {                            
                        //    setZlayerForNode(currentSpring, "top", -2);
                        //}
                        //else {
                        //    setZlayerForNode(currentSpring, "top", -3);
                        //}

                        setZlayerForNode(currentSpring, "top", -2);

                        //they are disabled during placement because they make other stuff go haywire. i don't really understand it.
                        //currentSpring.GetComponent<SpringJoint2D>().enabled = true;
                        //currentSpring.GetComponent<SliderJoint2D>().enabled = true;

                        springs.Add(currentSpring);

                        springState = SPRING_STATES.START_SPRING;
                    }

                    break;
            }
        }
    }
}
