using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;

public class TouchInput : MonoBehaviour
{


    public LayerMask TouchInputMask;
    public GameObject[] SingleTouchReceiver;
    public GameObject[] DoubleTouchReceiver;
    public GameObject UIHandlerGO;

    public EventSystem GameEventSystem;


    private UIHandler _uiHandler;
    public void Awake()
    {
        _uiHandler = UIHandlerGO.GetComponent<UIHandler>();
    }



    public static float SidebarWidth = (1 - 0.7788769f);
    private bool _touchMoved = false;
    private Vector3 _oldMousePosition;

    //void Start(){ }
    private bool _clickstarted = false;
    //change to fixed??
    void Update()
    {
        Input.simulateMouseWithTouches = false;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _uiHandler.ShowEndGameDialog();
        }
        //#if UNITY_EDITOR

        //EventSystem.current.

        if (Input.mousePosition.x > (Screen.width - Screen.width * SidebarWidth))
        {
            //print("smaller");
            return;
        }
        //if ( GameEventSystem.IsPointerOverGameObject())
        //{
        //    //Debug.Log("left-click over a GUI element!");
        //    return;
        //}
        // wenn der button nicht mehr geklickt ist soll 
        if (!Input.GetMouseButton(0))
        {
            _clickstarted = false;
        }

        if (Input.GetMouseButton(0))
        {
            if (!_clickstarted)
            {
                //print("mousepressed:");
                _oldMousePosition = Input.mousePosition;
                _clickstarted = true;
            }

            if (_oldMousePosition != Input.mousePosition)
            {
                //print("mouse moved");
                _touchMoved = true;

                Vector3 deltaMousePos = _oldMousePosition - Input.mousePosition;
                deltaMousePos *= 0.5f;

                SendTouchMove(deltaMousePos);
                _oldMousePosition = Input.mousePosition;
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            if (!_touchMoved)
            {
                SendTouchClick(Input.mousePosition);
            }
            _touchMoved = false;
            _clickstarted = false;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            foreach (var receiver in DoubleTouchReceiver)
            {
                receiver.SendMessage("DoubleTouchAnywhere", -10, SendMessageOptions.DontRequireReceiver);
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
        {
            foreach (var receiver in DoubleTouchReceiver)
            {
                receiver.SendMessage("DoubleTouchAnywhere", 10, SendMessageOptions.DontRequireReceiver);
            }
        }

        //#endif

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            //if (GameEventSystem.IsPointerOverGameObject(touch.fingerId))
            //{
            //GameEventSystem.
            //return;
            //}

            if (touch.position.x > (Screen.width - Screen.width * SidebarWidth))
            {
                return;
            }
            TouchPhase phase = touch.phase;
            switch (phase)
            {
                case TouchPhase.Began:
                    _touchMoved = false;
                    break;

                case TouchPhase.Moved:
                    _touchMoved = true;
                    SendTouchMove(touch.deltaPosition);
                    break;

                case TouchPhase.Stationary:
                    SendTouchMove(touch.deltaPosition);
                    break;

                case TouchPhase.Ended:
                    if (!_touchMoved)
                    {
                        SendTouchClick(touch.position);
                    }
                    break;

                case TouchPhase.Canceled:
                    _touchMoved = false;
                    break;
                default:
                    _touchMoved = false;
                    break;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            if (touchZero.position.x > (Screen.width - Screen.width * SidebarWidth))
            {
                return;
            }

            //if (GameEventSystem.IsPointerOverGameObject(touchZero.fingerId) || GameEventSystem.IsPointerOverGameObject(touchOne.fingerId))
            //{
            //return;
            //}
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudediff = prevTouchDeltaMag - touchDeltaMag;

            foreach (var receiver in DoubleTouchReceiver)
            {
                receiver.SendMessage("DoubleTouchAnywhere", deltaMagnitudediff, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void SendTouchMove(Vector2 deltaPosition)
    {
        foreach (var receiver in SingleTouchReceiver)
        {
            receiver.SendMessage("SingleTouchMove", deltaPosition, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void SendTouchClick(Vector3 InputPos)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(InputPos);

        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);
        if (hits.Count() == 0)
        {
            _uiHandler.ClickedOnEmptySpace();
            return;
        }
        foreach (RaycastHit2D hit in hits)
        {
            GameObject recipient = hit.transform.gameObject;
            if (hits.Count() > 1 && recipient.CompareTag("ship"))
            { // should go throug and fire to planet only
                continue;
            }
            recipient.SendMessage("SingleTouchClick", SendMessageOptions.DontRequireReceiver);
        }

    }
}
