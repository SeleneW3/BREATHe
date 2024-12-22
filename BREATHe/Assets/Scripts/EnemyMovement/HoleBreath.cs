using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleBreath : MonoBehaviour
{
    public float maxAttractionStrength = 10f;  // 最强吸引力
    public float maxAttractionDistance = 5f;  // 最大吸引范围
    private Transform player;
    private bool isPlayerInRange = false;

    public ParticleSystem attractionParticles;
    void Start()
    {
        attractionParticles.Play();
    }

    private void Update()
    {
        if (isPlayerInRange && player != null)
        {
            Vector3 direction = (transform.position - player.position).normalized;

            float distance = Vector3.Distance(player.position, transform.position);

            // 计算吸引力强度，距离越近，吸引力越强
            float attractionStrength = Mathf.Lerp(0, maxAttractionStrength, 1 - (distance / maxAttractionDistance));

            // 防止吸引力变为负值
            attractionStrength = Mathf.Max(attractionStrength, 0);

            // 根据吸引力强度将玩家拉向黑洞
            player.position = Vector3.MoveTowards(player.position, transform.position, attractionStrength * Time.deltaTime);
        }
    }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                player = other.transform;
                isPlayerInRange = true;
                Debug.Log("玩家进入吸引范围");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                player = null;
                isPlayerInRange = false;
                Debug.Log("玩家离开吸引范围");
            }
        }
    }
