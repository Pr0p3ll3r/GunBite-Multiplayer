using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector2 borders;
    [SerializeField] private float borderY;

    private Transform player;

    public void SetPlayer(Transform _player)
    {
        player = _player;
    }

    private void LateUpdate()
    {
        if(player != null)
            transform.position = player.position;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -borders.x, borders.x), Mathf.Clamp(transform.position.y, -borders.y, borderY), 0f);
    }
}
