using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour {

    public Color defaultColour;
    public Color selectedColour;
    private Material mat;

    private Vector3 targetPos;

    
    
	// Use this for initialization
	void Start () {
        targetPos = this.transform.position;
        mat = renderer.material;
	}

    void Update()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, targetPos, Time.deltaTime * 500);
    }


    void OnTouchDown()
    {
        mat.color = selectedColour;
    }

    void OnTouchUp()
    {
        mat.color = defaultColour;
    }

    void OnTouchStay(Vector3 point)
    {
        targetPos = new Vector3(point.x,point.y,targetPos.z);
         mat.color = selectedColour;
    }
    void OnTouchExit()
    {
        mat.color = defaultColour;
    }

    void SingleTouchClick() 
    {
    print("ihaaaa");    
    }
}
