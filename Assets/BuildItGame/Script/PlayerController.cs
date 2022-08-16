using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform headPos;
    [SerializeField] private Transform pickPoint;

    public Vector3 originOffset;
    public float height = 0.5f;
    public LayerMask groundMask;

    private float slopeLimitAngle = 120f;
    private float slopeAngle;
    private RaycastHit hitInfo;
    private bool isGround;

    private Rigidbody rb;
    private CapsuleCollider colliderObj;
    private Animator anim;

    public float sensitivity;
    public float turnTreshold;
    public float speed;
    public float turnSpeed;


    private Vector3 curDir;
    private Vector3 mouseStartPos;
    private Quaternion targetRot;

    public TextMeshProUGUI totalCountText;
    public GameObject brickPrefab;
    public float brickLimit;
    public float pickupLerpTime;
    public bool varyLerp;
    private List<GameObject> pickedBrickList = new List<GameObject>();

    private const string runAnim = "isRunning";
    private const string victoryAnim = "Victory";
    private const string carryAnim = "carryingStack";

    public Vector3 forward { get; private set; }
    public bool canPickUpBrick => pickedBrickList.Count < brickLimit;
    public Vector3 lastBrickPos
    {
        get
        {
            if (pickedBrickList.Count > 0)
            {
                return pickPoint.position + (offset * pickedBrickList.Count);
            }
            return pickPoint.position;
        }
    }

    private Vector3 offset;
    private void Awake()
    {
        offset = new Vector3(0, (brickPrefab.transform.localScale.y + 0.03f), 0);
    }
    private void Start()
    {
        SetReference();
    }
    private void Update()
    {
        GetInput();
    }
    private void FixedUpdate()
    {
        CalculateForwardAngle();
        CheckGround();
        PlayerMovement();
        MaintainBricks();
    }

    void RestText()
    {
        totalCountText.gameObject.SetActive(false);
    }

    private void SetReference()
    {
        rb = GetComponent<Rigidbody>();
        colliderObj = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();

        anim.applyRootMotion = false;
        anim.SetBool(carryAnim, true);
    }
    private void GetInput()
    {
        var mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPos = mousePos;
        }
        else if (Input.GetMouseButton(0))
        {
            float distance = (mousePos - mouseStartPos).magnitude;
            if (distance > turnTreshold)
            {
                if (distance > sensitivity)
                {
                    mouseStartPos = mousePos - (curDir * sensitivity / 2f);
                }

                var curDir2D = (mousePos - mouseStartPos).normalized;
                curDir = new Vector3(curDir2D.x, 0, curDir2D.y);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            curDir = Vector3.zero;
        }
    }

    private void PlayerMovement()
    {
        if (curDir != Vector3.zero)
        {
            anim.SetBool(runAnim, true);
            if (slopeAngle < slopeLimitAngle)
            {
                transform.position += new Vector3(forward.x, forward.y, forward.z) * speed * Time.deltaTime;
                // rb.AddForce(forward * speed, ForceMode.VelocityChange);
            }
            targetRot = Quaternion.LookRotation(curDir);
            if (rb.rotation != targetRot)
            {
                rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed);
            }
        }
        else
        {
            anim.SetBool(runAnim, false);
            // rb.velocity = Vector3.zero;
            // rb.angularVelocity = Vector3.zero;
        }

        // anim.SetBool(carryAnim, true);// pickedBrickList.Count > 0);
    }


    private void CalculateForwardAngle()
    {
        if (!isGround)
        {
            forward = transform.forward;
            slopeAngle = 90;
            return;
        }

        forward = Vector3.Cross(transform.right, hitInfo.normal);
        slopeAngle = Vector3.Angle(hitInfo.normal, transform.forward);
    }

    private void CheckGround()
    {
        isGround = Physics.Raycast(transform.position + originOffset, transform.TransformDirection(Vector3.down), out hitInfo, height, groundMask);
    }

    private void ApplyGravity()
    {
        if (!isGround)
        {
            transform.position += Physics.gravity * Time.deltaTime;
        }
    }

    IEnumerator RotateTowards(Quaternion targetRot)
    {
        float progress = 0;
        while (rb.rotation != targetRot)
        {
            transform.localRotation = Quaternion.Lerp(rb.rotation, targetRot, progress);
            progress += Time.deltaTime * 2f;
            yield return null;
        }
    }
    private void ApplyAlwaysGravity()
    {
        RaycastHit hit;
        bool isHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity);
        if (isHit)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }

    public void StopPlayer(bool isWin)
    {
        anim.SetBool(runAnim, false);
        colliderObj.enabled = false;

        if (isWin)
        {
            Vector3 endPos = Camera.main.transform.position * -1f;
            endPos.y = transform.position.y;
            Quaternion targRot = Quaternion.LookRotation(endPos);

            StartCoroutine(RotateTowards(Quaternion.identity));

            anim.Play(victoryAnim);
        }
    }
    public void PickUpBrick()
    {

        GameObject brickObj = Instantiate(brickPrefab, pickPoint.position, Quaternion.identity);
        pickedBrickList.Add(brickObj.gameObject);
        brickObj.gameObject.tag = "Untagged";
        brickObj.transform.SetParent(null);

        totalCountText.gameObject.SetActive(true);
        totalCountText.text = "+" + pickedBrickList.Count;
        Invoke(nameof(RestText), .9f);
    }
    public GameObject DropBrick()
    {
        if (pickedBrickList.Count == 0)
        {
            return null;
        }

        GameObject obj = pickedBrickList[pickedBrickList.Count - 1];
        obj.transform.parent = null;
        pickedBrickList.RemoveAt(pickedBrickList.Count - 1);
        return obj;
    }

    private void MaintainBricks()
    {
        for (int i = 0; i < pickedBrickList.Count; i++)
        {
            pickedBrickList[i].transform.rotation = this.transform.rotation;
            if (i != 0)
            {
                pickedBrickList[i].transform.position = Vector3.Lerp(
                    pickedBrickList[i].transform.position,
                    pickedBrickList[i - 1].transform.position + offset,
                    pickupLerpTime);
            }
            else
            {
                pickedBrickList[i].transform.position = pickPoint.position;
            }

        }
    }
}


