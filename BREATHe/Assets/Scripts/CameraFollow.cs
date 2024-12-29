using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // 角色的 Transform
    public float offsetY = 0f;  // 相机与角色的 Y 轴偏移量
    public float offsetZ = -10f;  // 相机与角色的 Z 轴偏移量
    public float smoothSpeed = 0.125f;  // 水平方向的平滑速度
    public float verticalSmoothTime = 0.1f;  // 垂直方向的平滑时间

    private Vector3 velocity = Vector3.zero;  // 用于水平方向的 SmoothDamp
    private float currentVerticalVelocity;  // 用于垂直方向的 SmoothDamp

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 currentPos = transform.position;
            
            // 垂直方向平滑跟随
            float targetY = player.position.y + offsetY;
            float newY = Mathf.SmoothDamp(currentPos.y, targetY, ref currentVerticalVelocity, verticalSmoothTime);

            // 目标位置
            Vector3 desiredPosition = new Vector3(player.position.x, newY, offsetZ);
            
            // 使用平滑的方式移动相机
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        }
    }
}
