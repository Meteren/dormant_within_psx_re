using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System;

public class PathGrid : MonoBehaviour, IComparable<PathGrid>
{
    public int X {  get; private set; }
    public int Y { get; private set; }

    public float GScore {  get; set; }
    public float FScore { get;set; }

    Transform agentTransform;

    public bool isMovable;

    List<Renderer> objectsOccupyingGrid = new List<Renderer>();

    Vector3 offset;

    Vector3 originalPos;


    [Header("LayerMask")]
    [SerializeField] private LayerMask layerMask; 


    [Header("Grids")]
    [SerializeField] private Transform grids;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Obstacle");
    }

    /*private void Update()
    {

        //transform.rotation = Quaternion.identity;
        //transform.position = agentTransform.position + offset;
        //transform.position = originalPos;



        Vector3 size = GetComponent<BoxCollider>().size * 0.5f;
        Vector3 boxCenter = transform.position;
        Collider[] collidedObjects = Physics.OverlapBox(boxCenter, size, Quaternion.identity, layerMask);

        Debug.Log("Collided object count:" + collidedObjects.Length);

        isMovable = collidedObjects.Length != 0 ? false : true;
       // Debug.Log($"{X}-{Y} isMovable: {isMovable}");

    }*/

    private void FixedUpdate()
    {
        isMovable = objectsOccupyingGrid.Count > 0 ? false : true;
    }

    public void InitPathGrid(Transform agentTransform,BoxCollider boxCollider,Vector3 position, int x, int y)
    {
        this.X = x;
        this.Y = y;
        this.agentTransform = agentTransform;
        transform.position = position;
        originalPos = position;
        offset = transform.position - agentTransform.position;
        gameObject.layer = LayerMask.NameToLayer("PathGrid");
        BoxCollider collider = transform.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        Vector3 collWorldSize = boxCollider.size;
        collWorldSize.Scale(agentTransform.lossyScale);
        collider.size = collWorldSize;
        //transform.SetParent(agentTransform);
        grids = GameObject.Find("Grids").transform;
        transform.SetParent(grids);
    }

    private void OnTriggerEnter(Collider other)
    {
      
        if (other.TryGetComponent<Renderer>(out Renderer renderer))
            if(renderer != GetComponent<Renderer>() && renderer.GetComponent<Item>() == null)
                objectsOccupyingGrid.Add(renderer);
          
    }
    private void OnTriggerExit(Collider other)
    {
       
        if (other.TryGetComponent<Renderer>(out Renderer renderer))
            if (renderer != GetComponent<Renderer>() && renderer.GetComponent<Item>() == null)
                objectsOccupyingGrid.Remove(renderer);
    }

    public float CalculateDistance(Vector3 positionToMove) => Vector3.Distance(transform.position, positionToMove);

    public int CompareTo(PathGrid other)
    {
        return FScore.CompareTo(other.FScore);
    }
}
