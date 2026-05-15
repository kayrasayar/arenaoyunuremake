using UnityEngine;

public class FinalSceneEntryAudio : MonoBehaviour
{
    [SerializeField] string clipAdi = "finalgiris";

    void Start()
    {
        if (GameProgressManager.Instance == null
            || GameProgressManager.Instance.currentDistrict != "Final")
        {
            return;
        }

        AudioClip clip = Resources.Load<AudioClip>(clipAdi);
        if (clip == null)
        {
            return;
        }

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.PlayOneShot(clip);
    }
}
