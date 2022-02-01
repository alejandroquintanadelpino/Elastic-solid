using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fixer : MonoBehaviour {

    public GameObject entity;
    private Vector3 fixerInitPos;
    List<Node> fixedNodes = new List<Node>();
	// Possibilities of the Fixer
	void Start () 
    {
        fixerInitPos = this.transform.position;
        List<Node> nodes = entity.GetComponent<ElasticSolid>().getNodes();

        Bounds bounds = GetComponent<Collider>().bounds;

        foreach (Node node in nodes)
        {
            if (bounds.Contains(node.pos))
            {
                node.setFixed(true);
                fixedNodes.Add(node);
            }
        }
    }

    void Update()
    {
        Vector3 fixerPos = this.transform.position;

        if (fixerPos - fixerInitPos != Vector3.zero)
        {
            Vector3 move = fixerPos - fixerInitPos;
            foreach (Node node in fixedNodes)
            {
                node.pos += move;
            }
            this.fixerInitPos = fixerPos;
        }
    }

}