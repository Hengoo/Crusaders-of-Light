using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

	PIDControllerVel velPID;
	PIDControllerRot rotPID;
	Rigidbody rbody;

	// Use this for initialization
	void Start()
	{
		rbody = this.gameObject.GetComponent<Rigidbody>();
		velPID = new PIDControllerVel(this.gameObject, 10, 0, 0.5f, new Vector3(1, 1, 1), 50f);
		//rotPID = new PIDControllerRot(this.gameObject, 100, 20 , 33);
		rotPID = new PIDControllerRot(this.gameObject, 38, 1, 14);

		inizializeRigidbody();
	}

	/// <summary>
	/// sets innertia tensor to (1,1,1) and center of mass to (0,0,0)
	/// </summary>
	private void inizializeRigidbody()
	{
		rbody.centerOfMass = Vector3.zero;
		rbody.inertiaTensor = Vector3.one * rbody.mass;
		rbody.maxAngularVelocity = 25;

	}


	// Update is called once per frame
	void FixedUpdate()
	{
		float speedfaktor = 10;
		Vector3 targetVel = new Vector3(Input.GetAxis("Horizontal") * speedfaktor, 0, Input.GetAxis("Vertical") * speedfaktor);
		velPID.UpdateTarget(targetVel, 1);

		Quaternion targetRot;
		Vector3 rightStick = new Vector3(Input.GetAxis("Horizontal2"), 0, -Input.GetAxis("Vertical2"));
		targetRot = Quaternion.LookRotation(rightStick, Vector3.up);
		rotPID.UpdateTarget(targetRot);
	}
}
