using UnityEngine;

public class PlayerAnimator : Movement
{


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.CrossFade("Twerk", 0.3f);
        }
    }
}
