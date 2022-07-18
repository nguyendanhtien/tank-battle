using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    private int m_hp, m_attack;
    private float moveSpeed = 4, rotationSpeed = 50, move, rotation, powerEllapse;
    public GameObject bullet, bulletLv2;
    public Transform shootingPoint,shootingPointLv2, pos2, pos3;
    float xDirection, yDirection;
    // Start is called before the first frame update
    protected int id;
    void Start()
    {
        m_attack= 1;
        m_hp = 50;
        powerEllapse = 0;
        id = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // xDirection = Input.GetAxisRaw("Horizontal");
        // yDirection = Input.GetAxisRaw("Vertical");

        // float xmoveStep = moveSpeed*xDirection*Time.deltaTime;
        // float ymoveStep = moveSpeed*yDirection*Time.deltaTime;

        if(id == 1){
            move = Input.GetAxis("Vertical") *moveSpeed * Time.deltaTime;
            rotation = Input.GetAxis("Horizontal") * -rotationSpeed *Time.deltaTime;

            // transform.position += new Vector3(xmoveStep, ymoveStep,0);
            if(powerEllapse >0)
                powerEllapse -= Time.deltaTime;

            if(Input.GetKeyDown(KeyCode.Space)){
                if(powerEllapse >0){
                    ShootLv2();
                } else{
                    Shoot();
                }         
            }
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

    public void Move(){

    }

    public void Shoot(){
        if(bullet && shootingPoint){
            Instantiate(bullet, shootingPoint.position, shootingPoint.rotation);
        }
    }

    public void ShootLv2(){
        if(bulletLv2 && shootingPointLv2){
            Instantiate(bulletLv2, shootingPointLv2.position, shootingPointLv2.rotation);
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

    public void IncreaseHP(int num){
        m_hp += num;
    }

    public void gotHit(){
        
    }

    public void EatBulletItem(){
        powerEllapse += 10;
    }
}
