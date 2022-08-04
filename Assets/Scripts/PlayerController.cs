using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform viewPoint;
    public float mouseSensitivity = 1.0f;
    private float verticalRotStore;
    private Vector2 mouseInput;
    public float minLookAngle = -60f;
    public float maxLookAngle = 60f;
 //   public bool invertX;
//    public bool invertY;
    public float moveSpeed = 5;
    private Vector3 moveDir, movement;
    public CharacterController charCon;
    //bool Jumping;
    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayer;
    private Camera FPScam;
    public float jumpForce = 3f, gravityMod = 2f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        FPScam = Camera.main;
    }
     void Update()
    {

        if(photonView.IsMine){
            playerRotation();
            playerMovement();
            if(Input.GetKey(KeyCode.LeftShift)){
                playerMovementSprint();
            }
        playerJump();
        }

        if(Input.GetMouseButtonDown(0)){
            Shoot();
        }


        // if(Input.GetKeyDown(KeyCode.Escape)){
        //     Cursor.lockState = CursorLockMode.None;
        // }
    
    }

    private void LateUpdate()
    {
        if(photonView.IsMine){
            FPScam.transform.position = viewPoint.position;
            FPScam.transform.rotation = viewPoint.rotation;
        }
        
    }

    void playerRotation(){
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
        
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        verticalRotStore += mouseInput.y;
       
        verticalRotStore = Mathf.Clamp(verticalRotStore, minLookAngle, maxLookAngle);
        
        viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
    }

    void playerMovement(){
        float yVel = movement.y;
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"),0 ,Input.GetAxisRaw("Vertical") );
        
        movement =  ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized;
        if(charCon.isGrounded){
            movement.y = 0;
        }else{
            movement.y = yVel;
        }
        movement.y += Physics.gravity.y * Time.deltaTime;
       // 
        charCon.Move( movement * moveSpeed * Time.deltaTime);
        
     }
      void playerMovementSprint(){
        float yVel = movement.y;
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"),0 ,Input.GetAxisRaw("Vertical") );
        
        movement =  ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized;
         if(charCon.isGrounded){
            movement.y = 0;
        }else{
            movement.y = yVel;
        }
        
        //gravity
       
        movement.y += Physics.gravity.y * Time.deltaTime;
        //
        charCon.Move( movement * moveSpeed * 2 * Time.deltaTime);
        
     }
     void playerJump(){
        
            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayer);
            
            if(Input.GetButtonDown("Jump") && isGrounded){
            movement.y = jumpForce;
            movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
            charCon.Move(movement * moveSpeed * Time.deltaTime);
            
        
            }

       
          
    }



    private void Shoot(){
        Ray ray = FPScam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = FPScam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit)){

            Debug.Log("hit " + hit.collider.gameObject.name);

        }
    }
}  

