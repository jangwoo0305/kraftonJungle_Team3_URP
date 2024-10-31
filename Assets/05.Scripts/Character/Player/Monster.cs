using Cinemachine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : Player
{
    private GameObject hungerParticle;

    [SerializeField] private AudioSource monster_kill_sound;
    [SerializeField] private GameObject scientistObj;
    [SerializeField] private GameObject monsterObj;

    private PlayerMovement playerMovement;

    //private bool isAttacking = false;
    //private bool isDayShiftedWhileAttacking = false;

    [SerializeField] AnimationSync aniSync;
    [SerializeField] private CinemachineVirtualCamera cvc;
    [SerializeField] private int vc_original_priority = 5;
    [SerializeField] private int vc_lookat_priority = 20;

    private MouseComponent mc;
    private CinemachineBrain brain;

    private void Start()
    {
        playerMovement = GetComponentInChildren<PlayerMovement>();
        mc = GetComponentInChildren<MouseComponent>();
        // CinemachineBrain 컴포넌트 가져오기
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    public override void OnAttack(GameObject victim)
    {
        base.OnAttack(victim);

        OnTransformation(TimeManager.instance.GetisDay());

        playerMovement.isAttacking = true;
        mc.isAttacking = true;
        TransitionCamera(true);

        // Todo: hunger time reset
        //attacker �� hunger time reset ȣ��
        // Monster 에게만
        switch (victim.GetComponent<Player>().type)
        {
            case CharacterType.NPC:

                Debug.Log("Your gauge is Max");
                FindObjectOfType<HungerSlider>().SetHungerMax();
                NEHungerGauge.HungerEvent(false);

                break;
        }

        //MonsterKillSoundPlay();
        //GameManager.instance.OnKilled += MonsterKillSoundPlay();

    }

    public void TransitionCamera(bool isThird)
    {
        if (isThird)
        {
            cvc.Priority = vc_lookat_priority;
            playerMovement.SetLayerRecursive(monsterObj, 0);
            // Monster FPS 팔 끄기
            playerMovement.monsterFPS.SetActive(false);
        }
        else
        {
            cvc.Priority = vc_original_priority;
            //playerMovement.SetLayerRecursive(monsterObj, 3);
            // 코루틴 시작
            StartCoroutine(WaitForCinemachine());
        }
    }

    private IEnumerator WaitForCinemachine()
    {
        // 블렌드가 완료될 때까지 대기
        while (brain.IsBlending)
        {
            yield return null;
        }

        // 블렌드 완료 후 실행
        playerMovement.SetLayerRecursive(monsterObj, 3);
    }

    public override void OnDamaged(GameObject attacker)
    {
        base.OnDamaged(attacker);
        
        // 여기서 Destroy 결과 전송
        if (GetComponent<PhotonView>().AmOwner)
        {
            //PhotonNetwork.Destroy(this.gameObject);
            NEPlayerDeath.PlayerDeath();
            SpectatorManager.instance.StartSpectating();
        }
        else
        {
            SpectatorManager.instance.RemoveRemainingPlayer(this.gameObject);
        }
    }

    public override void OnDead()
    {
        base.OnDead();
    }

    public void OnNightVisibleScientist()
    {
        MonsterOutlineEffect moe = GetComponentInChildren<MonsterOutlineEffect>();
        moe.EnableOutlineEffect();
    }

    // 괴물의 밤시야 끄기
    public void OnDayUniteVisibilityScientist()
    {
        MonsterOutlineEffect moe = GetComponentInChildren<MonsterOutlineEffect>();
        moe.DisableOutlineEffect();
    }

    // 괴물 모습 변환
    public void OnTransformation(bool isAttackingInDay)
    {
        // 연구원 모습 비활성화
        scientistObj.SetActive(false);
        monsterObj.SetActive(true);
        // 애니메이터 변환
        if (playerMovement)
        {
            playerMovement.animator = monsterObj.GetComponent<Animator>();
            playerMovement.OnMonsterFPS(isAttackingInDay);
        }
        aniSync.ani = monsterObj.GetComponent<Animator>();
    }

    public void OffTransformation()
    {
        // 연구원 모습 활성화
        scientistObj.SetActive(true);
        monsterObj.SetActive(false);
        if (playerMovement)
        {
            // 애니메이터 변환
            playerMovement.animator = scientistObj.GetComponent<Animator>();
            playerMovement.OffMonsterFPS();
        }
        aniSync.ani = scientistObj.GetComponent<Animator>();
    }

    // Use this when Hunger Gauge reach 0
    public void OnHunger()
    {
        hungerParticle = GetComponentInChildren<ParticleSystem>(true).GameObject();
        hungerParticle.SetActive(true);
    }

    // Use this when Hunger Gauge reset
    public void NoHunger()
    {
        hungerParticle = GetComponentInChildren<ParticleSystem>(true).GameObject();
        hungerParticle.SetActive(false);
    }

    public override bool AttackDetection(GameObject target)
    {
        if (TimeManager.instance.GetisDay())
        {
            if (target.CompareTag("NPC")) return true;
        }
        else
        {
            if (target.CompareTag("Player")) return true;
        }
        return false;
    }

    public override void PlayKillSound()
    {
        base.PlayKillSound();
        monster_kill_sound.Play();
    }
}
