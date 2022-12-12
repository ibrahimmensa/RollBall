﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DDTExamples
{
	public class DrawMultipleShapesExample : MonoBehaviour
	{

		#region ========== Variables ==========
		private float m_GridSize = 6.0f;
		private Vector3 m_Position;
		private float m_Time = 0.0f;
		private float m_DrawSphereGhostTime = 0.2f;
		private float m_DrawSphereTrailTimeCounter = 0.0f;
		private Vector3 m_DrawSphereTrailLastPos;
		#endregion

		#region ========== Functions ==========
		void Update()
		{


			// Draw grid
			DrawDebugTools.Instance.DrawGrid(transform.position, m_GridSize, 1.0f, 0.0f);

			// Increate time
			m_Time += Time.deltaTime;

			m_Position = transform.position + new Vector3(Mathf.Sin(m_Time) * 2.0f, 2.0f + Mathf.Sin(m_Time * 3.0f), Mathf.Cos(m_Time) * 2.0f);
			DrawDebugTools.Instance.DrawSphere(m_Position, 0.4f, 4, Color.green);

			// Draw sphere trail
			m_DrawSphereTrailTimeCounter += Time.deltaTime;
			if (m_DrawSphereTrailTimeCounter > m_DrawSphereGhostTime)
			{
				DrawDebugTools.Instance.DrawSphere(m_Position, 0.2f, 4, Color.yellow, 1.0f);
				DrawDebugTools.Instance.DrawLine(m_Position, m_DrawSphereTrailLastPos, Color.red, 1.0f);
				m_DrawSphereTrailTimeCounter = 0.0f;
				m_DrawSphereTrailLastPos = m_Position;
			}

			// Draw sphere arroes
			DrawDebugTools.Instance.DrawDirectionalArrow(transform.position, m_Position, 0.1f, Color.magenta);
			DrawDebugTools.Instance.DrawDirectionalArrow(transform.position, Vector3.ProjectOnPlane(m_Position, Vector3.up), 0.1f, Color.magenta);

			// Draw distance
			DrawDebugTools.Instance.DrawDistance(m_Position, Vector3.ProjectOnPlane(m_Position, Vector3.up), Color.blue);

			// Draw float graph
			DrawDebugTools.Instance.DrawFloatGraph("Sin Value", Mathf.Sin(m_Time * 3.0f), 1.0f, true, 100);
			DrawDebugTools.Instance.DrawFloatGraph("Sin Value", Mathf.Sin(m_Time * 3.0f), 1.0f, true, 100);

			// Log delta time
			DrawDebugTools.Instance.Log("Delta time = " + Time.deltaTime, Color.white);
		}
		#endregion
	}
}
