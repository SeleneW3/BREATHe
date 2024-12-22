using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float baseMoveSpeed = 10f; // ���˻����ƶ��ٶ�
    private float currentMoveSpeed;   // ��ǰ�ƶ��ٶ�

    void Start()
    {
        // ��ʼ�������ٶ�
        currentMoveSpeed = baseMoveSpeed;
    }

    private void Update()
    {
        MoveLeft();
    }

    // ���������ƶ�
    void MoveLeft()
    {
        transform.Translate(Vector2.left * currentMoveSpeed * Time.deltaTime);
    }

    // ӳ��ʱ�����ӵ������ƶ��ٶ�
    public void UpdateMoveSpeed(float timeScale)
    {
        // �� timeScale �� 0.1-5.0 ӳ�䵽�ƶ��ٶ� 2f�������� 10f���죩
        currentMoveSpeed = Mathf.Lerp(2f, baseMoveSpeed, (timeScale - 0.1f) / (5.0f - 0.1f));
        Debug.Log($"Enemy Move Speed Updated: {currentMoveSpeed} (Time Scale: {timeScale})");
    }

    // �������ҵĴ�����ײ
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("����������ң�");

            // ������ҹ���������������
            PlayerManager playerManager = other.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.TriggerDeath();
            }
        }
    }
}



