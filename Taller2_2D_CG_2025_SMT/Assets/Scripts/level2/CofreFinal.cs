using UnityEngine;

public class CofreFinal : MonoBehaviour
{
    public FinalResultados panelController;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().enabled = false;
            panelController.ShowPanel();
        }
    }
}
