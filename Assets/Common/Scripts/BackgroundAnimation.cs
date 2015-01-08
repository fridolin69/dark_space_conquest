using UnityEngine;
using System.Collections;

public class BackgroundAnimation : MonoBehaviour {

	public float animationSpeed = 50.0f;
	public float maxScale = 50.0f;



	// Use this for initialization
	void Start () {

		
	}
	
	// Update is called once per frame
	void Update () {


		if (transform.localScale.x > maxScale) {
			transform.localScale = new Vector3 (2, 2, 2);// initialScale;//new Vector3 (32, 32, 32);
			transform.Rotate(new Vector3(0, 0, Random.Range(0.0f, 360.0f)));
		}else{
			transform.localScale += new Vector3 (1, 1, 1) * animationSpeed * Time.deltaTime * transform.localScale.x ;
		}
		Color clr = Color.white;
		if (transform.localScale.x < 20) {
			clr.a = transform.localScale.x/20.0f;		//Slowly increase the transparency
		}
		renderer.material.color =clr;


		Vector3 pos = transform.position;
		pos.z = transform.localScale.x;
		transform.position = pos;
	}
}
