using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEngine.EventSystems.EventTrigger;

public class GameMain : MonoBehaviour
{
    [Header("EnemySettings")]
    [SerializeField] public GameObject enemyContainer;
    [SerializeField] public Enemy[] enemy;
    [HideInInspector] public int enemy_count;
    [HideInInspector] public int[] enemy_health_list;

    [Header("PlayerSettings")]
    [SerializeField] public GameObject PlayerObject;
    [SerializeField] public Player player;
    [SerializeField] public PlayerControl playerControl;
    [SerializeField] public Camera mainCamera;
    [SerializeField] public GameObject gun;
    [HideInInspector] public Vector3 playerPosition;
    [HideInInspector] public Quaternion playerRotation;
    [HideInInspector] public Vector3 playerDirection;
    [HideInInspector] public int playerHealth;

    [Header("VFX")]
    [SerializeField] public GameObject effectPrefab;

    [Header("UI")]

    // Singleton
    public static GameMain main;

    // ## Game Manager
    [HideInInspector] public float gameTimer;
    private int timerValue;
    private bool isShoot;
    private bool isOnhit;
    [HideInInspector] public bool isStop;

    private bool isEffectOn;
    private float effetTimer;

    // ## Player
    [HideInInspector] public int playerMode;
    [HideInInspector] public int gunPower;
    [HideInInspector] public bool isSkillOn;
     

    // ## VFX
    private LineRenderer lineRenderer;
    private TrailRenderer trailRenderer;
    private Vector3 shootPoint;
    private Ray ray;
    private RaycastHit hit;
    private float layDistance;
    //private int layerMask;
    //[HideInInspector] public Vector3 userPosition;
    //[HideInInspector] public Quaternion userOrientation;



    private void Awake()
    {
        if (main == null)
        {
            main = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    private void Start()
    {
        //main = this;
        gameTimer = 100f;
        timerValue = 1;
        effetTimer = 0f;

        player = PlayerObject.GetComponent<Player>();

        enemy_count = enemyContainer.transform.childCount;
        enemy_health_list = new int[enemy_count];
        for (int i = 0; i < enemy_count; i++) { enemy[i] = GameObject.Find($"Enemy ({i})").GetComponent<Enemy>(); }

        layDistance = 500.0f;
        isShoot = false;
        isOnhit = false;
        isEffectOn = false;
        effetTimer = 0f;
        isStop = false;

        gunPower = 1;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        if (gameTimer >= 0f)
        {
            TimeManage();
            InputManage();
            PlayerManage();
            EnemyManage();
            EffectManage();
            UIManage();

            ResetState();
        }
    }

    private void TimeManage()
    {
        if (GameMain.main.playerMode == 3)
        {
            timerValue = 10;
        }
        gameTimer -= Time.deltaTime / timerValue;
    }


    private void InputManage()
    {
        isShoot = Input.GetMouseButtonDown(0);
        isStop = Input.GetKey(KeyCode.Escape);
    }

    private void PlayerManage()
    {
        playerPosition = new Vector3(player.playerPosition[0], player.playerPosition[1] - 1, player.playerPosition[2]);
        playerRotation = player.playerRotation;
        playerDirection = player.playerForwardDirection;
        playerHealth = player.playerCurrentHeath;

        playerMode = player.playermode;
        isSkillOn = playerControl.skillOn;


        if (GameMain.main.playerMode == 2)
        {
            gunPower = 3;
        }

        for(int i=0; i < enemy_count; i++)
        {
            if (enemy[i].attackDone==true)
            {
                if (player.playerPosition[1] - enemy[i].enemyPosition[1] <= 1.7f)
                {
                    player.playerCurrentHeath -= enemy[i].attackDamage;
                }
            }
            //Debug.Log(player.playerCurrentHeath);
        }
        
    }

    private void EnemyManage()
    {
        enemy_count = enemyContainer.transform.childCount;
    }

    private void EffectManage()
    {
        // Ray¸¦ ½ú´Ù¸é
        if (isShoot)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, layDistance))
            {
                isOnhit = true;
                Debug.DrawRay(ray.origin, ray.direction * layDistance, Color.red);

                for (int i = 0; i < 10; i++)
                {
                    string enemyName = $"Enemy ({i})";
                    if (hit.collider.gameObject.name == enemyName) { enemy[i].currnet_health -= gunPower; }
                }
            }
        }

        // Ray°¡ Ãæµ¹Ã¼¿¡ ´ê¾Ò´Ù¸é
        if (isOnhit)
        {
            effetTimer = 0f;
            shootPoint = gun.transform.position;

            GameObject effect = Instantiate(effectPrefab, hit.point, Quaternion.identity);
            Destroy(effect, 1f);
            isEffectOn = true;
        }

        // ÀÌÆåÆ® ON
        if(isEffectOn)
        {
            effetTimer += Time.deltaTime;
        }

        if (effetTimer <= 1f)
        {
            lineRenderer.SetPosition(0, shootPoint);
            lineRenderer.SetPosition(1, hit.point);
            //trailRenderer.AddPosition(shootPoint);
            //trailRenderer.AddPosition(hit.point);
        }

        else
        {
            isEffectOn = false;

            // ¼± ¾ø¾Ö´Â ¹ýÀ» ¸ô¶ó¼­ ¼û°Ü¹ö·È½À´Ï´Ù..
            lineRenderer.SetPosition(0, new(-100f, -100f, -100f));
            lineRenderer.SetPosition(1, new(-101f, -101f, -101f));
            //trailRenderer.AddPosition(new(-100f, -100f, -100f));
            //trailRenderer.AddPosition(new(-101f, -101f, -101f));
        }
    }

    private void UIManage()
    {
        for(int i = 0; i < enemy_count; i++)
        {
            enemy_health_list[i] = enemy[i].currnet_health;
        }
        //Debug.Log(enemy_health_list[0]);
    }


    private void ResetState()
    {
        isOnhit = false;
    }
}
