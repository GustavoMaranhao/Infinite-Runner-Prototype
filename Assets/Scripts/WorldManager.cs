using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldManager : MonoBehaviour {
	[System.Serializable]
	public class LaneAssets {
		public GameObject Center;
		public GameObject Left;
		public GameObject Right;
	}
	public LaneAssets laneComponents;

	public int minLaneLength = 12;
	public int drawDistance = 64;
	public int minDistanceBetweenCurves = 12;
	public int chanceForCurves = 5;

	public float destroyDistance = 7f;
	public Vector3 startPos = new Vector3(0,0,0);

	private GameObject physicalLane;
	private List<GameObject> builtLane;
	private int lanesCount;
	private int distanceToLastCurve;
	private int startRot = 0;

	private int numberOfLefts = 0;
	private int numberOfRights = 0;

	private GameObject playerObject;

	void Start () {
		Debug.Log ("Scene Starting");
		if (laneComponents.Center == null) {
			laneComponents.Center = (GameObject)Resources.Load ("Prefabs/Floor/TripeStraight");
		}
		if (laneComponents.Left == null) {
			laneComponents.Left = (GameObject)Resources.Load ("Prefabs/Floor/TripeLeftCorner");
		}
		if (laneComponents.Right == null) {
			laneComponents.Right = (GameObject)Resources.Load ("Prefabs/Floor/TripeRightCorner");
		}

		playerObject = GameObject.FindGameObjectWithTag ("Player");
		playerObject.GetComponent<Rigidbody> ().isKinematic = true;
		playerObject.transform.Rotate (playerObject.transform.up, startRot);
		playerObject.GetComponent<Rigidbody> ().isKinematic = false;

		physicalLane = new GameObject();
		physicalLane.name = "Lane";

		int startingLength = Random.Range(minLaneLength, drawDistance);

		builtLane = new List<GameObject>();
		lanesCount = 0;

		Debug.Log ("Starting to build first lane stretch");

		GameObject laneObject = Instantiate (laneComponents.Center) as GameObject;
		laneObject.name = laneObject.name + " forward start";
		laneObject.transform.SetParent (physicalLane.transform);

		laneObject.transform.Rotate (laneObject.transform.up, startRot);

		Transform laneTransform = laneObject.transform;
		Vector3 lanePosition = laneTransform.position;
		Vector3 lanePieceSize = laneObject.GetComponent<Collider>().bounds.size;
		lanePosition.x = startPos.x + lanePieceSize.x * Mathf.RoundToInt(laneTransform.forward.x);
		lanePosition.y = 0;
		lanePosition.z = startPos.z + lanePieceSize.z * Mathf.RoundToInt(laneTransform.forward.z);
		laneObject.transform.position = lanePosition;

		builtLane.Add (laneObject);
		lanesCount++;

		for (int i = 1; i < startingLength - 1; i++) {
			AddForwardBlock ();
		}

		while (lanesCount < drawDistance) {
			int randomChoice = Random.Range(0, 3);
			int randomLength = Random.Range (minLaneLength, drawDistance); 
			if (distanceToLastCurve < minDistanceBetweenCurves) {
				Debug.Log ("Too close to another curve, continue straight");
				for (int i = 0; (i < randomLength - 1) && (lanesCount < drawDistance); ++i) {
					AddForwardBlock ();
				}
			} else {				
				switch (randomChoice) {
				case 0: //Forward
					Debug.Log ("Adding another straight stretch");
					for (int i = 0; (i < randomLength - 1) && (lanesCount < drawDistance); ++i) {
						AddForwardBlock ();
					}
					break;
				case 1: //Left
					AddTurnBlock ("Left");
					for (int i = 0; (i < randomLength - 1) && (lanesCount < drawDistance); ++i) {
						AddForwardBlock ();
					}
					break;
				case 2: //Right
					AddTurnBlock ("Right");
					for (int i = 0; (i < randomLength - 1) && (lanesCount < drawDistance); ++i) {
						AddForwardBlock ();
					}
					break;
				}
			}
		}
	}

	void Update () {
		if(Input.GetKeyDown("escape")) Application.Quit();

		if (Vector3.Distance (playerObject.transform.position, builtLane[0].transform.position) >= destroyDistance) {
			Destroy (builtLane [0]);
			builtLane.RemoveAt (0);
			if (distanceToLastCurve >= minDistanceBetweenCurves) {
				int randomPercent = Random.Range (0, 101);
				if (randomPercent > chanceForCurves) {
					AddForwardBlock ();
				} else {
					Debug.Log ("Chance for curves");
					int randomChoice = Random.Range (0, 3);
					randomChoice = 1;
					switch (randomChoice) {
					case 0:
						AddForwardBlock ();
						break;
					case 1:
						if (numberOfLefts < 3)
							AddTurnBlock ("Left");
						else {
							if (numberOfRights < 2) {
								randomChoice = Random.Range (0, 2);
								if (randomChoice == 0)
									AddForwardBlock ();
								else
									AddTurnBlock ("Right");
							} else {
								AddForwardBlock ();
							}
						}
						break;
					case 2:
						if (numberOfRights < 3)
							AddTurnBlock ("Right");
						else {
							if (numberOfLefts < 2) {
								randomChoice = Random.Range (0, 2);
								if (randomChoice == 0)
									AddForwardBlock ();
								else
									AddTurnBlock ("Left");
							} else {
								AddForwardBlock ();
							}
						}
						break;
					}
				}
			} else {
				AddForwardBlock ();
			}
		}
	}

	void AddForwardBlock(){
		GameObject lastBuiltObject = builtLane[builtLane.Count - 1];
		GameObject laneObject = Instantiate (laneComponents.Center) as GameObject;

		laneObject.name = laneObject.name + " forward";
		laneObject.transform.SetParent (physicalLane.transform);

		laneObject.transform.rotation = lastBuiltObject.transform.rotation;

		Transform laneTransform = laneObject.transform;
		Vector3 lanePosition = laneTransform.position;
		Vector3 lanePieceSize = laneObject.GetComponent<Collider>().bounds.size;
		lanePosition.x = lastBuiltObject.transform.position.x + lanePieceSize.x * Mathf.RoundToInt(laneTransform.forward.x);
		lanePosition.y = 0;
		lanePosition.z = lastBuiltObject.transform.position.z + lanePieceSize.z * Mathf.RoundToInt(laneTransform.forward.z);
		laneObject.transform.position = lanePosition;

		builtLane.Add (laneObject);
		lanesCount++;
		distanceToLastCurve++;
	}

	void AddTurnBlock(string direction){
		if (direction == "Left") {
			Debug.Log ("Building a turn left");
			GameObject lastBuiltObject = builtLane[builtLane.Count - 1];
			GameObject laneObject = Instantiate (laneComponents.Left) as GameObject;

			laneObject.name = laneObject.name + " left";
			laneObject.transform.SetParent (physicalLane.transform);

			laneObject.transform.rotation = lastBuiltObject.transform.rotation;

			Transform laneTransform = laneObject.transform;
			Vector3 lanePosition = laneTransform.position;
			Vector3 lanePieceSize = laneObject.GetComponent<Collider>().bounds.size;
			lanePosition.x = lastBuiltObject.transform.position.x + lanePieceSize.x * Mathf.RoundToInt(laneTransform.forward.x);
			lanePosition.y = 0;
			lanePosition.z = lastBuiltObject.transform.position.z + lanePieceSize.z * Mathf.RoundToInt(laneTransform.forward.z);
			laneObject.transform.position = lanePosition;

			builtLane.Add (laneObject);
			lanesCount++;

			//Building the first straight block after the left turn
			lastBuiltObject = builtLane[builtLane.Count - 1];
			laneObject = Instantiate (laneComponents.Center) as GameObject;

			laneObject.name = laneObject.name + " forward";
			laneObject.transform.SetParent (physicalLane.transform);

			laneObject.transform.rotation = lastBuiltObject.transform.rotation;
			laneObject.transform.Rotate (Vector3.up, -90);

			Vector3 laneInitialOffset;
			laneInitialOffset.x = lastBuiltObject.GetComponent<Collider>().bounds.size.x * Mathf.RoundToInt(laneObject.transform.forward.x);
			laneInitialOffset.y = 0;
			laneInitialOffset.z = lastBuiltObject.GetComponent<Collider>().bounds.size.z * Mathf.RoundToInt(laneObject.transform.forward.z);

			laneTransform = laneObject.transform;
			lanePosition = laneTransform.position;
			lanePieceSize = laneObject.GetComponent<Collider>().bounds.size;
			lanePosition.x = lastBuiltObject.transform.position.x;
			lanePosition.y = 0;
			lanePosition.z = lastBuiltObject.transform.position.z;
			laneObject.transform.position = lanePosition + laneInitialOffset;

			builtLane.Add (laneObject);
			lanesCount++;
			numberOfLefts++;
			numberOfRights = 0;

		} else if (direction == "Right") {
			Debug.Log ("Building a turn right");		
			GameObject lastBuiltObject = builtLane[builtLane.Count - 1];
			GameObject laneObject = Instantiate (laneComponents.Right) as GameObject;

			laneObject.name = laneObject.name + " right";
			laneObject.transform.SetParent (physicalLane.transform);

			laneObject.transform.rotation = lastBuiltObject.transform.rotation;

			Transform laneTransform = laneObject.transform;
			Vector3 lanePosition = laneTransform.position;
			Vector3 lanePieceSize = laneObject.GetComponent<Collider>().bounds.size;
			lanePosition.x = lastBuiltObject.transform.position.x + lanePieceSize.x * Mathf.RoundToInt(laneTransform.forward.x);
			lanePosition.y = 0;
			lanePosition.z = lastBuiltObject.transform.position.z + lanePieceSize.z * Mathf.RoundToInt(laneTransform.forward.z);
			laneObject.transform.position = lanePosition;

			builtLane.Add (laneObject);
			lanesCount++;

			//Building the first straight block after the right turn
			lastBuiltObject = builtLane[builtLane.Count - 1];
			laneObject = Instantiate (laneComponents.Center) as GameObject;

			laneObject.name = laneObject.name + " forward";
			laneObject.transform.SetParent (physicalLane.transform);

			laneObject.transform.rotation = lastBuiltObject.transform.rotation;
			laneObject.transform.Rotate (Vector3.up, 90);

			Vector3 laneInitialOffset;
			laneInitialOffset.x = lastBuiltObject.GetComponent<Collider>().bounds.size.x * Mathf.RoundToInt(laneObject.transform.forward.x);
			laneInitialOffset.y = 0;
			laneInitialOffset.z = lastBuiltObject.GetComponent<Collider>().bounds.size.z * Mathf.RoundToInt(laneObject.transform.forward.z);

			laneTransform = laneObject.transform;
			lanePosition = laneTransform.position;
			lanePieceSize = laneObject.GetComponent<Collider>().bounds.size;
			lanePosition.x = lastBuiltObject.transform.position.x;
			lanePosition.y = 0;
			lanePosition.z = lastBuiltObject.transform.position.z;
			laneObject.transform.position = lanePosition + laneInitialOffset;

			builtLane.Add (laneObject);
			lanesCount++;
			numberOfRights++;
			numberOfLefts = 0;
		}

		distanceToLastCurve = 1;
	}
}
