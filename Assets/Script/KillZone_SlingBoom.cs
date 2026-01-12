using UnityEngine;

public class KillZone_SlingBoom : MonoBehaviour
{
    // Khi có vật thể đi vào vùng Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem vật va chạm có phải là GameUnit (Player hoặc Enemy) không
        GameUnit_SlingBoom unit = other.GetComponent<GameUnit_SlingBoom>();
        
        if (unit != null)
        {
            Debug.Log(unit.name + " đã rơi vào vùng chết!");
            unit.ForceDeath(); // Gọi hàm chết ngay lập tức
        }
        else
        {
            // Tùy chọn: Nếu đạn bay vào vùng chết thì cũng hủy đạn
            BulletController_SlingBoom bullet = other.GetComponent<BulletController_SlingBoom>();
            if (bullet != null)
            {
                Destroy(other.gameObject);
            }
        }
    }
}