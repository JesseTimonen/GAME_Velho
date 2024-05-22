using UnityEngine;

public class FireVolley : MonoBehaviour
{
    [SerializeField] private GameObject[] fragments;
    [SerializeField] private float spreadAngle;
     
    private void Start()
    {
        int bulletsToFire = fragments.Length;
        if (bulletsToFire == 0) return;

        Vector3 spawnPosition = GameManager.Instance.GetPlayerTransform().position;
        float angleStep = bulletsToFire > 1 ? spreadAngle / (bulletsToFire - 1) : 0;
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < bulletsToFire; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, currentAngle));
            Vector3 direction = rotation * (transform.position - spawnPosition).normalized;

            GameObject fireball = Instantiate(fragments[i], transform.position, Quaternion.identity);
            fireball.GetComponent<FireBall>().SetDirection(direction);
        }

        Invoke(nameof(DestroyGameObject), 3f);
    }


    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
