using UnityEngine;
using UnityEngine.InputSystem;

public class LeverController : MonoBehaviour
{
    [Header("설정")]
    public float sensitivity = 17f; // 민감도
    public float minPos = -14f;      // 왼쪽 끝 제한
    public float maxPos = 14f;       // 오른쪽 끝 제한

    private float currentX = 0f;    // 누적된 레버 위치

    void Update()
    {
        // 1. 마우스의 좌우 움직임량(Delta X)을 읽어옵니다.
        // 나중에 하드웨어가 오면 이 부분만 하드웨어 입력값으로 바꾸면 됩니다!
        float deltaX = Mouse.current.delta.x.ReadValue();

        // 2. 현재 위치에 변화량을 더합니다.
        currentX += deltaX * sensitivity * Time.deltaTime;

        // 3. 레버가 범위를 벗어나지 않게 고정(Clamp)합니다.
        currentX = Mathf.Clamp(currentX, minPos, maxPos);

        // 4. 실제 오브젝트의 위치나 회전에 적용합니다.
        // 위치 이동의 경우:
        transform.position = new Vector3(currentX, transform.position.y, transform.position.z);

        // 만약 회전하는 레버라면:
        // transform.rotation = Quaternion.Euler(0, 0, currentX * 10f);
    }
}