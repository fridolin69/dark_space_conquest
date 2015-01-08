using UnityEngine;
using System.Collections;

//Note this class depends on the map beeing scaled to 1.
public class MapScaler : MonoBehaviour
{
    public const float OrthoZoomSpeed = 0.4f;
    public const float ScrollAnimationSpeed = 4.5f;

    public const float CameraBorderOffset = 1;
    public const float PreventShakeOffset = 2.5f;

    private const float SidePanelFactor = 1 - 0.7724556f;

    private float _cameraMinOrthoSize;
    private float _cameraMaxOrthoSize;

    private float _cameraWidthHalf;
    private float _cameraHeightHalf;

    private Map _map;
    private float _smallestPlanetDiameter;
    private Vector3 _moveToPosition = new Vector3(0, 0, 0);

    private Vector2 FindHomePlanet()
    {
        int _planetCount = _map.GetPlanetCount();
        for (int i = 0; i < _planetCount; ++i)
        {
            PlanetEntity planet = _map.GetPlanetByIndex(i);
            if (planet.owner != null &&  planet.owner.networkPlayer == Network.player)
            {   //gehört mir..
                print(planet.position);
                _map.SetPosition(new Vector2(0,0),1);
                return _map.GetPosition() - (planet.position + new Vector2(planet.diameter * 2, planet.diameter*2));
            }
        }
        return new Vector2(_map.GetPosition().x - _cameraWidthHalf, _map.GetPosition().y - _cameraHeightHalf);
    }

    void Awake()
    {
        _map = GameObject.Find("Map").GetComponent<Map>();
        if (_map == null)
        {
            throw new MissingComponentException("Unable to find Init Item.");
        }

        _cameraWidthHalf = Camera.main.orthographicSize * Camera.main.aspect;
        _cameraHeightHalf = Camera.main.orthographicSize;

        _smallestPlanetDiameter = _map.GetSmallestDiameter();

        RecalculateCameraMinAndMaxSize();

        float cameraInitOrthoSize = _cameraMaxOrthoSize* 2 / 3;
        Camera.main.orthographicSize = cameraInitOrthoSize;


        _moveToPosition = FindHomePlanet();

        Vector2 screenCenter = new Vector2( -_cameraWidthHalf, -_cameraHeightHalf  );
        
        _map.SetPosition(screenCenter,1);

        
        _map.SetPosition(
            Vector2.Lerp(
                _map.GetPosition(),
                _moveToPosition,
                ScrollAnimationSpeed * Time.smoothDeltaTime
                ),1 // reset scale (from menu)
            );
       
    }


    void SingleTouchMove(Vector2 acceleration)
    {

        float orthoDist = _cameraMaxOrthoSize - _cameraMinOrthoSize;
        float multiplicand = Camera.main.orthographicSize / orthoDist;
        multiplicand *= 0.45f;
        acceleration.x *= multiplicand;
        acceleration.y *= multiplicand;

        //print("SingleTouchAnywhere: " + acceleration.x + " | " + acceleration.y);
        _moveToPosition.x += acceleration.x ;
        _moveToPosition.y += acceleration.y ;
    }


    void DoubleTouchAnywhere(float deltaMagnitudediff)
    {
        deltaMagnitudediff *= .55f;
        Camera.main.orthographicSize += deltaMagnitudediff * OrthoZoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, _cameraMinOrthoSize, _cameraMaxOrthoSize);
    }

    public void FixedUpdate()
    {
        RecalculateCameraMinAndMaxSize();

        CorrectPosition();

        _map.SetPosition(
            Vector2.Lerp(
                _map.GetPosition(),
                _moveToPosition,
                ScrollAnimationSpeed * Time.deltaTime
                )
            );
    }


    void OnGUI()
    {
        //dummy method to clear away the buggy messages -.- fuck you unity
    }
    private void RecalculateCameraMinAndMaxSize()
    {
        Vector2 mapSize = _map.GetMapSize();


        float minOrthoSizeX = ((mapSize.x + mapSize.x *SidePanelFactor) / Camera.main.aspect / 2) - PreventShakeOffset; //-1 that the camera doesn't shake
        float minOrthoSizeY = (mapSize.y / 2) - PreventShakeOffset;

        _cameraMinOrthoSize = _smallestPlanetDiameter * 4f;
        _cameraMaxOrthoSize = Mathf.Min(minOrthoSizeX, minOrthoSizeY);

        //_cameraMinOrthoSize = 16;
        //_cameraMaxOrthoSize = 50;
    }

    // used to avoid black borders around the map
    private void CorrectPosition()
    {
        _cameraWidthHalf = Camera.main.orthographicSize * Camera.main.aspect;
        _cameraHeightHalf = Camera.main.orthographicSize;

        float cameraXMin = (Camera.main.transform.position.x - _cameraWidthHalf);
        float cameraXMax = (Camera.main.transform.position.x + _cameraWidthHalf - (_cameraWidthHalf * 2 * SidePanelFactor) + 1);
        float cameraYMin = (Camera.main.transform.position.y - _cameraHeightHalf);
        float cameraYMax = (Camera.main.transform.position.y + _cameraHeightHalf);

        if (cameraXMin < _map.GetPosition().x)
        {   //print("left");
            _moveToPosition.x = (cameraXMin - CameraBorderOffset); // push a little bit over the left edge
        }

        else if (cameraXMax > ( _map.GetPosition().x + _map.GetMapSize().x) )
        {   //print("right");
            _moveToPosition.x = (cameraXMax + CameraBorderOffset) - _map.mapBounds.width;
        }

        if (cameraYMin < _map.GetPosition().y) // check bottom
        {   //print("bottom");
            _moveToPosition.y = (cameraYMin - CameraBorderOffset);
        } 
        else if (cameraYMax > (_map.GetPosition().y + _map.mapBounds.height)) // check top
        {   //print("top");
            _moveToPosition.y = (cameraYMax + CameraBorderOffset) - _map.mapBounds.height;
        }

    }


    void OnDestroy() {
        Destroy( GameObject.Find("Networking" ) );  //Delete the map, the playerList and everythin else, because we are leaving the Game-Scene (This is needed, because that GameObject is imortable due to "DontDestroyOnLoad()")
    }
}


