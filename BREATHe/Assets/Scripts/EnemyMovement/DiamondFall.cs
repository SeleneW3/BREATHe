using UnityEngine;

public class DiamondFall : MonoBehaviour
{
    public float fallSpeed = 8.0f;
    private float startY;
    private bool canFall = false;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        if (canFall)
        {
            Fall();
        }
    }

    private void Fall()
    {
        float newY = transform.position.y - fallSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // 提供一个公共方法让触发器调用
    public void StartFalling()
    {
        canFall = true;
    }
}
