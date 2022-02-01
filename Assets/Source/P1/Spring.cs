using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring {

    public Node nodeA, nodeB;

    public float Length0;
    public float Length;
    private float damp;

    public float stiffness;
    public float volume;

    public Spring(Node nodeA, Node nodeB, float stiffness, float damping)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        this.stiffness = stiffness;
        this.damp = damping;
        this.volume = 0;

        UpdateLength();
        Length0 = Length;
    }

    /**
    // Use this for initialization
    void Start () {
        UpdateLength();
        Length0 = Length;
    }
	
	// Update is called once per frame
	void Update () {
        transform.localScale = new Vector3(transform.localScale.x, Length / 2.0f, transform.localScale.z);
        transform.position = 0.5f * (nodeA.pos + nodeB.pos);

        Vector3 u = nodeA.pos - nodeB.pos;
        u.Normalize();
        transform.rotation = Quaternion.FromToRotation(Vector3.up, u);
    }
    /**/

    public void UpdateLength ()
    {
        Length = (nodeA.pos - nodeB.pos).magnitude;
    }

    public void ComputeForces()
    {
        Vector3 u = nodeA.pos - nodeB.pos;
        u.Normalize();
        Vector3 force = - volume / (float)Math.Pow(Length0, 2) * stiffness * (Length - Length0) * u;
        force += -damp/10 * (nodeA.vel - nodeB.vel);
        force += -damp * Vector3.Project((nodeA.vel - nodeB.vel), u);
        nodeA.force += force;
        nodeB.force -= force;
    }

    public bool Equals(Spring s)
    {
        if ((this.nodeA == s.nodeA) && (this.nodeB == s.nodeB))
            return true;
        else
            return false;
    }
}
