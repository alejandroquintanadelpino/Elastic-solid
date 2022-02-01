using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Parser : MonoBehaviour {

    public TextAsset elem;
    public TextAsset node;
    public TextAsset edge;
    List<int> tetrahedrons;
    List<Vector3> vertices;
    List<int> modelEdges;

    public List<Vector3> getVertices()
    {
        return this.vertices;
    }

    public List<int> getTetrahedrons()
    {
        return this.tetrahedrons;
    }

    public List<int> getEdges()
    {
        return this.modelEdges;
    }

	public void ParseFiles () {
        vertices = new List<Vector3> { };
        tetrahedrons = new List<int> { };
        modelEdges = new List<int> { };

        string[] elems = elem.text.Split(new string[] { " ", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        string[] nodes = node.text.Split(new string[] { " ", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        string[] edges = edge.text.Split(new string[] { " ", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        int numElems = int.Parse(elems[0]);
        int numNodes = int.Parse(nodes[0]);
        int numEdges = int.Parse(edges[0]);

        for (int i = 3; i < numElems * 5 + 3; i+= 5)
        {
            int index = int.Parse(elems[i]);
            for (int j = i + 1; j < i + 5; j++)
            {
                tetrahedrons.Add(int.Parse(elems[j]));
            }
        }

        for (int i = 4; i < numNodes * 4 + 4; i+= 4)
        {
            int index = int.Parse(nodes[i]);
            vertices.Add(new Vector3(float.Parse(nodes[i + 1], CultureInfo.InvariantCulture), float.Parse(nodes[i + 2], CultureInfo.InvariantCulture), float.Parse(nodes[i + 3], CultureInfo.InvariantCulture)));
        }

        for (int i = 2; i < numEdges * 4 + 2; i += 4)
        {
            int index = int.Parse(edges[i]);
            for (int j = i + 1; j < i + 3; j++)
            {
                modelEdges.Add(int.Parse(edges[j]));
            }
        }
    }

}
