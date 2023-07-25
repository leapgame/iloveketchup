using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Input;
using Obi;
using UnityEngine;

public class KetchupHandler : MonoBehaviour
{
    [Serializable]
    public struct SolidData
    {
        public Transform reference;
        public Vector3 localPos;
        public int particleIndex;

        public SolidData(Transform reference)
        {
            this.reference = reference;
            this.localPos = Vector3.zero;
            this.particleIndex = 0;
        }
    };
    
    [SerializeField] private InputActions input;
    
    
    [Header("Solver for Cake")]
    [SerializeField] private ObiSolver solver; // using for cake.
    [SerializeField] private ObiEmitter emitter; // using for cake.
    [SerializeField] private GameObject objKetchupBottle;
    [SerializeField] private ObiParticleRenderer particleRenderer;

    [Header("Solver for ThrowTarget")]
    [SerializeField] private ObiSolver solverTarget; // using for the target will be thrown at face .
    [SerializeField] private ObiEmitter emitterTarget; // using for the target will be thrown at face.
    [SerializeField] private ObiParticleRenderer targetParticleRenderer;

    private float _particleSize = 1.5f;
    public float ParticleSize
    {
        get
        {
            return this._particleSize;
        }
        set
        {
            this._particleSize = value;
            this.particleRenderer.radiusScale = value;
        }
    }
    private Transform targetKetchup;
    #region lifecycles

    private void Start()
    {
        InitFluid();
    }
    
    private void OnEnable()
    {
        input.OnMousePositionUpdate += this.MousePositionUpdate;
        emitter.OnEmitParticle += OnEmitParticle;
        emitterTarget.OnEmitParticle += OnEmitParticleOnTarget;
    }

    private void OnDisable()
    {
        input.OnMousePositionUpdate -= this.MousePositionUpdate;
        emitter.OnEmitParticle -= OnEmitParticle;
        emitterTarget.OnEmitParticle -= OnEmitParticleOnTarget;
    }
    #endregion
    
    #region commons

    /// <summary>
    /// user after phase emit ketchup to cake
    /// </summary>
    /// <returns></returns>
    public List<Vector3> GetKetchupPositions()
    {
        return this.solids.ToList().ConvertAll(a => a.localPos).FindAll(a => a != Vector3.zero);
    }

    [ContextMenu("Kill All")]
    public void ClearAll()
    {
        this.emitter.KillAll();
        this.emitterTarget.KillAll();
    }

    public void ChangeBottle()
    {
        
    }

    public void SetBottleVisible(bool isVisible)
    {
        this.objKetchupBottle.SetActive(isVisible);
    }

    public void SetTargetKetchup(Transform trans)
    {
        this.targetKetchup = trans;
    }

    public void SetTargetThrowing(Transform trans)
    {
        if (trans == null)
        {
            Development.Log("SetTargetThrowing ERROR target is null");
            return;
        }
        
        this.solverTarget.transform.SetParent(trans);
        this.SetParticleVisible(true, false);
    }

    #endregion
    
    #region inputs
    private Ray ray;
    private void MousePositionUpdate(Vector2 position)
    {
        if (GameController.instance.state != GameController.State.KETCHUP) return;
        ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            // Development.Log("OnHit: " + hitInfo.collider.name);
            SpawnFluidAt(hitInfo.point);
        }
    }
    
    #endregion
    
    #region fluid particles
    private SolidData[] solids = new SolidData[0];
    private float minDistancePerFluid = 0.085f;
    
    private void InitFluid()
    {
        // resize array to store one reference transform per particle:
        Array.Resize(ref solids, solver.allocParticleCount);
        ParticleSize = 1.5f;
        SetParticleVisible(false, true);
    }
    
    #region cake fluid
    
    private Vector3 oldSpawnPos = Vector3.up * 1000.0f;
    private void SpawnFluidAt(Vector3 rayPos)
    {
        emitter.transform.position = rayPos + Vector3.up * 0.05f;
        if (Vector3.Distance(rayPos, oldSpawnPos) < minDistancePerFluid) return;
        this.oldSpawnPos = rayPos;
        emitter.EmitParticle(0.0f, 0.0f);
    }

    void OnEmitParticle(ObiEmitter emitter, int particleIndex)
    {   
        // remove the 'fluid' flag from the particle, turning it into a solid granule:
        solver.phases[particleIndex] &= (int)(~ObiUtils.ParticleFlags.Fluid);
        // fix the particle in place (by giving it infinite mass):
        solver.invMasses[particleIndex] = 0;
        
        SolidData addedData = AddToSolidData(particleIndex);
        TryEmitToTargetAlso(addedData.localPos);
    }
    
    SolidData AddToSolidData(int particleIndex)
    {
        SolidData sd = new SolidData(null);
        sd.particleIndex = particleIndex;
        sd.localPos =
            targetKetchup.InverseTransformPoint(solver.transform.TransformPoint(solver.positions[particleIndex]));
        solids[particleIndex] = sd;
        return sd;
    }


    #endregion
    
    //have to fluid at target as well, cause if we spawn at the time cake hit target its will cause laggy
    #region target fluid

    private Tween tweenParticle = null;
    public void SetParticleVisible(bool isVisible, bool isNow = false)
    {
        float finalValue = isVisible ? ParticleSize : 0.0f;
        if (isNow)
        {
            this.targetParticleRenderer.radiusScale = finalValue;
            return;
        }
        tweenParticle?.Kill();
        tweenParticle = DOVirtual.Float(this.targetParticleRenderer.radiusScale, finalValue, 0.35f, f =>
        {
            this.targetParticleRenderer.radiusScale = f;
        }).Play();
    }
    
    void OnEmitParticleOnTarget(ObiEmitter emitter, int particleIndex)
    {
        // remove the 'fluid' flag from the particle, turning it into a solid granule:
        solverTarget.phases[particleIndex] &= (int)(~ObiUtils.ParticleFlags.Fluid);
        // fix the particle in place (by giving it infinite mass):
        solverTarget.invMasses[particleIndex] = 0;
    }
    private List<Vector3> targetPositions = new List<Vector3>();
    void TryEmitToTargetAlso(Vector3 cakeLocalPos)
    {
        Vector3 raycastFromPoint = TargetHandler.Instance.transKetchupRaycastAnchor.TransformPoint(cakeLocalPos);
        targetPositions.Add(raycastFromPoint);
        
        //try ray cast if hit the target
        Ray rayToTarget = new Ray(raycastFromPoint, Vector3.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(rayToTarget, hitInfo: out hitInfo))
        {
            Vector3 spawnFluidPoint = hitInfo.point + hitInfo.normal * 0.05f;
            emitterTarget.transform.position = spawnFluidPoint;
            emitterTarget.EmitParticle(0.0f, 0.0f);
        }
    }
    #endregion
    
    #endregion
    
    #region debugs

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(this.ray);
        
        Gizmos.color = Color.red;
        foreach (var targetPosition in this.targetPositions)
        {
            Gizmos.DrawSphere(targetPosition, 0.1f);
        }
    }

    #endregion  
}
