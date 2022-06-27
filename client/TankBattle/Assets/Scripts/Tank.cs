using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    private float moveSpeed = 4, rotationSpeed = 50, move, rotation;
    public GameObject bullet;
    public Transform shootingPoint, pos2, pos3;
    float xDirection, yDirection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // xDirection = Input.GetAxisRaw("Horizontal");
        // yDirection = Input.GetAxisRaw("Vertical");

        // float xmoveStep = moveSpeed*xDirection*Time.deltaTime;
        // float ymoveStep = moveSpeed*yDirection*Time.deltaTime;

        move = Input.GetAxis("Vertical") *moveSpeed * Time.deltaTime;
        rotation = Input.GetAxis("Horizontal") * -rotationSpeed *Time.deltaTime;

        // transform.position += new Vector3(xmoveStep, ymoveStep,0);

        if(Input.GetKeyDown(KeyCode.Space)){
            Shoot();
            // Instantiate(projectile, shootingPoint.position, shootingPoint.rotation);
        }

        // if(xDirection == 1){
        //     transform.Rotate(-Vector3.forward*180*Time.deltaTime);
        // }else if(xDirection == -1)
        //     transform.Rotate(Vector3.forward*180*Time.deltaTime);
    }

    private void LateUpdate() {
        transform.Translate(0, move, 0);
        transform.Rotate(0,0,rotation);
    }

    public void Shoot(){
        if(bullet && shootingPoint){
            Instantiate(bullet, shootingPoint.position, shootingPoint.rotation);
        }
    }

    public void Shoot3(){
        if(bullet && shootingPoint){
            Instantiate(bullet, shootingPoint.position, shootingPoint.rotation);
            // Instantiate(bullet, shootingPoint.position + new Vector3(1.0f,0.0f,0), shootingPoint.rotation);
            // Instantiate(bullet, shootingPoint.position + new Vector3(-1.0f,-0.0f,0), shootingPoint.rotation);
            Instantiate(bullet, pos2.position, pos2.rotation);
            Instantiate(bullet, pos3.position, pos3.rotation);
        }
    }
}
