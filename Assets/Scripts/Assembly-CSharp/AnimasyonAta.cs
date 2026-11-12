using UnityEngine;

public class AnimasyonAta : MonoBehaviour
{
	public Animator animator;

	public RuntimeAnimatorController atanacakController;

	public void ControlleriAtaVeOynat()
	{
		if (animator != null && atanacakController != null)
		{
			animator.runtimeAnimatorController = atanacakController;
		}
	}
}
