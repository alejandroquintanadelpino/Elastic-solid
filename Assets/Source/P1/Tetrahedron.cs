using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrahedron
{
    public Node node1;
    public Node node2;
    public Node node3;
    public Node node4;
    public Spring[] springs;
    public float volume;

    // Start is called before the first frame update
    public Tetrahedron(Node node1, Node node2, Node node3, Node node4, Spring[] springs)
    {
        this.node1 = node1;
        this.node2 = node2;
        this.node3 = node3;
        this.node4 = node4;
        this.springs = springs;
        this.volume = computeVolume();
    }

    public float computeVolume()
    {
        float volume;
        volume = Mathf.Abs(Vector3.Dot(node2.pos - node1.pos, Vector3.Cross(node3.pos - node1.pos, node4.pos - node1.pos))) / 6;
        return volume;
    }

    public bool contains(Vector3 v)
    {
        return same(node1, node2, node3, node4, v) && same(node2, node3, node4, node1, v) && same(node3, node4, node1, node2, v) && same(node4, node1, node2, node3, v);
    }

    private bool same(Node nodeA, Node nodeB, Node nodeC, Node nodeD, Vector3 v)
    {
        Vector3 n = Vector3.Cross(nodeB.pos - nodeA.pos, nodeC.pos - nodeA.pos);
        return (Vector3.Dot(nodeD.pos - nodeA.pos, n) * Vector3.Dot(v - nodeA.pos, n)) > 0;
    }
}
