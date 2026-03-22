using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Cheese : MonoBehaviour
{
    private bool playerInRange = false;
    public GameObject promptUI;
    public int score;
    public TextMeshProUGUI scoreText;
    public ObjectiveScores objectiveScores;
    void Start()
    {
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            score += objectiveScores.cheeseScore;
            scoreText.text = $"Score: {score.ToString()}";
            promptUI.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            playerInRange = true;
            promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            playerInRange = false;
            promptUI.SetActive(false);
        }
    }
}
