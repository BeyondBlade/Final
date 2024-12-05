using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float turnSpeed = 20f;
    public float normalSpeed = 5f;
    public float boostedSpeed = 10f;
    public float boostDuration = 2f;
    public float boostCooldown = 5f;
    public Image boostIndicator;
    public float freezeChance = 0.01f;
    public float freezeDuration = 1f;
    private bool isFrozen = false;
    private int requiredPressesToUnfreeze = 2;
    private int currentPressCount = 0;
    private float freezeTimer;
    Animator m_Animator;
    Rigidbody m_Rigidbody;
    AudioSource m_AudioSource;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;

    private float speed;
    private float boostTimer;
    private float cooldownTimer;
    private bool isBoosting;
    private bool canBoost = true;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AudioSource = GetComponent<AudioSource>();
        speed = normalSpeed;
        boostTimer = 0f;
        cooldownTimer = 0f;

        boostIndicator = GameObject.Find("BoostIndicator").GetComponent<Image>();

        if (boostIndicator != null)
        {
            boostIndicator.fillAmount = 1f;
        }
    }

    void FixedUpdate()
    {
        if (!isFrozen && Random.value < freezeChance)
        {
            FreezeCharacter();
        }

        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;

            if (freezeTimer <= 0f)
            {
                EndFreeze();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentPressCount++;

                if (currentPressCount >= requiredPressesToUnfreeze)
                {
                    EndFreeze();
                }
            }
        }

        if (!isFrozen)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            m_Movement.Set(horizontal, 0f, vertical);
            m_Movement.Normalize();

            bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
            bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
            bool isWalking = hasHorizontalInput || hasVerticalInput;
            m_Animator.SetBool("IsWalking", isWalking);

            if (isWalking)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.Play();
                }
            }
            else
            {
                m_AudioSource.Stop();
            }

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
            m_Rotation = Quaternion.LookRotation(desiredForward);

            HandleBoost();
        }
    }

    void HandleBoost()
    {
        if (Input.GetKey(KeyCode.Space) && canBoost)
        {
            StartBoost();
        }

        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                EndBoost();
            }
            if (boostIndicator != null)
            {
                boostIndicator.fillAmount = 1f;
                boostIndicator.color = new Color(boostIndicator.color.r, boostIndicator.color.g, boostIndicator.color.b, 0.5f);
            }
        }

        if (!canBoost)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canBoost = true;
                if (boostIndicator != null)
                {
                    boostIndicator.fillAmount = 1f;
                    boostIndicator.color = new Color(boostIndicator.color.r, boostIndicator.color.g, boostIndicator.color.b, 1f);

                }
            }
            else if (boostIndicator != null)
            {
                boostIndicator.fillAmount = cooldownTimer / boostCooldown;
                boostIndicator.color = new Color(boostIndicator.color.r, boostIndicator.color.g, boostIndicator.color.b, 0.5f);
            }
        }
    }

    void StartBoost()
    {
        isBoosting = true;
        canBoost = false;
        speed = boostedSpeed;
        boostTimer = boostDuration;
        cooldownTimer = boostCooldown;
    }

    void EndBoost()
    {
        isBoosting = false;
        speed = normalSpeed;
    }

    void FreezeCharacter()
    {
        isFrozen = true;
        freezeTimer = freezeDuration;
        currentPressCount = 0;
        speed = 0f;

        m_Animator.SetBool("IsFrozen", true);
    }

    void EndFreeze()
    {
        isFrozen = false;
        m_Animator.SetBool("IsFrozen", false);
        speed = normalSpeed;
    }

    void OnAnimatorMove()
    {
        Vector3 newPosition = m_Rigidbody.position + m_Movement * speed * Time.deltaTime;
        m_Rigidbody.MovePosition(newPosition);
        m_Rigidbody.MoveRotation(m_Rotation);
    }
}
