using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : Player
{
    private GameObject hungerParticle;
    public override void OnAttack(GameObject victim)
    {
        base.OnAttack(victim);
    }

    public override void OnDamaged(GameObject attacker)
    {
        base.OnDamaged(attacker);

        // 여기서 Destroy 결과 전송
        //Destroy(this.gameObject);
        if (GetComponent<PhotonView>().AmOwner)
        {
            Camera.main.GetComponent<SpectatorMode>().StartSpectating();
            PhotonNetwork.Destroy(this.gameObject);
            NEPlayerDeath.PlayerDeath();
        }
        else
        {
            Camera.main.GetComponent<SpectatorMode>().RemovePlayer(this.gameObject);
        }
        // GameObject.FindObjectOfType<SpectatorMode>() // 이건 효율이 안 좋음
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

    public void OnDayUniteVisibilityScientist()
    {
        MonsterOutlineEffect moe = GetComponentInChildren<MonsterOutlineEffect>();
        moe.DisableOutlineEffect();
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
        if (GameManager.instance.GetTime())
        {
            if (target.CompareTag("NPC")) return true;
        }
        else
        {
            if (target.CompareTag("Player")) return true;
        }
        return false;
    }
}
