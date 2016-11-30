using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player_Swipe : MonoBehaviour
{



    Rigidbody2D playerRigid;
    bool hitFlag,collFlag,stuckFlag;
    //public float leadPoints,moveSpeed;
    float swipeDir,curStamina,prevPosRefDistance;

    [SerializeField] float speed,maxDistance,distanceBorder,initialDrag,maxDrag,dragReduceRate,ceilingThreshold,ceilingDetectionRadius,wallDetectionRadius,deadMovementTresh,maxStamina,stamReduceRate,stuckTimeThresh,korgiScale,
        stamDragRate,ceilingGrabTime,univTimerRate,offsetDistanceColorValue;
    [SerializeField] private int maxBlinks,livesLeft,maxLives,blinksLeft;
    [SerializeField]Color32 color1,color2;
    [SerializeField] RectTransform lifeContainer;
    [SerializeField]  GameObject lifeIcon,dashSmoke,dropSmoke;  
    [SerializeField]
    GameObject[] lifeList = new GameObject[3];
    #region ghostingValues


    [SerializeField]
    float ghostSpawnRate, ghostEffectDuration;
    [SerializeField]
    int maxGhost;
    Assets.Scripts.game.sfx.GhostingContainer ghostCont;

    [SerializeField]
    Color32 ghostEffectTint;
    #endregion


    #region
    bool facingRight =true;
    Vector3 prevEndPoint, prevStartPoint,relativeEndPoint,stuckRefPoint,respawnPt;
    Vector2 targetPos,firstPos, sourcePt,prevPosRefPt;
    int playerStatus, inputOrient, playerPrevStatus, scrollPt;
    //BG_Control lvlCtrlr;
    Ray rayClickPoint, rayReleasePoint;
    Timer delay, clickDelay, stuckTimer, lineTimer, deathTimer;
    GameObject prjectile;
    projectile_Control prjectile_control;
    LineRenderer lineSample;
    GameObject sourcePt2;
    Animator animator;
    LayerMask Mask;
    SpriteRenderer spriteRenderer;
    static RigidbodyConstraints2D _constraint_Freeze = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    static RigidbodyConstraints2D _constraint_Normal = RigidbodyConstraints2D.FreezeRotation;
    #endregion
    [SerializeField]
    Transform areastartPt;
    [SerializeField] enum Status { isWaiting = 0, isBlinking, isOnGrab, onMove, isDropping, isFloating, isOnCeiling, isOnGlide, onHalt, isDead };


    Stack<GameObject> scrolls = new Stack<GameObject>();
    Vector3 normalScale;
    
    #region timers
    [SerializeField]Transform[] offsetTargets = new Transform[3];
    [SerializeField]Transform[] groundTargets = new Transform[2];
    [SerializeField]Transform[] wallDetectors = new Transform[5];
    #endregion
    [SerializeField]LayerMask wallMask, projectileMask;
  //  public GameObject[] Skills = new GameObject[3];
    public bool IS_CLEARED = false;
    public Transform playerPos{
        get { return this.transform; }
        
            }

    public Transform SET_AREA_PT
    {
        set {areastartPt = value; }
    }

    public int GET_AVAILABLE_LIVES
    {
        get { return livesLeft; }
    }




    void Awake()
    {
        playerRigid = this.GetComponent<Rigidbody2D>();
     //   lvlCtrlr = GameObject.FindGameObjectWithTag("GameController").GetComponent<BG_Control>();

        delay = new Timer();
        clickDelay = new Timer();
        stuckTimer = new Timer();
        lineTimer = new Timer();
        deathTimer = new Timer();

        playerStatus = (int)Status.isWaiting;
        
        
        lineSample = this.gameObject.GetComponent<LineRenderer>();

        animator = this.GetComponent<Animator>();
        lineSample.sortingLayerName = "Player";
        print(lineSample.sortingLayerName);
        collFlag = false;
        var mask = projectileMask;
        Mask = ~mask;
        spawnLife(maxLives);
        scrollPt = 0;
        normalScale = new Vector3(korgiScale, korgiScale, korgiScale);

    }
    void Start()
    {
        //Input.simulateMouseWithTouches = false;
        //  areastartPt = GameObject.Find("areaStart").transform;
          blinksLeft = maxBlinks;
          spriteRenderer = GetComponent<SpriteRenderer>();
          ghostCont = GetComponent<Assets.Scripts.game.sfx.GhostingContainer>();
          transform.position = areastartPt.position;
          playerPrevStatus = playerStatus;
       //   transform.SetParent(areastartPt);   //  parent to level chip
          transform.localScale = normalScale;
          Utilities.Trigger = false;

        // prjectile_control.Player = this.gameObject;

    }
    // Update is called once per frame

    void Update()
    {
        if(!imOnHalt() && imNotDead()&&!IS_CLEARED)
        if (delay.isActive())
        {
            delay.isFinished();
        }
        else
        if(!deathTimer.isActive())
            getInput();

        if (imOnHalt())
            dropAndWalk();
        else
            statusCheck();

        updateAnimator();
        if (Input.GetButtonDown("Jump"))
        {
            print("load");            
        }
    }


    void LateUpdate()
    {

    }

    void FixedUpdate()
    {

        // playerRigid.velocity = new Vector2(0, playerRigid.velocity.y);
        if (!imOnHalt() && imNotDead())
            if (!move()) 
            if (isGrounded())
                playerStatus = (int)Status.isWaiting;
            else;
        updateAnimator();

        //if (playerStatus == (int)Status.isOnGrab)
          //  playerRigid.velocity = Vector2.zero;

       
        //if()


    }

    void initGhost()
    {
        ghostCont.Init(maxGhost, ghostSpawnRate, spriteRenderer, ghostEffectDuration, ghostEffectTint);
    }


    void statusCheck()
    {
        switch (playerStatus)
        {
            case (int)Status.isBlinking:
                playerRigid.constraints = _constraint_Normal;
                transferPosition();

                break;

            case (int)Status.onMove:
                modifyDrag(initialDrag);
               
                //if (icantReach(playerRigid.position))
                // resetOrient();
                break;

            case (int)Status.isDropping:
                modifyDrag(initialDrag);
                
                if (isGrounded())
                {
                    var smkepos = offsetTargets[0].transform.position+(Vector3.up*-.1f)+(Vector3.right*-.1f);
                    produceSmoke(smkepos, dropSmoke, transform.localScale, Quaternion.identity); 
                    playerStatus = (int)Status.isWaiting;
                }
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case (int)Status.isWaiting:
                if (!isGrounded())
                    playerStatus = (int)Status.isDropping;

                if (deathTimer.isActive())
                   if( deathTimer.isFinished())
                        animator.SetLayerWeight(1, 0f);

                if (blinksLeft == 0)
                    blinksLeft = maxBlinks;
                ghostCont.StopEffect();
                break;

            case (int)Status.isFloating:
                modifyDrag(initialDrag);
                if (imOnWall(getWallDetectSet()))
                    imOnGrab();
                else if (imOnCeiling())
                    imOnGrab2();

                else if (playerRigid.velocity == Vector2.zero)
                    playerStatus = (int)Status.isDropping;

                break;

            case (int)Status.isOnCeiling:
                if (blinksLeft == 0)
                    blinksLeft = maxBlinks;

                if (stuckTimer.isFinished())
                {
                    playerRigid.constraints = _constraint_Normal;
                    //modifyDrag(maxDrag);
                    //playerStatus = (int)Status.isOnGlide;
                    playerStatus = (int)Status.isDropping;
                }
              
                break;

            case (int)Status.isOnGlide:
                if (dragNormalize())
                    playerStatus = (int)Status.isDropping;
                break;


            case (int)Status.isOnGrab:
                animator.SetBool("kinematic", playerRigid.isKinematic);
                if (blinksLeft == 0)
                    blinksLeft = maxBlinks;


                if (stuckTimer.isActive())
                    if (stuckTimer.isFinished())
                        playerRigid.constraints = _constraint_Normal;
                if (!imOnWall(getWallDetectSet()))
                    playerStatus = (int)Status.isDropping;

                    if (dragNormalize())
                    playerStatus = (int)Status.isDropping;
                //else
                    else if (imOnWall(getWallDetectSet()) && isGrounded())
                { 
                    playerStatus = (int)Status.isWaiting;
                    modifyDrag(initialDrag);
                }
                
                break;

            case (int)Status.isDead:
                playerRigid.constraints = _constraint_Normal;
                if (livesLeft > 0)
                    if (deathTimer.isFinished())
                        resetPlayer();
                    else;
                else if (livesLeft == 0)
                {
                    deathTimer.setTime(2f, univTimerRate);
                    livesLeft--;
                }
                else if (livesLeft == -1)
                    if (deathTimer.isFinished())
                        livesLeft--;
                break;



            case (int)Status.onHalt:
               
                break;

                 default:
                     break;

             }

         }

         void throwBlinkProjectile(Vector3 startValue, Vector3 endValue)
         {

         //    if (playerStatus == (int)Status.isWaiting || playerStatus == (int)Status.isOnGrab || playerStatus == (int)Status.isOnCeiling|| playerStatus == (int)Status.isDropping)
            // if(playerStatus!=(int)Status.onMove || playerStatus != (int)Status.isBlinking || playerStatus !=(int)Status.isDropping)
             //{
                 playerRigid.isKinematic = false;
                 relativeEndPoint = new Vector3(endValue.x - startValue.x, endValue.y - startValue.y, startValue.z); //get relative direction
                 swipeDir = Mathf.Sign(relativeEndPoint.x);
                 relativeEndPoint = new Vector3(relativeEndPoint.x + transform.position.x, relativeEndPoint.y + transform.position.y, 0);//apply to character
               //  Debug.DrawLine(rayClickPoint.GetPoint(0), rayReleasePoint.GetPoint(0), Color.green, 10f);
                 var pos = getRelativePos(rayClickPoint.GetPoint(0), rayReleasePoint.GetPoint(0), transform.position);
                 //if (Physics2D.Linecast(playerRigid.position, pos, Mask))

                 var pt = Physics2D.Linecast(playerRigid.position, pos, Mask);
                    if (pt.collider != null)
                     if (pt.distance < deadMovementTresh&&playerStatus==(int)Status.isWaiting)
                         return;


                 prevPosRefPt = pos;
                 prevPosRefDistance = Vector2.Distance(pos, playerRigid.position);
                 playerStatus = (int)Status.isBlinking;
                 // if (swipingDown(Vector2.up, Vector2.left)) ;
                // if (swipingDown(transform.position,pos)) ;

             //}

         }

        public void kill()
        {

        //    playerStatus = (int)Status.isDead;
        var num = livesLeft;
        for (int x = 0; x < num; x++)
        {
            //livesLeft--;
            // = (int)Status.isDead;
            GameObject.Destroy(lifeList[x]);
            livesLeft--;

        }

        
        shrinkColliders();
        //resetColliders();
        updateAnimator();

       // deathTimer.Deactivate();
           // ;


        }



         void getInput()
         {
             //check if tap
             int command = 99;
          //   if (Application.platform != RuntimePlatform.Android)
                 command = isTap();
            // else
              //   command = isTap2();


                 switch (command)
             { 
                 //0 - waiting
                 case 0: break;

                 //1 - check what phase are we (blinking?,onGrab?,inMiddleofBlink?)
                 case 1:
                     tapCases(playerStatus);
                     updateAnimator();
                     break;
                     //0 - waiting
                     //1 - blinking
                     //2 - grab
                 //2 - check release pont
                 case 2:
                     //check if length is sufficient
                     //checkLength();
                     if(blinksLeft > 0)
                        throwBlinkProjectile(prevStartPoint, prevEndPoint);
                     break;                       
                         //y - do swipe command
                         //n - ignore
             }
         }


         int isTap()
         {

             //detect click
             var inputPosStart = Vector3.zero;
             if (Input.GetMouseButtonDown(0))
             {
                 inputPosStart = Input.mousePosition; //raw screen point direction start
                 rayClickPoint = Camera.main.ScreenPointToRay(inputPosStart);

                 return 0;
                 // Debug.Log("firstClick");

             }

             if(Input.GetMouseButton(0))
             {
                 lineSample.SetPosition(0, this.transform.position);
                 var mouseEnd = Input.mousePosition; //raw screen point direction end
                 var mouseToWorld = Camera.main.ScreenPointToRay(mouseEnd);
                 var endMouse = mouseToWorld.GetPoint(28.2f);
                 var relativeEndPoint = getRelativePos(rayClickPoint.GetPoint(28.2f), endMouse, playerRigid.position);

                 var linePt = new Vector3(relativeEndPoint.x, relativeEndPoint.y,transform.position.z);

                 if (Vector2.Distance(linePt, transform.position) > (maxDistance + offsetDistanceColorValue))
                     reEditLine(transform.position, linePt);
                 else
                 { 
                     lineSample.SetColors(color1, color1);
                 lineSample.SetPosition(1,linePt);
                 }
                 // else
                 //  Ray rar = RaycastHit2D
                 //  print("held");

             }

             //if clickup before timer register as tap return true

             if (Input.GetMouseButtonUp(0) && !checkLength())
                 return 1;

             else if (Input.GetMouseButtonUp(0) && checkLength())
                 return 2;
             else
                 //Debug.Log("wait?");
                 return 0;

         }


         int isTap2()
         {

             //detect click
             var inputPosStart = Vector3.zero;
             var touch = Input.touches[0];
             if (touch.phase == TouchPhase.Began)
             {
                 inputPosStart = touch.position; //raw screen point direction start
                 rayClickPoint = Camera.main.ScreenPointToRay(inputPosStart);
                 // Debug.Log("firstClick");

             }
             //if clickup before timer register as tap return true

             if (!checkLength())
                 return 1;

             else if (checkLength())
                 return 2;
             else
                 //Debug.Log("wait?");
                 return 0;

         }

         void tapCases(int num)
         {
             switch (num)
             {
                 case (int)Status.isWaiting:
                     //waiting
                    // Debug.Log("on tap blinking");
                     break;
                 case (int)Status.isBlinking:
                     //blinking
                     //transferPosition();
                    // Debug.Log("blinking");
                     break;
                 case (int)Status.isOnGrab:
                     playerRigid.constraints = _constraint_Normal;
                     playerStatus = (int)Status.isDropping;
                     print("iclicked");
                     break;
                 case (int)Status.isOnCeiling:
                     modifyDrag(maxDrag);
                        playerRigid.constraints = _constraint_Normal;
                        // playerStatus = (int)Status.isOnGlide;
                        playerStatus = (int)Status.isDropping;
                     break;
                 case (int)Status.isOnGlide:
                     playerStatus = (int)Status.isDropping;
                     break;

                 case (int)Status.onHalt:
                     break;

                default:
                     break;


             }




         }

         void flip()
         {
             facingRight = !facingRight;
             var scale = this.transform.localScale;
             scale.x *= -1;
             transform.localScale = scale;


         }

         bool checkLength()
         {
             //  if (Application.platform != RuntimePlatform.Android)
             var inputPosEnd = Input.mousePosition; //raw screen point direction end

             //else
             // {

             //   var touch = Input.touches[0];
             //    if (touch.phase == TouchPhase.Ended)
             //        inputPosEnd = touch.position;
             //    else return false;
             // }
             rayReleasePoint = Camera.main.ScreenPointToRay(inputPosEnd);
            // rayReleasePoint = relativeEndPoint;
             prevEndPoint = rayReleasePoint.GetPoint(28.2f);
             prevStartPoint = rayClickPoint.GetPoint(28.2f);

             var pt1 = new Vector2(prevStartPoint.x, prevStartPoint.y);
             var pt2 = new Vector2(prevEndPoint.x, prevEndPoint.y);
             swipeDir = Mathf.Sign(prevEndPoint.x);

             //var distance = Vector2.Distance(prevStartPoint, prevEndPoint);
             var distance = Vector2.Distance(pt1, pt2);
             //////////////////////////////////////////
             lineSample.SetPosition(0, Vector3.zero);
             lineSample.SetPosition(1, Vector3.zero);
             /////////////////////////////////////////
             if (distance > 2f) //prevent tap detection           
                 return true;
             else return false; 
         }


         bool move()
         {
             if (playerStatus != (int)Status.onMove)
                 return true;      
             var force = Vector2.zero;
             //kuya james bakit ka nandito

             if (!facingRight)
                 force = (transform.right*-1f) * speed;
             else
                 force = transform.right * speed;      

             playerRigid.velocity = force;

           //  playerRigid.MovePosition(sourcePt2.transform.position);

             var distance = Vector2.Distance(sourcePt2.transform.position, targetPos);
             var distance2 = Vector2.Distance(firstPos, sourcePt2.transform.position);
             // print(distance);
             var desti = new Vector3(sourcePt2.transform.position.x, sourcePt2.transform.position.y, 0f);
             var source = new Vector3(transform.position.x, transform.position.y, 0f);
             Debug.DrawLine(source, desti, Color.green, 3f);


             if (distance <= distanceBorder || distance2 > maxDistance)
                 return resetOrient();
             else if (movementCheck(transform.position))
                 return resetOrient();
             else
                 return true;
         }

         bool resetOrient()
         {
             playerRigid.velocity = Vector2.zero;
        var up = transform.right;

        if (Mathf.Sign(transform.localScale.x) == -1)
            up *= -300f;
        else
            up *= 300f;

             if (!isGrounded())
                 playerRigid.AddForce(up);
             else
            {
                playerRigid.AddForce(up);
                return false;
            }
        //playerStatus = (int)Status.isFloating;


        if (imOnWall(getWallDetectSet()))
                 imOnGrab();
             else if (imOnCeiling())
                 imOnGrab2();
             else
                 playerStatus = (int)Status.isFloating;

             ghostCont.StopEffect();        
             return false;
         }

        void  produceSmoke(Vector2 pos,GameObject smokeType,Vector3 scale,Quaternion rotation )
        {
            var smoke  =   Instantiate(smokeType, pos,rotation)as GameObject;
            smoke.transform.localScale = scale;
            smoke.transform.SetParent(areastartPt);
        }
        


         bool movementCheck(Vector3 curPos)
         {
             if (curPos == stuckRefPoint)
                 if (!stuckTimer.active)
                     stuckTimer.setTime(stuckTimeThresh, 1.1f);
                    // stuckTimer.setTime(0.05f, 1.1f);

                 else if (stuckTimer.isFinished())
                 {
                     print("imstuck");
                     return true;// im stuck
                 }
                 else;
             else
             {
                 stuckTimer.Deactivate();
                 stuckRefPoint = curPos;
             }
             return false;
         }

         void transferPosition()
         {

             playerStatus = (int)Status.onMove;
             blinksLeft--;
            // sourcePt = playerRigid.position;
             sourcePt2 = this.gameObject;
             var hit = Vector2.zero;

             hit =   castBound(prevEndPoint);


        var pos = groundTargets[0].transform.position;
        var scale = dashSmoke.transform.localScale;
        if (Mathf.Sign(transform.localScale.x) < 1)
            scale.x *= -1f;

        if (isGrounded())
        {
            produceSmoke(pos, dashSmoke, scale, Quaternion.identity);
        }

        else
        {
            int[] set = { 0, 1, 2, 3 };
           /* 
            if (imOnWall(set))
            {
                var offset = Vector3.right * .9f;
                if (Mathf.Sign(transform.localScale.x) < 1)
                    pos += offset;
                else
                    pos -= offset;


                var rot = Quaternion.Euler(0f, 0f, -90f);
                produceSmoke(pos, dashSmoke, scale, rot);
            }*/
            
        }

        targetPos = hit;
             firstPos = playerRigid.position;
             stuckRefPoint = transform.position;
             stuckTimer.Deactivate();

         }
      Vector2 castBound(Vector2 prevPoint)
      {

             var mask = projectileMask;
             mask = ~mask;

            
             var relativePos = getRelativePos(rayClickPoint.GetPoint(0), prevPoint,playerRigid.position);
             sourcePt2 = this.gameObject;

             var collider = castRay(playerRigid.position, relativePos);
             fixOrient(playerRigid.position, relativePos);
             initGhost();
            // if (isGrounded())
           //  return collider;


             var distance = Vector2.Distance(collider, playerRigid.position);
             if(offsetTargets.Length!=0)
             foreach (Transform obj in offsetTargets)
             {
                 
                 relativePos = getRelativePos(rayClickPoint.GetPoint(0), prevPoint, obj.position);
            //     if (Physics2D.OverlapCircle(obj.position, 0.25f, mask))
                    // continue;
                 var hitRef = Physics2D.Linecast(obj.position, relativePos, mask);
                 var distanceRef = Vector2.Distance(hitRef.point,obj.position);
                 var sampCollider = castRay(obj.position, relativePos);

                 if (hitRef.collider != null)
                     Debug.DrawLine(hitRef.point, obj.position, Color.yellow, 2f);
            //    else
              //       Debug.DrawLine(obj.position, relativePos, Color.cyan, 4f);
                   //  Debug.DrawRay(obj.position, obj.transform.right, Color.cyan, 4f);

                 if (distanceRef<=distance && hitRef.collider!= null )
                 {
                     distance = distanceRef;
                     collider = sampCollider;
                     sourcePt2 = obj.gameObject;
                     
                 }

             }

             return collider;
         }

         void updateAnimator()
         {

             if (playerStatus != playerPrevStatus)
                 { 
                 animator.SetInteger("state", playerStatus);
                 playerPrevStatus = playerStatus;
                 }


         }

         bool imOnWall(int[] targetSet)
         {
        //  if (!collFlag)
        //   return false;

        // int[] left = { 0, 1 };
        // int[] right = { 2, 3 };
        // var numSet = right;
        var numSet = targetSet;
        //    obj = Physics2D.OverlapCircle(wallDetectors[numSet[x]].transform.position, wallDetectionRadius, Mask);

        // if (swipeDir==-1)
        //numSet = left;

        for (int x = 0; x < targetSet.Length; x++)
            if (Physics2D.OverlapCircle(wallDetectors[numSet[x]].transform.position, wallDetectionRadius, Mask))
            {
                snapToWall(numSet[x]); 
                return true;
            }


        return false;
         }

         bool imOnCeiling()
         {
             if (Physics2D.OverlapCircle(wallDetectors[4].transform.position, ceilingDetectionRadius, Mask))
                 if (!Physics2D.Raycast(playerRigid.transform.position, Vector2.down, ceilingThreshold, Mask))
            {
                snapToCeiling();
                return true;
            }
        return false;
         }

         void imOnGrab()
         {
             //playerRigid.isKinematic = true;
             playerStatus = (int)Status.isOnGrab;
             playerRigid.constraints = _constraint_Freeze;
             stuckTimer.setTime(.5f, 1.1f);
             // if (facingRight)
             flip();
             playerRigid.drag = maxDrag;
         }
         void imOnGrab2()
         {
             playerStatus = (int)Status.isOnCeiling;
        //playerRigid.isKinematic = true;
             playerRigid.constraints = _constraint_Freeze;
             stuckTimer.setTime(ceilingGrabTime, 1.1f);
         }


        int[] getWallDetectSet()
        {
            int[] left = { 0, 1 };
            int[] right = { 2, 3 };
            var numSet = right;
            if (swipeDir == -1)
                 numSet = left;

            return numSet;
        }

        void snapToWall(int num)
    {
        var dir = Vector2.right;
        if (num < 1)
            dir.x *= -1f;
        var wallPos = Physics2D.Raycast(transform.position, dir, 2f, Mask);
        var distanceRef = 0f;
        var vec = new Vector2(transform.position.x, transform.position.y);
        var rel = vec - wallPos.point;
        rel.x *= -1;

        distanceRef = Vector2.Distance(transform.position, wallPos.point);
        if (distanceRef <= 0.79f||wallPos.collider==null)
            return;
        var need = vec + (dir * (distanceRef - .7f));
        transform.position = need;
        Debug.DrawRay(transform.position, rel, Color.cyan, 10f);
    }


    void snapToCeiling()
    {
        var dir = Vector2.up;
        var ceilPos = Physics2D.Raycast(transform.position, dir, 2f, Mask);
        var distanceRef = 0f;
        var vec = new Vector2(transform.position.x, transform.position.y);
        var rel = vec - ceilPos.point;

        distanceRef = Vector2.Distance(transform.position, ceilPos.point);
        if ((distanceRef <= 1.2f && distanceRef>.9f)|| ceilPos.collider == null)
            return;
        var need = vec + (dir * (distanceRef - 1.2f));
        transform.position = need;
        Debug.DrawRay(transform.position, rel, Color.cyan, 10f);
    }

    

    void OnCollisionEnter2D(Collision2D coll)
         {
             if (playerStatus ==(int)Status.onMove)
             {
               ///  var num = Utilities.convertLayerTag(coll.collider.tag, 'w');
                /// if(num==0)
                   ///  collFlag = true;
             }
         }

         void OnCollisionExit2D()
         {
             collFlag = false;
         }


         void jumpAction(float val)
         {
             playerRigid.AddForce(Vector2.up * val);

         }

         void debugRestart()
         {
             Application.LoadLevel(Application.loadedLevel);
         }

         void Death()
         {
             if(playerStatus != (int)Status.isDead)
             { 

                 playerRigid.velocity = Vector2.zero;
                 playerStatus = (int)Status.isDead;

                shrinkColliders();

                jumpAction(500f);           
                 modifyDrag(1.5f);

                 updateAnimator();
                 loseLife();
                 livesLeft--;

                 if (livesLeft > 0)
                     deathTimer.setTime(2f, univTimerRate);
             }


         }


         void OnTriggerEnter2D(Collider2D other)
         {
             // print(other.tag);
             if (other.tag == "scroll")
             {
                 scrolls.Push(other.gameObject); 
                 other.gameObject.SetActive(false);
             }
             else if (other.tag == "exit")
             {
                 var col = other.GetComponent<BoxCollider2D>();
                 col.enabled = false;
                 playerStatus = (int)Status.isDropping;
                 IS_CLEARED = true;

             }

         }

         bool isGrounded()
         {
             var mask = projectileMask;
             mask = ~mask;

        for (int x = 0; x < groundTargets.Length; x++)
            if (Physics2D.OverlapCircle(groundTargets[x].position, 0.1f, mask))
                return true;
           // else
               // return false;

             return false;
         }

         void fixOrient(Vector2 source ,Vector2 targetPos)
         {
             var plyr2d = new Vector3(targetPos.x, targetPos.y, 0);
             var prj2d = new Vector3(source.x, source.y, 0);
             var target = prj2d - plyr2d;
             var rotation = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;

             if (plyr2d.x > prj2d.x && !facingRight)//if back pointing           
                 flip();
             else if (plyr2d.x < prj2d.x && facingRight)
                 flip();

             //rotation -= 180;
             if (facingRight)
                 rotation -= 180;

           //  if (isGrounded())
               ///  if (targetPos.y <= playerRigid.position.y)
                 ///   return;

             this.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
             updateAnimator();
         }

         Vector3 getRelativePos(Vector3 start,Vector3 end,Vector3 target)
         {
             var startMouse =start;
             var endMouse = end;
             var relativeEnd = new Vector3(endMouse.x - startMouse.x, endMouse.y - startMouse.y, 0); //get relative direction
             relativeEnd = new Vector3(relativeEnd.x + target.x, relativeEnd.y + target.y, 0);//apply to character
             return relativeEnd;

         }

         Vector2 castRay(Vector3 source ,Vector3 target)
         {
             var mask = projectileMask;
             mask = ~mask;
             var hit = Physics2D.Linecast(source, target, mask);
             // var hitRef = hit;
             if (hit.collider != null)
                 return hit.point;
             else
                 return target;

         }


         bool _castRay(Vector3 source, Vector3 target)
         {
             var mask = projectileMask;
             mask = ~mask;
             var hit = Physics2D.Linecast(playerRigid.position, target, mask);
             var distance = Vector2.Distance(source, hit.point);

             var distance2 = Vector2.Distance(source, target);

             var percent = distance / distance2;
             // var hitRef = hit;
             if (hit.collider != null&&percent<0.8f)           
                 return true;
             else
                 return false;

         }



         bool dragNormalize()
         {

            var rate = (dragReduceRate * Time.deltaTime);

             playerRigid.drag = Mathf.MoveTowards(playerRigid.drag, initialDrag, rate);

             if (playerRigid.drag == initialDrag)
                 return true;



             return false;
         }

         void modifyDrag(float value)
         {
             playerRigid.drag = value;
         }

         bool swipingDown(Vector2 source,Vector2 target)
         {
             var ptB = target - source;
             var angle = Vector2.Angle(source,target);
             var orientation = Mathf.Sign(source.x * ptB.y - source.y * ptB.x);

             return false;

         }

         bool imOnHalt()
         {
        if (Utilities.Trigger)            
            ghostCont.StopEffect();

            return Utilities.Trigger;
         }

    bool imNotDead()
    {
        if (playerStatus == (int)Status.isDead)
            return false;
        else
            return true;
    }

    float modifyStamina(float rate)
    {
        curStamina += rate;
        var staminaPercentCurrent = curStamina / maxStamina;
        return staminaPercentCurrent;
    }


    bool icantReach(Vector3 curPos)
    {
        var current = new Vector2(curPos.x, curPos.y);
        var curDistance = (Vector2.Distance(current, prevPosRefPt));

        if (prevPosRefDistance < curDistance)
            return true;
        else
        {
            prevPosRefDistance = curDistance;
        }
        return false;
    }

    public void spawnLife(int quantity,int offset=99)
    {
        if (offset != 99)
            livesLeft = offset;

        var curX = livesLeft*45;
        for(int x=0;x<quantity;x++)
        {
            if (livesLeft >= maxLives)
                return;
            var obj = Instantiate(lifeIcon);
            obj.transform.SetParent(lifeContainer, false);
            var rectTrans = obj.GetComponent<RectTransform>();
            rectTrans.anchoredPosition = new Vector2(rectTrans.anchoredPosition.x + curX, rectTrans.anchoredPosition.y);
            curX += 80;

            lifeList[x] = obj;

            livesLeft++;
        }
        // for()

    }

    void loseLife()
    {
        for (int x = (lifeList.Length - 1); x >= 0; x--)
        {
            if (lifeList[x] == null)
                continue;
            GameObject.Destroy(lifeList[x]);
            return;
        }
    }


    void reEditLine(Vector2 source,Vector2 destination)
    {
        lineSample.SetColors(color2, color2);
        var relative = destination-source;

        var trans = transform.TransformDirection(relative);
        relative = relative.normalized * (maxDistance+offsetDistanceColorValue);
        relative = source + relative;
        lineSample.SetPosition(1, relative);
    }


    public void resetPlayer()
    {
        playerStatus = (int)Status.isWaiting;
        resetColliders();
        facingRight=  true;       
        transform.localScale = new Vector3(korgiScale,korgiScale,korgiScale);
        transform.position = areastartPt.position;
        playerRigid.constraints = _constraint_Normal;
       // var num = animator.GetLayerIndex("spriteEffect");
        animator.SetLayerWeight(1, 1f);
        deathTimer.setTime(.5f, univTimerRate);

        foreach (GameObject obj in scrolls)
            obj.SetActive(true);

        updateAnimator();
    }

    void validateScrolls()
    {
        while(scrolls.Count!=0)
        {
            scrolls.Pop();
            scrollPt += 1;
        }
    }

    void dropAndWalk()
    {
        playerRigid.velocity = Vector2.up * playerRigid.velocity.y;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        if (!isGrounded())
            playerStatus = (int)Status.isDropping;
        else
        {
            playerStatus = (int)Status.onMove;        
            var pos = new Vector2(areastartPt.transform.position.x, playerRigid.position.y);
            transform.position = Vector2.MoveTowards(playerRigid.transform.position, pos, 5f*Time.deltaTime);
            var cur = new Vector2(transform.position.x, transform.position.y);
            if (cur == pos)
                playerStatus = (int)Status.isWaiting;
        }
       // updateAnimator();
    }
    
    void resetColliders()
    {
      

        var collider = this.GetComponents<CircleCollider2D>();
        foreach (CircleCollider2D circle in collider)
            circle.radius *= 2f;

    }
    void shrinkColliders()
    {
        var collider = this.GetComponents<CircleCollider2D>();
        foreach (CircleCollider2D circle in collider)
            circle.radius /= 2f;
    }














}
