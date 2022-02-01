using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sample code for accessing MeshFilter data.
/// </summary>
public class ElasticSolid : MonoBehaviour
{
    /// <summary>
    /// Default constructor. Zero all. 
    /// </summary>
    public ElasticSolid()
    {
        this.Paused = true;
        this.TimeStep = 0.01f;
        this.Gravity = new Vector3(0.0f, -9.81f, 0.0f);
        this.IntegrationMethod = Integration.Symplectic;
        this.massDensity = 1.0f;
        this.stiffness = 50.0f;
        this.flexStiffness = 20.0f;

        this.damping = 0.0f;
        this.nodes = new List<Node> { };
        this.springs = new List<Spring> { };
        this.vertices = new List<Vector3> { };
        this.tetrahedrons = new List<Tetrahedron> { };
        this.tetrahedronsList = new List<int> { };
    }

   
    public enum Integration
    {
        Explicit = 0,
        Symplectic = 1,
    };

    #region InEditorVariables

    public bool Paused;
    public float TimeStep;
    public float massDensity;
    public float stiffness;
    public float flexStiffness;
    public float damping;
    public Vector3 Gravity;
    public Integration IntegrationMethod;

    private Mesh mesh;
    private Vector3[] modelVerticesHP;
    private Vector4[] baricentricCoordinates;
    private List<Vector3> vertices;
    private List<Tetrahedron> tetrahedrons;
    private List<int> tetrahedronsList;
    private int[] tetrahedronsContainsVertex;

    private List<Node> nodes;
    private List<Spring> springs;
    #endregion

    #region OtherVariables

    #endregion

    #region MonoBehaviour

    public void Awake()
    {
        this.mesh = this.GetComponentInChildren<MeshFilter>().mesh;
        this.modelVerticesHP = mesh.vertices;
        this.baricentricCoordinates = new Vector4[modelVerticesHP.Length];
        this.tetrahedronsContainsVertex = new int[modelVerticesHP.Length];

        GetComponent<Parser>().ParseFiles();
        this.vertices = GetComponent<Parser>().getVertices();
        this.tetrahedronsList = GetComponent<Parser>().getTetrahedrons();

        createNodes(this.vertices);
        createTetrahedrons(tetrahedronsList);
        createBaricentricCoordinates(this.modelVerticesHP, this.tetrahedrons);
        //createSprings();

        foreach (Tetrahedron tet in tetrahedrons)
        {
            tet.node1.mass += tet.volume * massDensity / 4;
            tet.node2.mass += tet.volume * massDensity / 4;
            tet.node3.mass += tet.volume * massDensity / 4;
            tet.node4.mass += tet.volume * massDensity / 4;

            foreach (Spring spring in tet.springs)
            {
                spring.volume += tet.volume / 6;
            }
        }
    }

    public void Update()
    {
        
    }

    public void FixedUpdate()
    {
        if (this.Paused)
            return; // Not simulating

        // Select integration method
        switch (this.IntegrationMethod)
        {
            case Integration.Explicit: this.stepExplicit(); break;
            case Integration.Symplectic: this.stepSymplectic(); break;
            default:
                throw new System.Exception("[ERROR] Should never happen!");
        }

    }

    public void createNodes(List<Vector3> v)
    {
        for(int i = 0; i < v.Count; i++)
        {
            Vector3 pos = transform.TransformPoint(v[i]);

            Node newNode = new Node(pos, 0, this.Gravity);
            nodes.Add(newNode);
        }
    }

    public void createTetrahedrons(List<int> t)
    {
        for (int i = 0; i < t.Count - 3; i+= 4)
        {
            int vertex1 = t[i];
            int vertex2 = t[i + 1];
            int vertex3 = t[i + 2];
            int vertex4 = t[i + 3];

            Spring spring1 = new Spring(nodes[vertex1], nodes[vertex2], stiffness, damping);
            if (!springs.Contains(spring1)) springs.Add(spring1);
            Spring spring2 = new Spring(nodes[vertex1], nodes[vertex3], stiffness, damping);
            if (!springs.Contains(spring2)) springs.Add(spring2);
            Spring spring3 = new Spring(nodes[vertex1], nodes[vertex4], stiffness, damping);
            if (!springs.Contains(spring3)) springs.Add(spring3);
            Spring spring4 = new Spring(nodes[vertex2], nodes[vertex3], stiffness, damping);
            if (!springs.Contains(spring4)) springs.Add(spring4);
            Spring spring5 = new Spring(nodes[vertex4], nodes[vertex3], stiffness, damping);
            if (!springs.Contains(spring5)) springs.Add(spring5);
            Spring spring6 = new Spring(nodes[vertex2], nodes[vertex4], stiffness, damping);
            if (!springs.Contains(spring6)) springs.Add(spring6);

            Spring[] newSprings = { springs[springs.IndexOf(spring1)], springs[springs.IndexOf(spring2)], springs[springs.IndexOf(spring3)], springs[springs.IndexOf(spring4)], springs[springs.IndexOf(spring5)], springs[springs.IndexOf(spring6)] };

            this.tetrahedrons.Add(new Tetrahedron(nodes[vertex1], nodes[vertex2], nodes[vertex3], nodes[vertex4], newSprings));
        }
    }

    /**
    public void createSprings()
    {
        for(int i = 0; i < edges.Count/2; i++)
        {
            int j = i * 2;
            Spring newSpring = new Spring(nodes[e[j]], nodes[e[j + 1]], stiffness, this.damping);
            springs.Add(newSpring);
        }
    }
    /**/

    public List<Node> getNodes()
    {
        return this.nodes;
    }

    public List<Spring> getSprings()
    {
        return this.springs;
    }

    public void createBaricentricCoordinates(Vector3[] v, List<Tetrahedron> t)
    {
        for (int i = 0; i < v.Length; i++)
        {
            bool found = false;
            int counter = 0;
            while (!found && counter < t.Count)
            {
                if (t[counter].contains(v[i]))
                {
                    found = true;
                    this.tetrahedronsContainsVertex[i] = counter;
                    this.baricentricCoordinates[i] = computeBaricentricCoordinates(t[counter], v[i]);
                }
                counter++;
            }
        }
    }

    public Vector4 computeBaricentricCoordinates(Tetrahedron t, Vector3 v)
    {
        Vector4 result = new Vector4();
        float volume = t.volume;
        result[0] = (Mathf.Abs(Vector3.Dot(t.node2.pos - v, Vector3.Cross(t.node3.pos - v, t.node4.pos - v))) / 6) / volume;
        result[1] = (Mathf.Abs(Vector3.Dot(v - t.node1.pos, Vector3.Cross(t.node3.pos - t.node1.pos, t.node4.pos - t.node1.pos))) / 6) / volume;
        result[2] = (Mathf.Abs(Vector3.Dot(t.node2.pos - t.node1.pos, Vector3.Cross(v - t.node1.pos, t.node4.pos - t.node1.pos))) / 6) / volume;
        result[3] = (Mathf.Abs(Vector3.Dot(t.node2.pos - t.node1.pos, Vector3.Cross(t.node3.pos - t.node1.pos, v - t.node1.pos))) / 6) / volume;
        return result;
    }

    #endregion

    /// <summary>
    /// Performs a simulation step in 1D using Explicit integration.
    /// </summary>
    private void stepExplicit()
    {
        foreach (Node node in nodes)
        {
            node.force = Vector3.zero;
            node.ComputeForces();
        }
        foreach (Spring spring in springs)
        {
            spring.ComputeForces();
        }

        foreach (Node node in nodes)
        {
            if (!node.isFixed)
            {
                node.pos += TimeStep * node.vel;
                node.vel += TimeStep / node.mass * node.force;
            }
        }

        foreach (Spring spring in springs)
        {
            spring.UpdateLength();
        }

        for (int i = 0; i < this.modelVerticesHP.Length; i++)
        {
            Vector3 pos = baricentricCoordinates[i][0] * tetrahedrons[tetrahedronsContainsVertex[i]].node1.pos +
                baricentricCoordinates[i][1] * tetrahedrons[tetrahedronsContainsVertex[i]].node2.pos +
                baricentricCoordinates[i][2] * tetrahedrons[tetrahedronsContainsVertex[i]].node3.pos +
                baricentricCoordinates[i][3] * tetrahedrons[tetrahedronsContainsVertex[i]].node4.pos;

            this.modelVerticesHP[i] = transform.InverseTransformPoint(pos);
        }
        this.mesh.vertices = this.modelVerticesHP;

    }

    /// <summary>
	/// Performs a simulation step in 1D using Symplectic integration.
	/// </summary>
	private void stepSymplectic()
    {
        foreach (Node node in nodes)
        {
            node.force = Vector3.zero;
            node.ComputeForces();
        }

        foreach (Spring spring in springs)
        {
            spring.ComputeForces();
        }

        foreach (Node node in nodes)
        {
            if (!node.isFixed)
            {
                node.vel += TimeStep / node.mass * node.force;
                node.pos += TimeStep * node.vel;
            }
        }

        foreach (Spring spring in springs)
        {
            spring.UpdateLength();
        }

        for(int i = 0; i < this.modelVerticesHP.Length; i++){
            Vector3 pos = baricentricCoordinates[i][0] * tetrahedrons[tetrahedronsContainsVertex[i]].node1.pos +
                baricentricCoordinates[i][1] * tetrahedrons[tetrahedronsContainsVertex[i]].node2.pos +
                baricentricCoordinates[i][2] * tetrahedrons[tetrahedronsContainsVertex[i]].node3.pos +
                baricentricCoordinates[i][3] * tetrahedrons[tetrahedronsContainsVertex[i]].node4.pos;

            this.modelVerticesHP[i] = transform.InverseTransformPoint(pos);
        }
        this.mesh.vertices = this.modelVerticesHP;

    }
}
