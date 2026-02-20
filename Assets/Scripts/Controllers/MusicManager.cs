using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    void Start()
    {
        audioSource.PlayDelayed(2f);
    }
}
