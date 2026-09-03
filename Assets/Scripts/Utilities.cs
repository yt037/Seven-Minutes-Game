using UnityEngine;

public class Utilities : MonoBehaviour
{
    [SerializeField] private GameObject image;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.HasKeycard)
        {
            image.SetActive(true);
        }
    }

    public void Escape()
    {
        GameManager.Instance.EscapeEnding();
    }
}
