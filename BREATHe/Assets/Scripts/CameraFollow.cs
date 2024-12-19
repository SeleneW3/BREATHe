using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // 角色的 Transform
    public float offsetY = 0f;  // 摄像机与角色在 Y 轴的偏移量（保持摄像机在水平位置中央）
    public float offsetZ = -10f;  // 摄像机与角色在 Z 轴的偏移量
    public float smoothSpeed = 0.125f;  // 摄像机平滑移动的速度

    private Vector3 velocity = Vector3.zero;  // 用于存储当前摄像机的移动速度

    private void LateUpdate()
    {
        if (player != null)
        {
            // 计算理想的摄像机位置
            Vector3 desiredPosition = new Vector3(player.position.x, offsetY, offsetZ);

            // 使用平滑的方式移动摄像机
            // 使用 SmoothDamp 而不是 Lerp，以提高平滑度
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        }
    }
}
