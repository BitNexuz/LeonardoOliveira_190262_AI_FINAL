using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    public Transform player;        //tranform do player
    public Transform bulletSpawn;   //lugar onde a bala spawna
    public Slider healthBar;        //barra de HP
    public GameObject bulletPrefab; //prefab bullet

    NavMeshAgent agent;              //navmesh do agent
    public Vector3 destination;      //o destino para onde se mover
    public Vector3 target;           //aonde mirar
    float health = 100.0f;           //HP inicial
    float rotSpeed = 5.0f;           //velocidade de rotação
    float speed1 = 10000f;           //variação de velocidade para fuga
    float speed2 = 20f;              //variação para depois da fuga
    
    
   
    






    float visibleRange = 80.0f;      //limite de visão
    float shotRange = 40.0f;         //alcance da bala

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();                                //liga o agent ao navmesh
        
        agent.stoppingDistance = shotRange - 5;                                   
        InvokeRepeating("UpdateHealth",5,0.5f);                                 
    }

    void Update()
    {
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);    //barra de hp segue a cam
        healthBar.value = (int)health;                                                     //Liga a barra de HP do canvas com a vida do jogador
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);                 //posiciona a barra de hp
        
    }

    void UpdateHealth()
    {
       if(health < 100)              //caso a vida seja menor que 100, a vida é recuperada
        health ++;                   //soma pontos a vida
    }

    void OnCollisionEnter(Collision col) //caso entre em colisão
    {
        if(col.gameObject.tag == "bullet")              //se colidir com algo com a tag de bullet, sofre dano
        {
            health -= 10;                               //cada bala causa 10 de dano
        }
    }

    [Task]                                    
    public void PickRandomDestination()                                                       //seleciona um destino aleatorio
    {
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));      //ponto aleatorio
        agent.SetDestination(dest);                                                          
        agent.speed = speed2;                                                                 //retorna a velocidade para 20
        Task.current.Succeed();                                                               //tarefa concluida
    }

    [Task]                                                                            
    public void MoveToDestination()                                                   //função que move o player até o destino
    {
        if(Task.isInspected)                                                          //caso a tarefa tenha sido inspecionada
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);          //tempo que a tarefa esta em execução
        if(agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)   //caso a distancia percorrida pelo agent for menor que a distancia para que ele pare, a tarefa é concluida
        {
            Task.current.Succeed();                                                   //tarefa concluida
        }

    }
    
    [Task]                                                                            
    public void PickDestination(int x, int z)                                         
    {
        Vector3 dest = new Vector3(x, 0, z);                                          //novo destino colocado na bt
        agent.SetDestination(dest);                                                   //torna dest o destino do agente

        Task.current.Succeed();                                                       //tarefa concluida
    }
   
    [Task]                                                                            
    public void TargetPlayer()                                                        //faz do jogador o alvo
    {
        target = player.transform.position;                                           
        Task.current.Succeed();                                                       //tarefa concluida
    } 
    
   

    [Task]
    bool Turn(float angle)                                                                                      
    {
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;     
        target = p;                                                                                             //p é o target
        return true;                                                                                            //retorna true
    }


    [Task]                                                                                                                                      
    public void LookAtTarget()                                                                                                                  //detecta o target
    {
        Vector3 direction = target - this.transform.position;                                                                                   //torna o target a direção do vetor
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);     //rotação que o ai usa para virar pro jogador
       
        if (Task.isInspected)                                                                                                                   
            Task.current.debugInfo = string.Format("anfle={0}", Vector3.Angle(this.transform.forward, direction));                              //ativa a tarefa
       
        if(Vector3.Angle(this.transform.forward, direction) < 5.0f)                                                                             //caso o angulo seja menor que 5.
        {
            Task.current.Succeed();                                                                                                             //tarefa concluida
        }
    }

    [Task]                                                                            
    public bool Fire()                                                                //metodo de disparo
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);     //Instancia bullet       
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);                                                   //aplica força para disparar a bala para frente

        return true;                                                                                                                  //se disparou, retorna verdadeiro
    }

    [Task]                                                                       
    bool SeePlayer()                                                               //vê o jogador
    {
        Vector3 distance = player.transform.position - this.transform.position;    //distancia do até o player
        RaycastHit hit;                                                            //gera o raycast
        bool seeWall = false;
        Debug.DrawRay(this.transform.position, distance, Color.red);               //escolhe vermelho como a cor do raycast
       
        if (Physics.Raycast(this.transform.position, distance, out hit))           
        {
            if(hit.collider.gameObject.tag == "wall")                              //caso o raycast colida com algo de tag wall
            {
                seeWall = true;                                                    //retorna true para o seeWall
            }
        }
       
        if (Task.isInspected)                                                      
            Task.current.debugInfo = string.Format("wall={0}", seeWall);           

        if (distance.magnitude < visibleRange && !seeWall)                         //caso o seewall seja falso e esteja com visão
            return true;                                                           //retonar true
        else                                                                       
            return false;                                                          //retorna false

    }

    [Task]                                                 
    public bool IsHealthLessThan(float health)             //usado para casos em que a vida seja menor que um valor
    {
        return this.health < health;                      
    }

    [Task]                               
    public bool Explode()                  //destrói o oponente
    {
        Destroy(healthBar.gameObject);     //destrói a barra de HP
        Destroy(this.gameObject);          //destroI o gameobject 
        return true;                       //retorna true
    }

    [Task]
    public void LookAtTargetFuga()                                                                                                              
    {
        Vector3 direction = target - this.transform.position;                                                                                   //torna o target a direção do vetor
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);     //rotação que o ai usa para virar pro jogador



        if (Task.isInspected)                                                                                                                   
        {
            Task.current.debugInfo = string.Format("anfle={0}", Vector3.Angle(this.transform.forward, direction));                              
            agent.speed = speed1;                                                                                                               //altera a velocidade
        }

        if (Vector3.Angle(this.transform.forward, direction) < 5.0f)                                                                             //caso o angulo seja menor que 5
        {
            Task.current.Succeed();                                                                                                   //tarefa concluida        
        }
    }

    [Task]

    public void EmpurrarPraFora()
    {

        if (Vector3.Angle(this.transform.forward, destination) < 5.0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed1 * Time.deltaTime);                      //corre até o player e o empurra
        }
           
    }

    [Task]

    public void Morre()
    {    
        Destroy(gameObject);                       //Destroi o robo
    }

    [Task]
    public void Congela()
    {
        Vector3 direction = player.position;                                                                           //torna o target a direção do vetor
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);      //rotação que o ai usa para virar pro jogador


        if (Task.isInspected)                                                                                                                   
        {
            Task.current.debugInfo = string.Format("anfle={0}", Vector3.Angle(this.transform.forward, direction));                              
                                                                                                                         
        }

        if (Vector3.Angle(this.transform.forward, direction) < 5.0f)                                                                             //caso o angulo seja menor que 5
        {
            Task.current.Succeed();                                                                                                   //tarefa concluida       
        }
    }
}



  


