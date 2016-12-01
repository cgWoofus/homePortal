using UnityEngine;
using System.Collections;

public class Camera_Control1 : MonoBehaviour {

    // Use this for initialization
    [SerializeField]
    Transform _level;

    [SerializeField]
    panDirection myDirection;


    [SerializeField]
    float _speed,_offset;

    [SerializeField]
    LayerMask layerMask;
   
    bool _isAdjusting=false;
    float _curSpeed = 0f, step;
    Player_Swipe player;

    Vector3 _targetPos,_curPos;

    public static Vector3 _nextPos;

    enum panDirection
    {
        pan01,pan02,pan03,pan04,pan05,pan06
    }


    EdgeCollider2D panArea;
    void Start()
    {
        panArea = this.GetComponent<EdgeCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Swipe>();

        _nextPos = _level.position;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {

       if (_isAdjusting)
            Adjust(_nextPos);


        if (panArea.IsTouchingLayers(layerMask))
            switch (myDirection)
            {
                case panDirection.pan01:
                    //check if pan02 is active if not set active
                    Camera_Parent.PANLINES[1].SetActive(true);
                    _curSpeed = _speed;
                    _targetPos =setPos((_level.right), player.transform.position.x-3.3f) + getCameraPosition(panDirection.pan01);
                    //_nextPos = setPos((_level.right), player.transform.position.x-2.8f) + getCameraPosition(panDirection.pan01);
                    Follow(_targetPos, _curSpeed);
                    step = 0;
                    _curPos = _level.transform.position;
                   // _isAdjusting = true;
                    break;
                    
               case panDirection.pan02:
                    //check if pan01 is active if not set active
                    Camera_Parent.PANLINES[0].SetActive(true);
                    _curSpeed = _speed;
                    _targetPos = setPos((_level.right), player.transform.position.x + 3.3f) + getCameraPosition(panDirection.pan01);
                   // _nextPos = setPos((_level.right), player.transform.position.x - 1f) + getCameraPosition(panDirection.pan01);
                    _curPos = _level.transform.position;
                    Follow(_targetPos, _curSpeed);
                    //_isAdjusting = true;
                    break;
                    
               case panDirection.pan03:
                    //make pan01 inactive
                    Camera_Parent.PANLINES[0].SetActive(false);
                    break;

               case panDirection.pan04:
                    //make pan02 inactive
                    Camera_Parent.PANLINES[1].SetActive(false);
                    break;

                case panDirection.pan05:
                    _curSpeed = _speed;
                    _targetPos = setPos((_level.up), player.transform.position.y-1.8f ) + getCameraPosition(panDirection.pan05);
                  //  _nextPos = setPos((_level.up), player.transform.position.y + 1f) + getCameraPosition(panDirection.pan05);
                    _curPos = _level.transform.position;
                    Follow(_targetPos, _curSpeed);
                   // _isAdjusting = true;
                    break;

                case panDirection.pan06:
                    _curSpeed = _speed;
                    _targetPos = setPos((_level.up), player.transform.position.y+1.8f) + getCameraPosition(panDirection.pan05);
                  //  _nextPos = setPos((_level.up), player.transform.position.y - 1f) + getCameraPosition(panDirection.pan05);
                    _curPos = _level.transform.position;
                    Follow(_targetPos, _curSpeed);
                   // _isAdjusting = true;
                    break;

            }
	}

    Vector3 setPos(Vector3 dir,float offset)
    {
       
        return dir * offset;

    }

    Vector3 getCameraPosition(panDirection pan)
    {
        var posX = Vector3.zero;var posY = Vector3.zero; var posZ = Vector3.zero;
        switch (pan)
        {

            case panDirection.pan01:
                posY = _level.position.y * Vector3.up;
                break;

            case panDirection.pan05:
                posX = _level.position.x * Vector3.right;
                break;
           

        }


        posZ = _level.position.z * Vector3.forward;      
        return Vector3.zero + posX + posY + posZ;

    }

    void Follow(Vector3 dir,float _spd)
    {
        _level.position = dir;
    }

    void Adjust(Vector3 dir)
    {
        var velocity = Vector3.zero;
       _level.transform.position = Vector3.SmoothDamp(_level.position, dir, ref velocity, 0.09f);

        if(_level.transform.position==dir)
        {
            //_curPos = _level.transform.position;
            //step = 0;
            _isAdjusting = false;

        }
    }


}
