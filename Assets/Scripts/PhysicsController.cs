using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController
{

	PID.PIDControllerVel velPID;
	PID.PIDControllerRot rotPID;
	Rigidbody rbody;

	Quaternion oldRot;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="gObject">The gameobject that has the rigidbody</param>
	public PhysicsController(GameObject gObject)
	{
		rbody = gObject.GetComponent<Rigidbody>();
		velPID = new PID.PIDControllerVel(gObject, 10, 0, 0.5f, new Vector3(1, 0, 1), 50f);
		//rotPID = new PIDControllerRot(this.gameObject, 100, 20 , 33);
		rotPID = new PID.PIDControllerRot(gObject, 38, 1, 14);
		oldRot = Quaternion.LookRotation(Vector3.right, Vector3.up);

		inizializeRigidbody();
	}

	/// <summary>
	/// sets innertia tensor to (1,1,1) and center of mass to (0,0,0)
	/// </summary>
	private void inizializeRigidbody()
	{
		//rbody.centerOfMass = Vector3.zero;
		rbody.inertiaTensor = Vector3.one * rbody.mass;
		rbody.maxAngularVelocity = 25;

	}

	/// <summary>
	/// needs to be called every physics tick
	/// the new forwardDirection is ignored when forwardDirection.magnitude is to small (stick is not moved)
	/// the rotation is also slerped
	/// </summary>
	public void SetVelRot(Vector3 targetVel, Vector3 forwardDirection)
	{
		//velocity
		velPID.UpdateTarget(targetVel, 1);

		Quaternion targetRot;
		if (forwardDirection == Vector3.zero)
		{
			targetRot = oldRot;
		}
		else
		{
			//rotation
			targetRot = Quaternion.LookRotation(forwardDirection, Vector3.up);
			if (forwardDirection.magnitude > 0.2f)
			{
				targetRot = Quaternion.LookRotation(forwardDirection, Vector3.up);
				targetRot = Quaternion.Slerp(targetRot, oldRot, 0.9f);
				oldRot = targetRot;
			}
			else
			{
				targetRot = oldRot;
			}
		}
		rotPID.UpdateTarget(targetRot);
	}

	/// <summary>
	/// possible way to get the targetvel and forwardDirection
	/// was used to controll the character
	/// </summary>
	void ExampleFixedUpdate()
	{
		float speedfaktor = 10;
		//left stick
		Vector3 targetVel = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * speedfaktor;

		//right stick
		Vector3 targetDir = new Vector3(Input.GetAxisRaw("Horizontal2"), 0, -Input.GetAxisRaw("Vertical2"));

		SetVelRot(targetVel, targetDir);
	}
}
