using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1 : MonoBehaviour
{
    protected float posX, posY, rot;
    
    protected int m_hp, m_powerElapsed, currentHP, numShot;
    protected float moveSpeed = 4, rotationSpeed = 50, move, rotation;
    public GameObject bullet, bulletLv2;
    public Transform shootingPoint,shootingPointLv2;
    float xDirection, yDirection;
    // Start is called before the first frame update
    protected bool isLocal;
    
    // protected Player1 singleton;
    public HealthBar healthBar;
    public ManaBar manaBar;
    void Start()
    {
        m_powerElapsed= 0;
        m_hp = 50;
        currentHP = m_hp;  // = max hp
        posX = -7.5f;
        posY = -3.5f;
        rot = 0.0f;
        numShot = 0;

        healthBar.SetMaxHealth(m_hp);
        manaBar.SetMaxMana(10);
        Debug.Log($"Init p1 {isLocal}");
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
                

            if(Input.GetKeyDown(KeyCode.Space)){
                NetworkController.instance.sendShootMessage();
                
                if(this.m_powerElapsed > 0){
                   
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

    protected void LateUpdate() {
        transform.Translate(0, move, 0);
        transform.Rotate(0,0,rotation);
        if (move != 0 || rotation != 0) 
            NetworkController.instance.sendMoveData(transform.position.x, transform.position.y, transform.rotation.eulerAngles.z);
    }

    public void Move(){

    }

    public int getPowerElapsed() {
        return this.m_powerElapsed;
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
            numShot--;
        }
    }

    public void IncreaseHP() {
        NetworkController.instance.sendMessage("UPHP");
    }

    public void gotHit(int dame){
        NetworkController.instance.sendMessage("GHIT");
    }

    public void EatBulletItem(){
        NetworkController.instance.sendMessage("UPAT");

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
        this.m_powerElapsed = power;
      
        this.healthBar.SetHealth(this.m_hp);
        this.manaBar.SetMana(this.m_powerElapsed);
        // Debug.Log($"HP: {this.m_hp}");
    }
}
