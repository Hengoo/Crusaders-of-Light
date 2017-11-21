using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

	PID.PIDControllerVel velPID;
	PID.PIDControllerRot rotPID;
	Rigidbody rbody;

	Quaternion oldRot;

	// Use this for initialization
	void Start()
	{
		rbody = this.gameObject.GetComponent<Rigidbody>();
		velPID = new PID.PIDControllerVel(this.gameObject, 10, 0, 0.5f, new Vector3(1, 0, 1), 50f);
		//rotPID = new PIDControllerRot(this.gameObject, 100, 20 , 33);
		rotPID = new PID.PIDControllerRot(this.gameObject, 38, 1, 14);

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

	void FixedUpdate()
	{
		float speedfaktor = 10;
		Vector3 targetVel = new Vector3(Input.GetAxisRaw("Horizontal"), -0.3f, Input.GetAxisRaw("Vertical")) * speedfaktor;
		velPID.UpdateTarget(targetVel, 1);

		Quaternion targetRot;
		Vector3 rightStick = new Vector3(Input.GetAxisRaw("Horizontal2"), 0, -Input.GetAxisRaw("Vertical2"));
		if(rightStick.magnitude > 0.2f)
		{
			targetRot = Quaternion.LookRotation(rightStick, Vector3.up);
			targetRot = Quaternion.Slerp(targetRot, oldRot, 0.3f);
			oldRot = targetRot;
		}
		else
		{
			targetRot = oldRot;
		}
		rotPID.UpdateTarget(targetRot);
	}
}
