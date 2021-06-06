using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drive : MonoBehaviour {

    float speed = 20.0F;              //velocidade do jogador
    float rotationSpeed = 120.0F;     //rotação
    public GameObject bulletPrefab;   //prefab da bullet
    float health = 10.0f;             //Vida inicial
    public Transform bulletSpawn;     //lugar onde a bullet sera spawnada

    public Slider healthBar;        //barra de HP

    
  


    private void Start()
    {
        
    }

    void Update() {
        float translation = Input.GetAxis("Vertical") * speed;             //movimentação pra frente
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;      //movimentação de rotação
       
        translation *= Time.deltaTime;                                     //movimentação multiplicada pelo delta time
        rotation *= Time.deltaTime;                                        //rotação multiplicada pelo delta time

        transform.Translate(0, 0, translation);                            //vetor modificado pela movimentação
        transform.Rotate(0, rotation, 0);                                  //vetor modificado pela rotação
        
        healthBar.value = (int)health;                                    //Liga a barra de HP do canvas com a vida do jogador
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);    //Barra de HP segue a cam
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);                 //posiciona a barra de HP

        if (Input.GetKeyDown("space"))          //se espaço for apertado
        {
            GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);  //spawna a bala no BulletSpawn
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*2000);                                                  //adiciona força na bala para que ela seja disparada
        }

        if(health == 0)                                               //se vida = 0
        {
            transform.position = new Vector3(239.5f, 0, -218f);       //player vai para a posição do parametro
            health = 100;                                             //enche a vida (vida = 100)
        }
        
    }

    
    void OnCollisionEnter(Collision col)                 //caso entre em colisão
    {
        if (col.gameObject.tag == "bullet")              //se colidir com algo com a tag de bullet, sofre dano
        {
            health -= 10;                               //dano de 10 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "out")
        {
            transform.position = new Vector3(239.5f, 0, -218f);       //player vai para a posição do parametro
            health = 100;                                             //enche a vida (vida = 100)
        }
    }



}
