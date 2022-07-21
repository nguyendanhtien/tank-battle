using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1 : MonoBehaviour
{
    protected float posX, posY, rot;
    
    protected int m_hp, m_attack, currentHP, numShot;
    protected float moveSpeed = 4, rotationSpeed = 50, move, rotation, powerEllapse;
    public GameObject bullet, bulletLv2;
    public Transform shootingPoint,shootingPointLv2, pos2, pos3;
    float xDirection, yDirection;
    // Start is called before the first frame update
    protected bool isLocal;
    

    public HealthBar healthBar;
    public ManaBar manaBar;
    void Start()
    {
        m_attack= 1;
        m_hp = 50;
        currentHP = m_hp;  // = max hp
        powerEllapse = 0;
        posX = -7.5f;
        posY = -3.5f;
        rot = 0.0f;
        numShot = 0;
    
        healthBar.SetMaxHealth(m_hp);
        manaBar.SetMaxMana(10);
        // isLocal = false;
    }
    public void setLocal(bool isLocalArg) {
        isLocal = isLocalArg;
    }

    public bool getLocal() {
        return isLocal;
        
    }

    public int getNumShooting() {
        return numShot;
    }

    
    // Update is called once per frame
    protected void Update()
    {
        // xDirection = Input.GetAxisRaw("Horizontal");
        // yDirection = Input.GetAxisRaw("Vertical");

        // float xmoveStep = moveSpeed*xDirection*Time.deltaTime;
        // float ymoveStep = moveSpeed*yDirection*Time.deltaTime;
        
        if(isLocal){
            move = Input.GetAxis("Vertical") *moveSpeed * Time.deltaTime;
            rotation = Input.GetAxis("Horizontal") * -rotationSpeed *Time.deltaTime;
            
            
            // transform.position += new Vector3(xmoveStep, ymoveStep,0);
            if(powerEllapse >0)
                powerEllapse -= Time.deltaTime;
                manaBar.SetMana(powerEllapse);

            if(Input.GetKeyDown(KeyCode.Space)){
                NetworkController.instance.sendShootMessage();
                if(powerEllapse >0){
                    ShootLv2();
                } else{
                    Shoot();
                }         
            }
        }

        if(Input.GetKeyDown(KeyCode.Space)){
            gotHit(5);
        }

        
        // if(xDirection == 1){
        //     transform.Rotate(-Vector3.forward*180*Time.deltaTime);
        // }else if(xDirection == -1)
        //     transform.Rotate(Vector3.forward*180*Time.deltaTime);
    }

    protected void LateUpdate() {
        transform.Translate(0, move, 0);
        transform.Rotate(0,0,rotation);
        if (move != 0 || rotation != 0) 
            NetworkController.instance.sendMoveData(transform.position.x, transform.position.y, transform.rotation.eulerAngles.z);
    }

    public void Move(){

    }

    public void Shoot(){
        if(bullet && shootingPoint){
            Instantiate(bullet, shootingPoint.position, shootingPoint.rotation);
            numShot--;
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

    public void gotHit(int dame){
        currentHP -= dame;
        healthBar.SetHealth(currentHP);
    }

    public void EatBulletItem(){
        powerEllapse += 10;

    }

    public void moveNewPos(){
        // Debug.Log($"{this.posX} {this.posY}");
        transform.position = new Vector3(this.posX, this.posY, 0);
        // transform.position.x += posX;
        // transform.position.y += posY;
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, this.rot);
        // transform.rotation.z += rotation;
    }

    public void setState(float posX, float posY, float rotation, int hp, int isShot, int power) {
        this.posX = posX;
        this.posY = posY;
        this.rot = rotation;
        this.numShot += isShot;
        this.m_hp = hp;
        this.m_attack = power;
    }
}
