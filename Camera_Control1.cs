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
    float _curSpeed = 0f;
    Player_Swipe player;

    Vector3 _targetPos;

    public static Vector3 _nextPos;

    enum panDirection
    {
        pan01,pan02,pan03,pan04
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
        if (panArea.IsTouchingLayers(layerMask))
            switch (myDirection)
            {
                case panDirection.pan01:
                    //check if pan02 is active if not set active
                    Camera_Parent.PANLINES[1].SetActive(true);
                    _curSpeed = _speed;
                    _targetPos = _level.position + setPos((_level.right), _offset);
                    _nextPos = _targetPos + setPos((_level.right), _offset * 3);
                    Follow(_targetPos, _curSpeed);
                    _isAdjusting = true;
                    break;
               case panDirection.pan02:
                    //check if pan01 is active if not set active
                    Camera_Parent.PANLINES[0].SetActive(true);
                    _curSpeed = _speed;
                    _targetPos = _level.position + setPos((_level.right * -1), _offset);
                    _nextPos = _targetPos + setPos((_level.right*-1), _offset * 3);
                    Follow(_targetPos, _curSpeed);
                    _isAdjusting = true;
                    break;

               case panDirection.pan03:
                    //make pan01 inactive
                  //  _isAdjusting = false;
                  //  _nextPos = _level.position;
                    Camera_Parent.PANLINES[0].SetActive(false);
                    break;

               case panDirection.pan04:
                    //make pan02 inactive
                  //  _isAdjusting = false;
                   // _nextPos = _level.position;
                    Camera_Parent.PANLINES[1].SetActive(false);
                    break;

            }
      //  else
           //  if (_isAdjusting)
                  //  Adjust(_nextPos, _speed);
	}

    Vector3 setPos(Vector3 dir,float offset)
    {
       
        return dir * offset;

    }

    void Follow(Vector3 dir,float _spd)
    {
        _level.position = dir;
    }

    void Adjust(Vector3 dir, float _spd)
    {
      
        if (_level.position == dir)
         _isAdjusting = false;

        _level.position = Vector3.MoveTowards(_level.position,dir, Time.deltaTime * _spd);


    }


}
