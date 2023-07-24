using System;
using System.Collections.Generic;
using System.Linq;
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

        public SolidData(Transform reference)
        {
            this.reference = reference;
            this.localPos = Vector3.zero;
        }
    };
    
    [SerializeField] private InputActions input;
    [SerializeField] private ObiSolver solver; // Reference to the ObiEmitter component.
    [SerializeField] private ObiEmitter emitter; // Reference to the ObiEmitter component.
    [SerializeField] private GameObject objKetchupBottle;
    
    #region lifecycles

    private void Start()
    {
        InitFluid();
    }
    
    private void OnEnable()
    {
        input.OnMousePositionUpdate += this.MousePositionUpdate;
        solver.OnBeginStep += Solver_OnBeginStep;
        emitter.OnEmitParticle += OnEmitParticle;
        // solver.OnCollision += Solver_OnCollision;
        // solver.OnParticleCollision += Solver_OnParticleCollision;
    }

    private void OnDisable()
    {
        input.OnMousePositionUpdate -= this.MousePositionUpdate;
        solver.OnBeginStep -= Solver_OnBeginStep;
        emitter.OnEmitParticle -= OnEmitParticle;

        // solver.OnCollision -= Solver_OnCollision;
        // solver.OnParticleCollision -= Solver_OnParticleCollision;
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
    }

    public void ChangeBottle()
    {
        
    }

    public void SetBottleVisible(bool isVisible)
    {
        this.objKetchupBottle.SetActive(isVisible);
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
    }

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
        Solidify(particleIndex, new SolidData(null));
    }

    #region collision (deprecated) performance issue
    void Solver_OnBeginStep(ObiSolver s, float stepTime)
    {
        for (int i = 0; i < solids.Length; ++i)
        {
            try
            {
                if (solver.invMasses[i] < 0.0001f)
                {
                    solver.positions[i] =
                        solver.transform.InverseTransformPoint(solids[i].reference.TransformPoint(solids[i].localPos));
                }
            }
            catch
            {
                //no need to handle this
            }
        }
    }
    
    void Solver_OnCollision(object sender, ObiSolver.ObiCollisionEventArgs e)
    {

        var colliderWorld = ObiColliderWorld.GetInstance();

        for (int i = 0; i < e.contacts.Count; ++i)
        {
            if (e.contacts.Data[i].distance < 0.001f)
            {
                var col = colliderWorld.colliderHandles[e.contacts.Data[i].bodyB].owner;
                Solidify(solver.simplices[e.contacts.Data[i].bodyA], new SolidData(col.transform));
            }
        }
    }
    
    void Solver_OnParticleCollision(object sender, ObiSolver.ObiCollisionEventArgs e)
    {
        for (int i = 0; i < e.contacts.Count; ++i)
        {
            if (e.contacts.Data[i].distance < 0.001f)
            {
                int particleIndexA = solver.simplices[e.contacts.Data[i].bodyA];
                int particleIndexB = solver.simplices[e.contacts.Data[i].bodyB];
        
                if (solver.invMasses[particleIndexA] < 0.0001f && solver.invMasses[particleIndexB] >= 0.0001f)
                    Solidify(particleIndexB, solids[particleIndexA]);
                if (solver.invMasses[particleIndexB] < 0.0001f && solver.invMasses[particleIndexA] >= 0.0001f)
                    Solidify(particleIndexA, solids[particleIndexB]);
            }
        }
    }
    #endregion
    
    void Solidify(int particleIndex, SolidData solid)
    {
        // remove the 'fluid' flag from the particle, turning it into a solid granule:
        solver.phases[particleIndex] &= (int)(~ObiUtils.ParticleFlags.Fluid);

        // fix the particle in place (by giving it infinite mass):
        solver.invMasses[particleIndex] = 0;

        // set the solid data for this particle:
        // try
        // {
        //     solid.localPos =
        //         solid.reference.InverseTransformPoint(solver.transform.TransformPoint(solver.positions[particleIndex]));
        //     solids[particleIndex] = solid;
        // }
        // catch
        // {
        //     //no need to handle this
        // }
    }
    
    #endregion
    
    #region debugs

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(this.ray);
    }

    #endregion  
}
