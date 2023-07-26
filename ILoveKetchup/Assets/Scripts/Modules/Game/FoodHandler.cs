using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoodHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> foods;
    public GameObject activeFood { get; private set; }

    public float explosionRadius = 5f;
    public float jumpPower = 5f;
    public int numJumps = 1;
    public float explosionDuration = 1f;
    public LayerMask attachRaycastLayers;
    
    private List<Transform> foodComponents = new List<Transform>(); 
    private List<Transform> foodHaveColliderComponents = new List<Transform>(); 
    private List<Transform> foodNonColliderComponents = new List<Transform>(); 
    private void Awake()
    {
        ActiveRandomFood();
    }

    void ActiveRandomFood()
    {
        foods.ForEach(fd => fd.gameObject.SetActive(false));
        int random = Random.Range(0, foods.Count);
        foods[random].gameObject.SetActive(true);
        this.activeFood = foods[random];
        GetFoodComponents();
    }

    void GetFoodComponents()
    {
        foodComponents = activeFood.GetComponentsInChildren<Transform>().ToList();
        foodComponents.ForEach(food =>
        {
            if (food.GetComponent<MeshCollider>() != null)
            {
                foodHaveColliderComponents.Add(food);
            }
            else
            {
                foodNonColliderComponents.Add(food);
            }
        });
    }

    [ContextMenu("DoEffectExplode")]
    public void DoEffectExplode(Transform attachedBone)
    {
        List<Transform> willAttachToTargetObjs = new List<Transform>(this.foodNonColliderComponents.Count);
        foodNonColliderComponents.ForEach(food =>
        {
            bool randomValue = Random.value <= 0.6f;
            RaycastHit hit;
            if (randomValue && Physics.Raycast(food.position, Vector3.forward, out hit, Mathf.Infinity, attachRaycastLayers))
            {
                Debug.DrawRay(food.position, Vector3.forward * hit.distance, Color.yellow);
                willAttachToTargetObjs.Add(food);
                
                //anim
                food.DOKill();
                food.DOMove(hit.point + hit.normal * 0.1f, 0.1f);
                food.DORotate(Quaternion.LookRotation(hit.normal).eulerAngles + new Vector3(-90.0f, 0.0f, 0.0f), 0.1f)
                    .OnComplete(() =>
                    {
                        if (attachedBone != null)
                        {
                            food.SetParent(attachedBone);
                        }
                    });
            }
            else
            {
                Debug.DrawRay(transform.position, Vector3.forward * 1000, Color.white);
            }
        });
        
        Explode(this.foodComponents, willAttachToTargetObjs);
    }

    public void Explode(List<Transform> foodPieces, List<Transform> excepts)
    {
        foreach (Transform food in foodPieces)
        {
            if (excepts.Contains(food)) continue;
            Vector3 randomDirection = Random.insideUnitSphere * explosionRadius;
            Vector3 targetPosition = food.transform.position + randomDirection;

            food.transform.DOJump(targetPosition, jumpPower, numJumps, explosionDuration).SetEase(Ease.OutQuad);
            
            Vector3 randomRotation = new Vector3(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );

            food.transform.DORotate(randomRotation, explosionDuration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
            food.DOScale(0.0f, explosionDuration * 0.9f).SetEase(Ease.InQuart);

        }
    }
}
