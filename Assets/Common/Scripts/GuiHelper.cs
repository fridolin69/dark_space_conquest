using UnityEngine;
using System.Collections;

/*
 * Helper
 * For drawing GUI's that resize well on all screens
 */

// Sets the skin of the GUI, and can be used to convert dimensions or draw buttons and co
public class GuiHelper : MonoBehaviour {

	public GUISkin skin;	
	public GUIStyle TitleStyle;		//Title only

	
	public float BigElemHeight { get; private set; }	
	public float BigElemSpacing { get; private set; }
	public float BigElemWidth { get; private set; }

	public float ElemHeight { get; private set; }	
	public float ElemSpacing { get; private set; }
	public float ElemWidth { get; private set; }
	public int ElemtFontSize { get; private set; }


	public GUIStyle SmallButtonStyle { get; private set; }
	public GUIStyle SmallLabelStyle { get; private set; }
	public float SmallElemHeight { get; private set; }	
	public float SmallElemWidth { get; private set; }
	public int SmallElemtFontSize { get; private set; }
	

	void Start () {		
		TitleStyle = new GUIStyle ();
		TitleStyle.normal.textColor = Color.white;
		TitleStyle.alignment = TextAnchor.UpperCenter;
	}


	// Update is called once per frame
	    void OnGUI () {		
		    Prepare();
		    //ButtonWidth = GuiHelper.X(180, 5, 40);
	    }


	//Sets the skin for an OnGUI-call
	    public void Prepare(){
		    GUI.skin = skin;

		    TitleStyle.fontSize =  (int)Mathf.Min( GuiHelper.Y(80, 5, 12), GuiHelper.X(80, 3, 8));

		    BigElemHeight = GuiHelper.Y(40, 10, 13);	
		    BigElemSpacing = GuiHelper.Y(22, 3, 8);
		    BigElemWidth = GuiHelper.X(650, 35, 100);	

		    ElemHeight = BigElemHeight;
		    ElemSpacing = BigElemSpacing * 0.4f;
		    ElemWidth = GuiHelper.X(400, 25, 48);		
		    ElemtFontSize = (int)(ElemHeight * 0.45f);

		    SmallButtonStyle = new GUIStyle("button");
		    SmallLabelStyle = new GUIStyle ("label");
		    SmallElemHeight = ElemHeight * 0.5f;
		    SmallElemWidth = ElemWidth/2;
		    SmallElemtFontSize = (int)(SmallElemHeight * 0.6);
		    SmallLabelStyle.fontSize = SmallElemtFontSize;
		    SmallLabelStyle.alignment = TextAnchor.MiddleLeft;

		    SmallButtonStyle.fontSize = SmallElemtFontSize;

            GUI.skin.button.fontSize = ElemtFontSize;
            GUI.skin.label.fontSize = ElemtFontSize;
            GUI.skin.textField.fontSize = ElemtFontSize;        
	    }



    //Returns the spacing, that should be used for the border of the game window
        public float GetWindowPadding(){
            return GuiHelper.XtoPx(2);
        }
	//Display the exit/return-button (lower left corner)
	    public bool ExitButton(string label){
		    return GUI.Button(new Rect(GetWindowPadding(), Screen.height-SmallElemHeight-GuiHelper.YtoPx(4), GuiHelper.X(220, 15, 40), SmallElemHeight), label, SmallButtonStyle);		
	    }

    //Display the next-button (lower right corner)
        public bool NextButton(string label) {
            float width = GuiHelper.X(220, 15, 40);
            return GUI.Button(new Rect(Screen.width - GetWindowPadding() - width, Screen.height - SmallElemHeight - GuiHelper.YtoPx(4), width, SmallElemHeight), label, SmallButtonStyle);
        }



	//Returns, how much space the exit-button needs (y, from the bottom)
	    public float GetExitButtonSpace(){
		    return SmallElemHeight + GetWindowPadding() * 2;
	    }

	//Display the exit/return-button (lower left corner)
	    public void TitleText(string text){
		    GUI.Label (new Rect (0, GuiHelper.Y(50, 1, 5), Screen.width, TitleStyle.fontSize), text, TitleStyle);		//50px, max. 5% of upper edge
	    }

	//Returns, how much space the title needs (y, from the top)
	    public float GetTitleSpace(){
		    return TitleStyle.fontSize + GuiHelper.Y (50, 1, 5) * 2; 
	    }



	//Returns a Rect of the size of a big element;    if fromBottom==true: the y-coordinate is measured from the bottom
	    public Rect BigElemRect(float x, float y, bool fromRight = false, bool fromBottom = false){
		    return new Rect (fromRight  ? (Screen.width  - BigElemWidth  - x ) : x, 
		                     fromBottom ? (Screen.height - BigElemHeight - y ) : y, 
		                     BigElemWidth, BigElemHeight);
	    }

	//Returns a Rect of the size of a big element, which is centered horizontally;    if fromBottom==true: the y-coordinate is measured from the bottom
	    public Rect BigCenterElemRect(float y, bool fromBottom = false){
		    return new Rect ((Screen.width - BigElemWidth) / 2, 
		                      fromBottom ? (Screen.height - BigElemHeight - y ) : y, 
		                      BigElemWidth, BigElemHeight);
	    }

	//Returns a Rect of the size of a normal element
	    public Rect ElemRect(float x, float y){
		    return new Rect (x, y, ElemWidth, ElemHeight);
	    }
	//Returns a Rect of the size of a small element
	    public Rect SmallElemRect(float x, float y, bool fromRight = false, bool fromBottom = false){
		    return new Rect (fromRight  ? (Screen.width  - SmallElemWidth  - x ) : x, 
		                     fromBottom ? (Screen.height - SmallElemHeight - y ) : y, 
		                     SmallElemWidth, SmallElemHeight);
	    }




	
	//Convert percent-values into px-values, depending on the screen resolution
	    public static float XtoPx(float percent){
		    return Screen.width * percent/100;
	    }
	    public static float YtoPx(float percent){
		    return Screen.height * percent/100;
	    }

	//get the x-value in px; use px. Use minP (%) if px are less. Use maxP (%) if px are bigger
	    public static float X(float px, float minP, float maxP){
		    float min=XtoPx(minP);
		    float max=XtoPx(maxP);		
		    if(px < min){
			    return min;
		    }
		    if(px > max){
			    return max;
		    }
		    return px;
	    }
	
	    public static float Y(float px, float minP, float maxP){
		    float min=YtoPx(minP);
		    float max=YtoPx(maxP);		
		    if(px < min){
			    return min;
		    }
		    if(px > max){
			    return max;
		    }
		    return px;
	    }


}
