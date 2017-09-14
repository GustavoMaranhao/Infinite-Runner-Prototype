using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
	public float maxSpeed = 5f;
	public float playerAcceleration = 2f;
	public float playerTurnSpeed = 1f;

	private Rigidbody playerRigidBody;

	private bool isTurningLeft = false;
	private bool isTurningRight = false;
	private Quaternion targetRotation;

	private int strafePosition = 0;

	void Start () {
		playerRigidBody = gameObject.GetComponent<Rigidbody> ();
	}

	void Update () {
		bool strafeLeft = Input.GetKeyDown ("a"); 
		bool strafeRight = Input.GetKeyDown("d");
		if ((strafeLeft || strafeRight) && !(isTurningLeft || isTurningRight)) {
			bool shouldStrafe = false;
			if (strafeLeft && strafePosition >= -1) {
				strafePosition--;
				shouldStrafe = true;
			}
			if (strafeRight && strafePosition <= 1) {
				strafePosition++;
				shouldStrafe = true;
			}
			if (shouldStrafe) {
				Vector3 newPosition = playerRigidBody.position;
				newPosition.x += playerRigidBody.transform.right.x * 0.8f * strafePosition;
				newPosition.y = playerRigidBody.transform.position.y;
				newPosition.z += playerRigidBody.transform.right.z * 0.8f * strafePosition;
				playerRigidBody.MovePosition (newPosition);
			}
		} 

		if (isTurningLeft || isTurningRight) {
			if (isTurningLeft) {
				Quaternion q = Quaternion.AngleAxis (-playerTurnSpeed, transform.up) * transform.rotation;
				playerRigidBody.MoveRotation(q);
			}
			if (isTurningRight) {
				Quaternion q = Quaternion.AngleAxis (playerTurnSpeed, transform.up) * transform.rotation;
				playerRigidBody.MoveRotation(q);
			}
			float speedMagnitude = playerRigidBody.velocity.magnitude;
			playerRigidBody.velocity = transform.forward * speedMagnitude;

			if (Quaternion.Angle (playerRigidBody.transform.rotation, targetRotation) <= playerTurnSpeed) {
				Vector3 tempForward;
				tempForward.x = Mathf.RoundToInt (transform.forward.x);
				tempForward.y = 0;
				tempForward.z = Mathf.RoundToInt (transform.forward.z);
				transform.forward = tempForward;
				playerRigidBody.velocity = transform.forward * speedMagnitude;
				isTurningLeft = false;
				isTurningRight = false;
			}
		} else {
			if (playerRigidBody.velocity.magnitude < maxSpeed) {
				playerRigidBody.AddForce (transform.forward * playerAcceleration);
			}
		}
	}

	void OnTriggerEnter (Collider col){		
		if (col.gameObject.tag == "CornerEntranceLeft") {
			isTurningLeft = true;
			targetRotation = transform.rotation * Quaternion.AngleAxis(-90f, transform.up);
		} else {
			if (col.gameObject.tag == "CornerEntranceRight") {
				isTurningRight = true;
				targetRotation = transform.rotation * Quaternion.AngleAxis(90f, transform.up);
			}
		}
	}
}
