using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestBossTrigger : MonoBehaviour
{
    public GameObject MiniBoss;
    private GameObject MiniBossInstance;

    private ChestDrop DropsPrefab;
    private ChestDrop DropsInstance;

    public Animator ChestAnimator;
    public string Anim_TryOpen = "Trigger_TryOpen";
    public string Anim_DoesOpen = "Trigger_DoesOpen";

    private bool UpdateAreaTrigger = false;

    public void OnInteractionWithChest()
    {
        ChestAnimator.SetTrigger(Anim_TryOpen);
    }

    // This function is called from the Animation: Anim_TryOpen!
    public void SpawnMiniBoss()
    {
        MiniBossInstance = Instantiate(MiniBoss, this.transform.position + Vector3.forward * 2, Quaternion.identity);
        MiniBossInstance.GetComponent<MiniBoss>().InitializeMiniBoss(this);
    }

    public void OnMiniBossGuardDeath(ChestDrop Drops)
    {
        if (!Drops) { return; }

        DropsPrefab = Drops;

        if (ChestAnimator && ChestAnimator.enabled)
        {
            ChestAnimator.SetTrigger(Anim_DoesOpen);
        }
             
    }

    // This function is called from the Animation: Anim_DoesOpen!
    public void SpawnDropObject()
    {
        DropsInstance = Instantiate(DropsPrefab, this.transform);

        if (GameController.Instance)
        {
            if (DropsInstance.GetDroppedWeapon())
            {
                GameController.Instance.UnlockWeapon(DropsInstance.GetDroppedWeapon());
            }

            if (DropsInstance.GetDroppedElement())
            {
                GameController.Instance.UnlockElement(DropsInstance.GetDroppedElement());
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (!UpdateAreaTrigger && col.gameObject.tag == "AreaArenaTrigger")
        {
            UpdateAreaTrigger = true;
            col.GetComponent<AreaArenaTrigger>().SetMiniBossChest(this);
        }
    }
}


// Press button : Animation plays? -> boss spawns!

// If boss dies: Animation plays automatically -> unlock Item/Element!