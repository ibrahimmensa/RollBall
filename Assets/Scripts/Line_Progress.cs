using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace PathCreation.Examples
{
    public class Line_Progress : PathSceneTool
    {

        public float roadWidth;
        public bool flattenSurface;
        public float thickness;

         Mesh mesh;
        public Transform ballParent;

        public RoadMeshCreator roadMeshCreator;

        public MeshFilter newmesh;
        public int _numPointsRendered;
        int numPointsRendered
        {
            get
            {
                return _numPointsRendered;
            }
            
            set
            {
                _numPointsRendered = value;
                UIManager.UI.UpdateProgress(value, path.NumPoints);
            }
        }

        public List<Vector3> pathPoints;

        void Start()
        {
            roadWidth = roadMeshCreator.roadWidth;
            flattenSurface = roadMeshCreator.flattenSurface;
            thickness = roadMeshCreator.thickness;

            mesh = GetComponent<MeshFilter>().sharedMesh;

            transform.position = roadMeshCreator.meshHolder.transform.position + new Vector3(0f, 0.01f, 0f);

            CreateRoadMesh();

            pathPoints = new List<Vector3>();
            for (int i = 0; i < path.NumPoints; i++)
                pathPoints.Add(path.GetPoint(i));

            GameSystem.Sytem.LEVEL.OnLevelResart += LEVEL_OnLevelResart;

            setMesh();
        }

        private void LEVEL_OnLevelResart()
        {
            numPointsRendered = 1;

            mesh.Clear();
            mesh.vertices = verts.Take(1 * 8).ToArray();
            mesh.normals = normals.Take(1 * 8).ToArray();
            mesh.subMeshCount = 3;
            mesh.SetTriangles(roadTriangles.Take((2 * (1 - 1)) * 3).ToArray(), 0);
            mesh.RecalculateBounds();
        }

        protected override void PathUpdated()
        {

        }

        void FixedUpdate()
        {
            if (GameSystem.Sytem.RunningBall.GetComponent<Ball>().isMoving)
            {
                setMesh();
            }

            /*DrawDebugTools.Instance.Log("Points TOTAL : " + path.NumPoints, Color.yellow);
            DrawDebugTools.Instance.Log("Current Points Rendered : " + numPointsRendered, Color.yellow);
            DrawDebugTools.Instance.Log("point from pathpoints : " + pathPoints[numPointsRendered], Color.yellow);
            DrawDebugTools.Instance.Log("contains ball current point : " + pathPoints.Contains(ball.currentPoint), Color.yellow);*/

        }

        Vector3[] verts;
        Vector3[] normals;
        int[] roadTriangles;

        Vector3[] vertsTemp;
        Vector3[] normalsTemp;
        int[] roadTrianglesTemp;

        int numTris;
        int[] underRoadTriangles;
        int[] sideOfRoadTriangles;

        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

        Vector3 localUp;
        Vector3 localRight;

        // Find position to left and right of current path vertex
        Vector3 vertSideA;
        Vector3 vertSideB;

        public int index;
        public List<Vector3> i;

        public void setMesh()
        {

            i = pathPoints.FindAll(d => Vector3.Distance(ballParent.position, d) <= 0.30f);

            /*foreach(Vector3 v in i)
            {
                Debug.Log(Vector3.Distance(ballParent.position, v) + " - " + pathPoints.FindIndex(d => d == v));
            }*/

            foreach(Vector3 v in i)
            {
                index = pathPoints.FindIndex(d => d == v);
                //numPointsRendered = index;

                if (index == numPointsRendered + 1)
                {
                    numPointsRendered = index;
                }
            }

            mesh.Clear();
            mesh.vertices = verts.Take(numPointsRendered * 8).ToArray();
            mesh.normals = normals.Take(numPointsRendered * 8).ToArray();
            mesh.subMeshCount = 3;
            mesh.SetTriangles(roadTriangles.Take((2 * (numPointsRendered - 1)) * 3).ToArray(), 0);
            mesh.RecalculateBounds();
        }

        void CreateRoadMesh()
        {
            verts = new Vector3[path.NumPoints * 8];
            normals = new Vector3[verts.Length];

            numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
            roadTriangles = new int[numTris * 3];

            int vertIndex = 0;
            int triIndex = 0;

            // Vertices for the top of the road are layed out:
            // 0  1
            // 8  9
            // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.

            bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

            for (int i = 0; i < path.NumPoints; i++)
            {
                //Debug.Log(Vector3.Distance(ball.currentPoint, path.GetPoint(i)));
                localUp = (usePathNormals) ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
                localRight = (usePathNormals) ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

                // Find position to left and right of current path vertex
                vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth);
                vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth);

                // Add top of road vertices
                verts[vertIndex + 0] = vertSideA;
                verts[vertIndex + 1] = vertSideB;
                // Add bottom of road vertices
                verts[vertIndex + 2] = vertSideA - localUp * thickness;
                verts[vertIndex + 3] = vertSideB - localUp * thickness;

                // Duplicate vertices to get flat shading for sides of road
                verts[vertIndex + 4] = verts[vertIndex + 0];
                verts[vertIndex + 5] = verts[vertIndex + 1];
                verts[vertIndex + 6] = verts[vertIndex + 2];
                verts[vertIndex + 7] = verts[vertIndex + 3];

                // Top of road normals
                normals[vertIndex + 0] = localUp;
                normals[vertIndex + 1] = localUp;

                // Set triangle indices
                if (i < path.NumPoints - 1 || path.isClosedLoop)
                {
                    for (int j = 0; j < triangleMap.Length; j++)
                    {
                        roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                    }

                }

                vertIndex += 8;
                triIndex += 6;
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.subMeshCount = 3;
            mesh.SetTriangles(roadTriangles, 0);
            mesh.RecalculateBounds();
        }
    }
}
