using UnityEngine;

public class EnergyOrb : MonoBehaviour
{
    [SerializeField] private int scoreValue = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || !player.IsAlive)
        {
            return;
        }

        ScoreManager.Instance?.CollectOrb(scoreValue);
        GameAudio.PlayPickup();

        GameObject target = transform.parent != null ? transform.parent.gameObject : gameObject;
        Destroy(target);
    }
}
