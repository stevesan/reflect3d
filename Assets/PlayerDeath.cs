using UnityEngine;
using System.Collections;

public class PlayerDeath : MonoBehaviour {
	
	public float m_fallDurationBeforeDeath = 5.0f;
	
	float m_lastTimeOnGround = 0.0f;
	
	void Respawn() {
		GameController.main.ActivateLevel("0-hub");
		m_lastTimeOnGround = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Physics.Raycast(new Ray(transform.position, -Vector3.up), 5.0f)) {
			m_lastTimeOnGround = Time.time;	
		}
		
		if (Time.time - m_lastTimeOnGround > m_fallDurationBeforeDeath) {
			Respawn();
		}
	}
}
