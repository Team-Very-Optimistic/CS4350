﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour_Spider : EnemyBehaviourBase
{
    enum SpiderBehaviourStates
    {
        Running,
        Windup,
        Jump,
        Explode //Not used
    }

    private SpiderBehaviourStates spiderState;

    public float windUpTime;

    //Jump
    private Vector3 originalPosition;
    private float jumpHeight;
    private float jumpTimeElapsed;
    [SerializeField] private AnimationCurve jumpCurve;
    private float lastKeyTime;

    //Explode
    public float explodeRadius = 4.0f;
    public float explodeDuration = 1.0f;
    public GameObject explodeParticlePrefab;

    //Post-explode lingering damage
    public GameObject poisonLingerParticlePrefab;
    public Color explodeDamageColor;
    private float explodeDamagePerTick = 1.0f;
    private float explodeDamageTotalDuration = 2.0f;
    private int explodeDamageNumTicks = 2;

    public GameObject redIndicatorPrefab;
    private Vector3 redIndicatorPosOffset;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        jumpHeight = player.GetComponent<Collider>().bounds.size.y;

        //Set running state
        spiderState = SpiderBehaviourStates.Running;

        lastKeyTime = jumpCurve[jumpCurve.length - 1].time;

        redIndicatorPosOffset = new Vector3(0.0f, GetComponent<Collider>().bounds.size.y, 0.0f);
    }

    // Update is called once per frame
    public override void Update()
    {
        if (navMeshAgent.enabled) // Not in knockback
        {
            switch (spiderState)
            {
                case SpiderBehaviourStates.Running:
                    {
                        if (!isInKnockback)
                        {
                            //Navigation
                            navMeshAgent.SetDestination(player.transform.position);
                        }

                        //If close enough to player, switch to wind up
                        if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
                        {
                            SetupJump();
                        }

                        break;
                    }
                case SpiderBehaviourStates.Windup:
                    {
                        transform.LookAt(new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z));

                        break;
                    }
                case SpiderBehaviourStates.Jump:
                    {
                        jumpTimeElapsed += Time.deltaTime;
                        float yOffset = jumpCurve.Evaluate(jumpTimeElapsed) * jumpHeight;

                        transform.position = originalPosition + new Vector3(0.0f, yOffset, 0.0f);

                        if (jumpTimeElapsed >= lastKeyTime)
                        {
                            Explode();
                        }
                        break;
                    }
                case SpiderBehaviourStates.Explode:
                    {
                        break;
                    }
            }
        }
        else
        {
            //Check in knockback state before stopping knockback state - Velocity update not neccesarily within same frame of enableknockback
            if (!isInKnockback)
            {
                if (rigidbody.velocity.magnitude > 0.0f)
                {
                    isInKnockback = true;
                }
            }
            else
            {
                if (rigidbody.velocity.magnitude <= knockbackVelStoppingThreshold)
                {
                    EnableKnockback(false);
                    isInKnockback = false;
                }
            }
        }
    }

    private void SetupJump()
    {
        //Invoke show red indicator function + start charge function
        Invoke(nameof(ShowRedIndicator), windUpTime * 0.5f);
        Invoke(nameof(StartJump), windUpTime);

        //Change enum state
        spiderState = SpiderBehaviourStates.Windup;

        //Trigger anim
        ResetAllAnimationTriggers();
        animator.SetTrigger("ToWindup");
    }

    private void ShowRedIndicator()
    {
        float timeToDestroy = windUpTime * 0.5f;

        GameObject redIndicator = GameObject.Instantiate(redIndicatorPrefab, transform.position + redIndicatorPosOffset * 2.0f, Quaternion.identity);
        Destroy(redIndicator, timeToDestroy);
    }

    private void StartJump()
    {
        //Change enum state
        spiderState = SpiderBehaviourStates.Jump;

        originalPosition = transform.position;

        GetComponent<Collider>().enabled = false;

        ResetAllAnimationTriggers();
        animator.SetTrigger("ToJump");
    }

    private void Explode()
    {
        var colliders = Physics.OverlapSphere(transform.position, explodeRadius);

        foreach (Collider hit in colliders)
        {
            if(hit.gameObject.CompareTag("Player"))
            {
                //Player parts are all tagged as "Player", check for PlayerHealth script
                PlayerHealth playerHealthScript = hit.gameObject.GetComponent<PlayerHealth>();

                if (playerHealthScript != null)
                {
                    GetComponent<Damage>().DealDamage(hit);
                    hit.gameObject.GetComponent<PlayerHealth>().StartCoroutine(
                        hit.gameObject.GetComponent<PlayerHealth>().ApplyDamageOverTime(
                            explodeDamagePerTick, explodeDamageNumTicks, explodeDamageTotalDuration, explodeDamageColor));

                    GameObject poisonLingerObject = GameObject.Instantiate(poisonLingerParticlePrefab, hit.transform.position, Quaternion.Euler(new Vector3(-90.0f, 0.0f, 0.0f)));
                    poisonLingerObject.GetComponent<FollowObject>().SetFollowObject(hit.gameObject);
                    ParticleSystem.MainModule poisonLingerParticleSystem = poisonLingerObject.GetComponent<ParticleSystem>().main;
                    poisonLingerParticleSystem.startLifetime = explodeDamageTotalDuration;
                    poisonLingerParticleSystem.duration = explodeDamageTotalDuration * 1.25f;
                    Destroy(poisonLingerObject, explodeDamageTotalDuration * 2.0f);

                    break; //Player has two colliders, just apply damage once 
                }
            }
        }

        GameObject poisonCloudParticle = GameObject.Instantiate(explodeParticlePrefab, transform.position, Quaternion.identity);
        ParticleSystem.MainModule poisonCloudParticleSystem = poisonCloudParticle.GetComponent<ParticleSystem>().main;

        poisonCloudParticleSystem.startSize = explodeRadius + 4;
        poisonCloudParticleSystem.startLifetime = explodeDuration;
        poisonCloudParticleSystem.duration = explodeDuration;
        poisonCloudParticle.GetComponent<ParticleSystem>().Play();
        Destroy(poisonCloudParticle, 1.0f);

        ScreenShakeManager.Instance.ScreenShake(0.25f, 0.4f);

        Destroy(gameObject);
    }


    private void ResetAllAnimationTriggers()
    {
        animator.ResetTrigger("ToWindup");
        animator.ResetTrigger("ToJump");
    }
}
