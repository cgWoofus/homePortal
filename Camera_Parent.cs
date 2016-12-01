using UnityEngine;
using System.Collections;

public class Camera_Parent : MonoBehaviour {

    // Use this for initialization

    [SerializeField]
    GameObject[] panLines;

    public static GameObject[] PANLINES;
    float step = 0f;
    Vector3 _curPos;
	void Start ()
    {
        PANLINES = panLines;
        _curPos = this.transform.position;
	}

    void FixedUpdate()
    {
       // if(Camera_Control1._nextPos!=_curPos)
         //   Adjust(Camera_Control1._nextPos,3f);

    }

    void Adjust(Vector3 dir, float _spd)
    {

        // if (_level.position == dir)
        //_isAdjusting = false;
        step += Mathf.Clamp01(Time.deltaTime * _spd);
        this.transform.position = Vector3.Lerp(_curPos, dir, step);

        if(step>=1)
        {
            _curPos = transform.position;
            step = 0;

        }


    }

}
