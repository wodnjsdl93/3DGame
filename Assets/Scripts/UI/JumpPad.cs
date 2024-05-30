using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 10f;  // 점프 강도 설정

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected"); // 충돌 감지 메시지
        Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("Applying jump force"); // 힘 적용 메시지
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}

