using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleBreath : MonoBehaviour
{
    public float maxAttractionStrength = 10f;  // ��ǿ������
    public float maxAttractionDistance = 5f;  // ���������Χ
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

            // ����������ǿ�ȣ�����Խ����������Խǿ
            float attractionStrength = Mathf.Lerp(0, maxAttractionStrength, 1 - (distance / maxAttractionDistance));

            // ��ֹ��������Ϊ��ֵ
            attractionStrength = Mathf.Max(attractionStrength, 0);

            // ����������ǿ�Ƚ��������ڶ�
            player.position = Vector3.MoveTowards(player.position, transform.position, attractionStrength * Time.deltaTime);
        }
    }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                player = other.transform;
                isPlayerInRange = true;
                Debug.Log("��ҽ���������Χ");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                player = null;
                isPlayerInRange = false;
                Debug.Log("����뿪������Χ");
            }
        }
    }
