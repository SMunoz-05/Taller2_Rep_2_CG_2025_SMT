using UnityEngine;

public class FlagTrigger : MonoBehaviour
{
    private Animator animator;
    private bool triggered = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;
        if (collision.CompareTag("Player"))
        {
            triggered = true;
            animator.SetTrigger("PlayFlag");
            StartCoroutine(LoadNextSceneAfterDelay(0.5f));
        }
    }
    private System.Collections.IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.CargarSiguienteNivelPorNombre();
    }
}
