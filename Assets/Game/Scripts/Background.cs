using UnityEngine;
using System.Collections;

public class Background : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnUpdateMe(Vector3 acceleration)
    {
        print("in updateme... ");
        //this.transform.Translate(acceleration.x + Time.deltaTime * 10, -acceleration.y * Time.deltaTime * 10,
        //    0);
        //Time.deltaTime
    }
}
